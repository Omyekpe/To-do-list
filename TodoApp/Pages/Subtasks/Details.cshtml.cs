using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Pages.Subtasks;

public class DetailsModel : PageModel
{
    private readonly TodoStorageService _storage;

    public DetailsModel(TodoStorageService storage)
    {
        _storage = storage;
    }

    public SubtaskItem? Subtask { get; set; }
    public List<StepItem> Steps { get; set; } = new();

    [BindProperty]
    public string NewStepName { get; set; } = "";

    public bool IsLocked(StepItem step) => _storage.IsStepLocked(step);

    public int DoneStepCount => Steps.Count(s => s.IsDone);
    public int StepCount => Steps.Count;

    public IActionResult OnGet(int id)
    {
        Subtask = _storage.GetSubtask(id);
        if (Subtask is null)
        {
            return NotFound();
        }

        // A locked Subtask can't be worked on yet — send the user back to finish the one before it.
        if (_storage.IsSubtaskLocked(Subtask))
        {
            return RedirectToPage("/Tasks/Details", new { id = Subtask.TaskId });
        }

        Steps = _storage.GetSteps(id);
        return Page();
    }

    public IActionResult OnPostAddStep(int id)
    {
        if (!string.IsNullOrWhiteSpace(NewStepName))
        {
            _storage.AddStep(id, NewStepName);
        }

        return RedirectToPage(new { id });
    }

    public IActionResult OnPostToggleStep(int id, int stepId)
    {
        _storage.ToggleStepDone(stepId);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostMoveStepUp(int id, int stepId)
    {
        _storage.MoveStepUp(stepId);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostMoveStepDown(int id, int stepId)
    {
        _storage.MoveStepDown(stepId);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostRenameStep(int id, int stepId, string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _storage.RenameStep(stepId, name);
        }

        return RedirectToPage(new { id });
    }

    public IActionResult OnPostDeleteStep(int id, int stepId)
    {
        _storage.DeleteStep(stepId);
        return RedirectToPage(new { id });
    }
}
