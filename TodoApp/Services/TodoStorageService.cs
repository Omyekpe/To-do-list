using System.Text.Json;
using TodoApp.Models;

namespace TodoApp.Services;

public class TodoStorageService
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private TodoData _data;

    public TodoStorageService(IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, "data", "todos.json");
        _data = Load();
    }

    private TodoData Load()
    {
        if (!File.Exists(_filePath))
        {
            return new TodoData();
        }

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<TodoData>(json) ?? new TodoData();
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    // Projects

    public List<ProjectItem> GetProjects() => _data.Projects;

    public ProjectItem? GetProject(int id) => _data.Projects.FirstOrDefault(p => p.Id == id);

    public void AddProject(string name)
    {
        lock (_lock)
        {
            var nextId = _data.Projects.Count == 0 ? 1 : _data.Projects.Max(p => p.Id) + 1;
            _data.Projects.Add(new ProjectItem { Id = nextId, Name = name });
            Save();
        }
    }

    public void RenameProject(int id, string name)
    {
        lock (_lock)
        {
            var project = GetProject(id);
            if (project is null) return;

            project.Name = name;
            Save();
        }
    }

    public void DeleteProject(int id)
    {
        lock (_lock)
        {
            var taskIds = GetTasks(id).Select(t => t.Id).ToList();
            foreach (var taskId in taskIds)
            {
                RemoveTaskAndDescendants(taskId);
            }

            _data.Projects.RemoveAll(p => p.Id == id);
            Save();
        }
    }

    // Tasks

    public List<TaskItem> GetTasks(int projectId) =>
        _data.Tasks.Where(t => t.ProjectId == projectId).OrderBy(t => t.Order).ToList();

    public TaskItem? GetTask(int id) => _data.Tasks.FirstOrDefault(t => t.Id == id);

    public void AddTask(int projectId, string name)
    {
        lock (_lock)
        {
            var siblings = GetTasks(projectId);
            var nextId = _data.Tasks.Count == 0 ? 1 : _data.Tasks.Max(t => t.Id) + 1;
            var nextOrder = siblings.Count == 0 ? 1 : siblings.Max(t => t.Order) + 1;
            _data.Tasks.Add(new TaskItem { Id = nextId, ProjectId = projectId, Name = name, Order = nextOrder });
            Save();
        }
    }

    public void ToggleTaskDone(int id)
    {
        lock (_lock)
        {
            var task = GetTask(id);
            if (task is null) return;

            // Only a leaf Task (no Subtasks yet) can be toggled by hand.
            if (GetSubtasks(task.Id).Count == 0)
            {
                task.IsDone = !task.IsDone;
                Save();
            }
        }
    }

    public bool IsTaskLocked(TaskItem task)
    {
        var siblings = GetTasks(task.ProjectId);
        var index = siblings.FindIndex(t => t.Id == task.Id);
        return siblings.Take(index).Any(t => !t.IsDone);
    }

    public void MoveTaskUp(int taskId) => MoveTask(taskId, -1);
    public void MoveTaskDown(int taskId) => MoveTask(taskId, 1);

    private void MoveTask(int taskId, int direction)
    {
        lock (_lock)
        {
            var task = GetTask(taskId);
            if (task is null) return;

            var siblings = GetTasks(task.ProjectId);
            var index = siblings.FindIndex(t => t.Id == taskId);
            var swapIndex = index + direction;
            if (swapIndex < 0 || swapIndex >= siblings.Count) return;

            (siblings[index].Order, siblings[swapIndex].Order) = (siblings[swapIndex].Order, siblings[index].Order);
            Save();
        }
    }

    public void RenameTask(int id, string name)
    {
        lock (_lock)
        {
            var task = GetTask(id);
            if (task is null) return;

            task.Name = name;
            Save();
        }
    }

    public void DeleteTask(int id)
    {
        lock (_lock)
        {
            RemoveTaskAndDescendants(id);
            Save();
        }
    }

    // Removes a Task plus every Subtask and Step nested under it, without locking or saving —
    // callers (DeleteTask, DeleteProject) already hold the lock and save once at the end.
    private void RemoveTaskAndDescendants(int taskId)
    {
        var subtaskIds = GetSubtasks(taskId).Select(s => s.Id).ToList();
        foreach (var subtaskId in subtaskIds)
        {
            RemoveSubtaskAndDescendants(subtaskId);
        }

        _data.Tasks.RemoveAll(t => t.Id == taskId);
    }

    // Subtasks

    public List<SubtaskItem> GetSubtasks(int taskId) =>
        _data.Subtasks.Where(s => s.TaskId == taskId).OrderBy(s => s.Order).ToList();

    public SubtaskItem? GetSubtask(int id) => _data.Subtasks.FirstOrDefault(s => s.Id == id);

    public void AddSubtask(int taskId, string name)
    {
        lock (_lock)
        {
            var siblings = GetSubtasks(taskId);
            var nextId = _data.Subtasks.Count == 0 ? 1 : _data.Subtasks.Max(s => s.Id) + 1;
            var nextOrder = siblings.Count == 0 ? 1 : siblings.Max(s => s.Order) + 1;
            _data.Subtasks.Add(new SubtaskItem { Id = nextId, TaskId = taskId, Name = name, Order = nextOrder });
            RecomputeTaskDone(taskId);
            Save();
        }
    }

    public void ToggleSubtaskDone(int id)
    {
        lock (_lock)
        {
            var subtask = GetSubtask(id);
            if (subtask is null) return;

            // Only a leaf Subtask (no Steps yet) can be toggled by hand.
            if (GetSteps(subtask.Id).Count == 0)
            {
                subtask.IsDone = !subtask.IsDone;
            }

            RecomputeTaskDone(subtask.TaskId);
            Save();
        }
    }

    public bool IsSubtaskLocked(SubtaskItem subtask)
    {
        var siblings = GetSubtasks(subtask.TaskId);
        var index = siblings.FindIndex(s => s.Id == subtask.Id);
        return siblings.Take(index).Any(s => !s.IsDone);
    }

    public void MoveSubtaskUp(int subtaskId) => MoveSubtask(subtaskId, -1);
    public void MoveSubtaskDown(int subtaskId) => MoveSubtask(subtaskId, 1);

    private void MoveSubtask(int subtaskId, int direction)
    {
        lock (_lock)
        {
            var subtask = GetSubtask(subtaskId);
            if (subtask is null) return;

            var siblings = GetSubtasks(subtask.TaskId);
            var index = siblings.FindIndex(s => s.Id == subtaskId);
            var swapIndex = index + direction;
            if (swapIndex < 0 || swapIndex >= siblings.Count) return;

            (siblings[index].Order, siblings[swapIndex].Order) = (siblings[swapIndex].Order, siblings[index].Order);
            Save();
        }
    }

    public void RenameSubtask(int id, string name)
    {
        lock (_lock)
        {
            var subtask = GetSubtask(id);
            if (subtask is null) return;

            subtask.Name = name;
            Save();
        }
    }

    public void DeleteSubtask(int id)
    {
        lock (_lock)
        {
            var subtask = GetSubtask(id);
            if (subtask is null) return;

            var taskId = subtask.TaskId;
            RemoveSubtaskAndDescendants(id);
            RecomputeTaskDone(taskId);
            Save();
        }
    }

    // Removes a Subtask plus every Step nested under it, without locking or saving —
    // callers already hold the lock and save once at the end.
    private void RemoveSubtaskAndDescendants(int subtaskId)
    {
        _data.Steps.RemoveAll(s => s.SubtaskId == subtaskId);
        _data.Subtasks.RemoveAll(s => s.Id == subtaskId);
    }

    // Steps

    public List<StepItem> GetSteps(int subtaskId) =>
        _data.Steps.Where(s => s.SubtaskId == subtaskId).OrderBy(s => s.Order).ToList();

    public StepItem? GetStep(int id) => _data.Steps.FirstOrDefault(s => s.Id == id);

    public void AddStep(int subtaskId, string name)
    {
        lock (_lock)
        {
            var siblings = GetSteps(subtaskId);
            var nextId = _data.Steps.Count == 0 ? 1 : _data.Steps.Max(s => s.Id) + 1;
            var nextOrder = siblings.Count == 0 ? 1 : siblings.Max(s => s.Order) + 1;
            _data.Steps.Add(new StepItem { Id = nextId, SubtaskId = subtaskId, Name = name, Order = nextOrder });
            RecomputeSubtaskDone(subtaskId);
            Save();
        }
    }

    public void ToggleStepDone(int id)
    {
        lock (_lock)
        {
            var step = GetStep(id);
            if (step is null) return;

            step.IsDone = !step.IsDone;
            RecomputeSubtaskDone(step.SubtaskId);
            Save();
        }
    }

    public bool IsStepLocked(StepItem step)
    {
        var siblings = GetSteps(step.SubtaskId);
        var index = siblings.FindIndex(s => s.Id == step.Id);
        return siblings.Take(index).Any(s => !s.IsDone);
    }

    public void MoveStepUp(int stepId) => MoveStep(stepId, -1);
    public void MoveStepDown(int stepId) => MoveStep(stepId, 1);

    private void MoveStep(int stepId, int direction)
    {
        lock (_lock)
        {
            var step = GetStep(stepId);
            if (step is null) return;

            var siblings = GetSteps(step.SubtaskId);
            var index = siblings.FindIndex(s => s.Id == stepId);
            var swapIndex = index + direction;
            if (swapIndex < 0 || swapIndex >= siblings.Count) return;

            (siblings[index].Order, siblings[swapIndex].Order) = (siblings[swapIndex].Order, siblings[index].Order);
            Save();
        }
    }

    public void RenameStep(int id, string name)
    {
        lock (_lock)
        {
            var step = GetStep(id);
            if (step is null) return;

            step.Name = name;
            Save();
        }
    }

    public void DeleteStep(int id)
    {
        lock (_lock)
        {
            var step = GetStep(id);
            if (step is null) return;

            var subtaskId = step.SubtaskId;
            _data.Steps.RemoveAll(s => s.Id == id);
            RecomputeSubtaskDone(subtaskId);
            Save();
        }
    }

    // Cascade: a parent with children is "done" exactly when all its children are done.
    // Called after any Step/Subtask add or toggle, before Save().

    private void RecomputeSubtaskDone(int subtaskId)
    {
        var subtask = GetSubtask(subtaskId);
        if (subtask is null) return;

        var steps = GetSteps(subtaskId);
        if (steps.Count > 0)
        {
            subtask.IsDone = steps.All(s => s.IsDone);
        }

        RecomputeTaskDone(subtask.TaskId);
    }

    private void RecomputeTaskDone(int taskId)
    {
        var task = GetTask(taskId);
        if (task is null) return;

        var subtasks = GetSubtasks(taskId);
        if (subtasks.Count > 0)
        {
            task.IsDone = subtasks.All(s => s.IsDone);
        }
    }
}
