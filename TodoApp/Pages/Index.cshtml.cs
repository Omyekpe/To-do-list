using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Pages;

public class IndexModel : PageModel
{
    private readonly TodoStorageService _storage;

    public IndexModel(TodoStorageService storage)
    {
        _storage = storage;
    }

    public List<ProjectItem> Projects { get; set; } = new();

    [BindProperty]
    public string NewProjectName { get; set; } = "";

    public void OnGet()
    {
        Projects = _storage.GetProjects();
    }

    public IActionResult OnPost()
    {
        if (!string.IsNullOrWhiteSpace(NewProjectName))
        {
            _storage.AddProject(NewProjectName);
        }

        return RedirectToPage();
    }
}
