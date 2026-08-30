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

    // Tasks

    public List<TaskItem> GetTasks(int projectId) =>
        _data.Tasks.Where(t => t.ProjectId == projectId).ToList();

    public TaskItem? GetTask(int id) => _data.Tasks.FirstOrDefault(t => t.Id == id);

    public void AddTask(int projectId, string name)
    {
        lock (_lock)
        {
            var nextId = _data.Tasks.Count == 0 ? 1 : _data.Tasks.Max(t => t.Id) + 1;
            _data.Tasks.Add(new TaskItem { Id = nextId, ProjectId = projectId, Name = name });
            Save();
        }
    }

    public void ToggleTaskDone(int id)
    {
        lock (_lock)
        {
            var task = GetTask(id);
            if (task is not null)
            {
                task.IsDone = !task.IsDone;
                Save();
            }
        }
    }

    // Subtasks

    public List<SubtaskItem> GetSubtasks(int taskId) =>
        _data.Subtasks.Where(s => s.TaskId == taskId).ToList();

    public void AddSubtask(int taskId, string name)
    {
        lock (_lock)
        {
            var nextId = _data.Subtasks.Count == 0 ? 1 : _data.Subtasks.Max(s => s.Id) + 1;
            _data.Subtasks.Add(new SubtaskItem { Id = nextId, TaskId = taskId, Name = name });
            Save();
        }
    }

    public void ToggleSubtaskDone(int id)
    {
        lock (_lock)
        {
            var subtask = _data.Subtasks.FirstOrDefault(s => s.Id == id);
            if (subtask is not null)
            {
                subtask.IsDone = !subtask.IsDone;
                Save();
            }
        }
    }
}
