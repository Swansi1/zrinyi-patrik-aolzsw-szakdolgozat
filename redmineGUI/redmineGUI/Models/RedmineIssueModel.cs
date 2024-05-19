namespace redmineGUI.Models;

public class RedmineIssue
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public int AssignedToId { get; set; }
    public string Status { get; set; }
}