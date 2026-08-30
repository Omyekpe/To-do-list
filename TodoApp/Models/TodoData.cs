namespace TodoApp.Models;

public class TodoData
{
    public List<ProjectItem> Projects { get; set; } = new();
    public List<TaskItem> Tasks { get; set; } = new();
    public List<SubtaskItem> Subtasks { get; set; } = new();
    public List<StepItem> Steps { get; set; } = new();
}
