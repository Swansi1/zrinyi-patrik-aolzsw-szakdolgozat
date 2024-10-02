namespace redmineGUI.Models;

using Newtonsoft.Json;
using System.Collections.Generic;

public class RedmineTrackers
{
    public int Id { get; set; }
    public string Name { get; set; }
    public RedmineIssueStatus Default_status { get; set; }
    public string Description { get; set; }
    public List<string> Enabled_standard_fields { get; set; }
}

public class RedmineTrackersResponse
{
    [JsonProperty("trackers")]
    public List<RedmineTrackers> Trackers { get; set; }
}