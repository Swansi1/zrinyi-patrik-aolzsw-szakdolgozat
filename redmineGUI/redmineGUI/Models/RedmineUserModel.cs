namespace redmineGUI.Models;

using Newtonsoft.Json;
using System.Collections.Generic;

public class RedmineUser
{
    public int Id { get; set; }
    public string Login { get; set; }
    public bool Admin { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Mail { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime? LastLoginOn { get; set; }
    public DateTime? PasswdChangedOn { get; set; }
    public string TwofaScheme { get; set; }
}

public class RedmineUserResponse
{
    [JsonProperty("users")]
    public List<RedmineUser> Users { get; set; }
    
    [JsonProperty("User")]
    public RedmineUser User { get; set; }
    
    [JsonProperty("offset")]
    public int Offset { get; set; }

    [JsonProperty("limit")]
    public int Limit { get; set; }

    [JsonProperty("total_count")]
    public int TotalCount { get; set; }
}
