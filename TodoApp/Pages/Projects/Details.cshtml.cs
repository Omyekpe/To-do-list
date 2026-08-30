using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Pages.Projects;

public class DetailsModel : PageModel
{
    private readonly TodoStorageService _storage;

    public DetailsModel(TodoStorageService storage)
    {
        _storage = storage;
    }

    public ProjectItem? Project { get; set; }
    public List<TaskItem> Tasks { get; set; } = new();

    [BindProperty]
    public string NewTaskName { get; set; } = "";

    public bool IsLocked(TaskItem task) => _storage.IsTaskLocked(task);
    public bool HasSubtasks(TaskItem task) => _storage.GetSubtasks(task.Id).Count > 0;

    public IActionResult OnGet(int id)
    {
        Project = _storage.GetProject(id);
        if (Project is null)
        {
            return NotFound();
        }

        Tasks = _storage.GetTasks(id);
        return Page();
    }

    public IActionResult OnPostAddTask(int id)
    {
        if (!string.IsNullOrWhiteSpace(NewTaskName))
        {
            _storage.AddTask(id, NewTaskName);
        }

        return RedirectToPage(new { id });
    }

    public IActionResult OnPostToggleTask(int id, int taskId)
    {
        _storage.ToggleTaskDone(taskId);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostMoveTaskUp(int id, int taskId)
    {
        _storage.MoveTaskUp(taskId);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostMoveTaskDown(int id, int taskId)
    {
        _storage.MoveTaskDown(taskId);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostRenameTask(int id, int taskId, string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _storage.RenameTask(taskId, name);
        }

        return RedirectToPage(new { id });
    }

    public IActionResult OnPostDeleteTask(int id, int taskId)
    {
        _storage.DeleteTask(taskId);
        return RedirectToPage(new { id });
    }
}
