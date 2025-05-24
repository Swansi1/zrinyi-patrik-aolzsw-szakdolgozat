using Microsoft.AspNetCore.Mvc;
using redmineGUI.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO.Compression;
using System.IO;
using System.Text.Json;

namespace redmineGUI.Controllers;

public class MigrationController : Controller
{
    private static ApiKeyModel _apiKeyModel;
    private static List<int> _redmineProjectIds = new List<int>();
    private static List<int> _redmineUsersId = new List<int>();
    private static List<List<int>> _redmineStatusId = new List<List<int>>();
    private static List<List<int>> _redmineUsersMap = new List<List<int>>();
    private static List<List<int>> _redmineTrackerId = new List<List<int>>();

    private static RedmineApiController _redmineApiController;

    public async Task<IActionResult> Index()
    {
        var issues = await GetIssuesAsync();

        return View(issues);
    }

    [HttpGet]
    public IActionResult StepContent(int step)
    {
        switch (step)
        {
            case 1:
                return PartialView("_Step1");
            case 2:
                return PartialView("_Step2");
            case 3:
                return PartialView("_Step3");
            case 4:
                return PartialView("_Step4");
            case 5:
                return PartialView("_Step5");
            case 6:
                return PartialView("_Step6");
            case 7:
                return PartialView("_Step7");
            default:
                return PartialView("_DefaultStep");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveApiKey([FromBody] ApiKeyModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(kvp => kvp.Value.Errors.Any())
                .ToDictionary(
                    kvp => kvp.Key.Split('.').Last(),
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new { success = false, errors });
        }

        _apiKeyModel = model;

        var exportServer = new RedmineServerModel
        {
            ApiKey = _apiKeyModel.ApiKeyExport,
            BaseUrl = _apiKeyModel.BaseUrlExport,
        };

        var importServer = new RedmineServerModel
        {
            ApiKey = _apiKeyModel.ApiKeyImport,
            BaseUrl = _apiKeyModel.BaseUrlImport,
        };

        _redmineApiController = new RedmineApiController(exportServer, importServer);

        return Json(new { success = true, message = "API keys saved successfully.", apikeyModel= _apiKeyModel.ApiKeyExport });
    }

    [HttpPost]
    public IActionResult SaveProjects([FromBody] RedmineSelectedProjects model)
    {
        if (model?.ProjectIds == null || model.ProjectIds.Count == 0)
        {
            return BadRequest(model.ProjectIds);
        }

        _redmineProjectIds.Clear();
        _redmineProjectIds.AddRange(model.ProjectIds);

        return Ok(new { success = true, message = "Selected projects saved successfully." });
    }

    [HttpPost]
    public IActionResult SaveUsers([FromBody] RedmineSelectedUsersModel model)
    {
        if (model?.UserIds == null || model.UserIds.Count == 0)
        {
            return BadRequest(model.UserIds);
        }

        _redmineUsersId.Clear();
        _redmineUsersId.AddRange(model.UserIds);

        return Ok(new { success = true, message = "Selected users saved successfully." });
    }

    [HttpPost]
    public IActionResult SaveStatus([FromBody] RedmineSelectedStatusModel model)
    {
        if (model?.StatusIds == null || model.StatusIds.Count == 0)
        {
            return BadRequest(model.StatusIds);
        }

        _redmineStatusId.Clear();
        _redmineStatusId = model.StatusIds;

        return Ok(new {success = true, message = "Selected statuses saved successfully."});
    }
    
    [HttpPost]
    public IActionResult SaveTracker([FromBody] RedmineSelectedTrackerModel model)
    {
        if (model?.TrackerIds == null || model.TrackerIds.Count == 0)
        {
            return BadRequest(model.TrackerIds);
        }

        _redmineTrackerId.Clear();
        _redmineTrackerId = model.TrackerIds;

        return Ok(new {success = true, message = "Selected trackers saved successfully."});
    }

    [HttpPost]
    public IActionResult SaveConflictUsers([FromBody] RedmineUsersMappedModel model)
    {
        if (model?.Users == null || model.Users.Count == 0)
        {
            return BadRequest(model.Users);
        }

        _redmineUsersMap.Clear();
        _redmineUsersMap = model.Users;

        return Ok(new {success = true, message = "Selected statuses saved successfully." });
    }

    public async Task<List<RedmineIssue>> GetIssuesAsync()
    {
        using (HttpClient client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiKeyModel.BaseUrlExport}/issues.json");
            request.Headers.Add("X-Redmine-API-Key", _apiKeyModel.ApiKeyExport);
            var response = await client.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var issuesResponse = JsonConvert.DeserializeObject<IssuesResponse>(responseData);
                return issuesResponse.Issues;
            }
            else
            {
                // Handle error response
                return new List<RedmineIssue>();
            }
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects(int offset = 0, int limit = 50)
    {
        return Ok(await _redmineApiController.GetProjects(RedmineApiController.TYPE_IMPORT, offset, limit));
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(int offset = 0, int limit = 50)
    {
       return Ok(await _redmineApiController.GetUsers(RedmineApiController.TYPE_IMPORT, offset, limit));
    }

    [HttpGet]
    public async Task<IActionResult> GetIssueStatuses(int type = 0)
    {
        return Ok(await _redmineApiController.GetIssueStatus(
                type == 0 ?
                RedmineApiController.TYPE_IMPORT :
                RedmineApiController.TYPE_EXPORT
            ));
    }

    [HttpGet]
    public async Task<IActionResult> GetTrackers(int type = 0)
    {
        return Ok(await _redmineApiController.GetTrackers(
            type == 0 ?
                RedmineApiController.TYPE_IMPORT :
                RedmineApiController.TYPE_EXPORT
        ));
    }

    [HttpGet]
    public async Task<IActionResult> GetUserConflict()
    {
        List<RedmineUser> conflictUser = new List<RedmineUser>();
        var exportServerUsers = await _redmineApiController.GetUsers(RedmineApiController.TYPE_EXPORT, -1);

        foreach (var userId in _redmineUsersId) {
            RedmineUser user = await _redmineApiController.GetUserById(RedmineApiController.TYPE_IMPORT, userId);
            conflictUser.Add(user);
        }

        return Ok(new { success = true, conflictUser = conflictUser, exportServerUsers = exportServerUsers });
    }

    [HttpGet]
    public IActionResult DownloadJsonFiles()
    {
        var files = new Dictionary<string, object>
        {
            { "users.json", CreateJsonFromList(_redmineUsersMap) },
            { "statuses.json", CreateJsonFromList(_redmineStatusId) },
            { "projects.json", CreateJsonFromList(_redmineProjectIds) },
            { "trackers.json", CreateJsonFromList(_redmineTrackerId) }
        };

        using (var memoryStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(file.Key);
                    using (var entryStream = entry.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(file.Value);
                        streamWriter.Write(json);
                    }
                }
            }

            return File(memoryStream.ToArray(), "application/zip", "json-files.zip");
        }
    }

    private Dictionary<int, object> CreateJsonFromList(List<int> list)
    {
        var dictionary = new Dictionary<int, object>();
        foreach (var item in list)
        {
            dictionary[item] = null;
        }

        return dictionary;
    }

    private Dictionary<int, object> CreateJsonFromList(List<List<int>> list)
    {
        var dictionary = new Dictionary<int, object>();
        foreach (var innerList in list)
        {
            dictionary[innerList[0]] = innerList[1];
        }

        return dictionary;
    }
}
