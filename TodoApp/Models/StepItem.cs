namespace TodoApp.Models;

public class StepItem
{
    public int Id { get; set; }
    public int SubtaskId { get; set; }
    public string Name { get; set; } = "";
    public bool IsDone { get; set; }
    public int Order { get; set; }
}
