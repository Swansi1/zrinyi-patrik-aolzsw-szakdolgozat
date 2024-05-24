namespace redmineGUI.Models;

using Newtonsoft.Json;
using System.Collections.Generic;

public class RedmineIssueStatus
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsClosed { get; set; }
    public bool IsDefault { get; set; }
}

public class RedmineIssueStatusesResponse
{
    [JsonProperty("issue_statuses")]
    public List<RedmineIssueStatus> IssueStatuses { get; set; }
}