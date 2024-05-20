namespace redmineGUI.Models;

using Newtonsoft.Json;
using System.Collections.Generic;


public class IssuesResponse
{
    [JsonProperty("issues")]
    public List<RedmineIssue> Issues { get; set; }
}