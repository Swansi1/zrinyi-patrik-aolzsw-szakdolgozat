namespace redmineGUI.Models;

using Newtonsoft.Json;

public class RedmineIssue
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public int AssignedToId { get; set; }

    [JsonProperty("status")]
    public Status Status { get; set; }
}

    public class Status
{
    public int Id { get; set; }
    public string Name { get; set; }
}