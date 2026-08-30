namespace TodoApp.Models;

public class SubtaskItem
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string Name { get; set; } = "";
    public bool IsDone { get; set; }
}
