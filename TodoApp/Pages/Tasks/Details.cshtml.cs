using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Pages.Tasks;

public class DetailsModel : PageModel
{
    private readonly TodoStorageService _storage;

    public DetailsModel(TodoStorageService storage)
    {
        _storage = storage;
    }

    public TaskItem? Task { get; set; }
    public List<SubtaskItem> Subtasks { get; set; } = new();

    [BindProperty]
    public string NewSubtaskName { get; set; } = "";

    public bool IsLocked(SubtaskItem subtask) => _storage.IsSubtaskLocked(subtask);
    public bool HasSteps(SubtaskItem subtask) => _storage.GetSteps(subtask.Id).Count > 0;

    public int DoneSubtaskCount => Subtasks.Count(s => s.IsDone);
    public int SubtaskCount => Subtasks.Count;

    public IActionResult OnGet(int id)
    {
        Task = _storage.GetTask(id);
        if (Task is null)
        {
            return NotFound();
        }

        // A locked Task can't be worked on yet — send the user back to finish the one before it.
        if (_storage.IsTaskLocked(Task))
        {
            return RedirectToPage("/Projects/Details", new { id = Task.ProjectId });
        }

        Subtasks = _storage.GetSubtasks(id);
        return Page();
    }

    public IActionResult OnPostAddSubtask(int id)
    {
        if (!string.IsNullOrWhiteSpace(NewSubtaskName))
        {
            _storage.AddSubtask(id, NewSubtaskName);
        }

        return RedirectToPage(new { id });
    }

    public IActionResult OnPostToggleSubtask(int id, int subtaskId)
    {
        _storage.ToggleSubtaskDone(subtaskId);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostMoveSubtaskUp(int id, int subtaskId)
    {
        _storage.MoveSubtaskUp(subtaskId);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostMoveSubtaskDown(int id, int subtaskId)
    {
        _storage.MoveSubtaskDown(subtaskId);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostRenameSubtask(int id, int subtaskId, string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _storage.RenameSubtask(subtaskId, name);
        }

        return RedirectToPage(new { id });
    }

    public IActionResult OnPostDeleteSubtask(int id, int subtaskId)
    {
        _storage.DeleteSubtask(subtaskId);
        return RedirectToPage(new { id });
    }
}
