namespace redmineGUI.Models;

using Newtonsoft.Json;

public class RedmineIssue
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public int AssignedToId { get; set; }

    [JsonProperty("status")]
    public RedmineIssueStatus Status { get; set; }
}
