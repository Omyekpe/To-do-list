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

    public IActionResult OnGet(int id)
    {
        Task = _storage.GetTask(id);
        if (Task is null)
        {
            return NotFound();
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
}
