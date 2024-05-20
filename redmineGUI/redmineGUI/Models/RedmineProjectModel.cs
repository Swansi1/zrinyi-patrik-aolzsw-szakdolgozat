namespace redmineGUI.Models;

using Newtonsoft.Json;
using System.Collections.Generic;

public class RedmineProject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Identifier { get; set; }
    public string Description { get; set; }
}

public class RedmineProjectsResponse
{
    [JsonProperty("projects")]
    public List<RedmineProject> Projects { get; set; }
}