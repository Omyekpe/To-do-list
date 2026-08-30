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

    public IActionResult OnPostRenameProject(int projectId, string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _storage.RenameProject(projectId, name);
        }

        return RedirectToPage();
    }

    public IActionResult OnPostDeleteProject(int projectId)
    {
        _storage.DeleteProject(projectId);
        return RedirectToPage();
    }
}
