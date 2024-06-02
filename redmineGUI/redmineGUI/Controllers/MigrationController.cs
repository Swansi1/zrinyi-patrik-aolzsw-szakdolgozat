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

namespace redmineGUI.Controllers;

public class MigrationController : Controller
{
    private static ApiKeyModel _apiKeyModel;
    private static List<int> _redmineProjectIds = new List<int>();
    private static List<int> _redmineUsersId = new List<int>();
    private static List<int> _redmineStatusId = new List<int>();

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
            default:
                return PartialView("_DefaultStep");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveApiKey([FromBody] ApiKeyModel model)
    {
        _apiKeyModel = model;

        var exportServer = new RedmineServerModel
        {
            ApiKey = _apiKeyModel.apiKeyExport,
            BaseUrl = _apiKeyModel.baseUrlExport,
        };

        var importServer = new RedmineServerModel
        {
            ApiKey = _apiKeyModel.apiKeyImport,
            BaseUrl = _apiKeyModel.baseUrlImport,
        };

        _redmineApiController = new RedmineApiController(exportServer, importServer);

        return Json(new { success = true, message = "API keys saved successfully.", apikeyModel= _apiKeyModel.apiKeyExport });
    }

    [HttpPost]
    public IActionResult SaveProjects([FromBody] RedmineSelectedProjects model)
    {
        if (model?.ProjectIds == null || model.ProjectIds.Count == 0)
        {
            return BadRequest(model.ProjectIds);
        }

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

        _redmineStatusId = model.StatusIds.Distinct().ToList();

        return Ok(new { success = true, message = "Selected statuses saved successfully."});
    }

    public async Task<List<RedmineIssue>> GetIssuesAsync()
    {
        using (HttpClient client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiKeyModel.baseUrlExport}/issues.json");
            request.Headers.Add("X-Redmine-API-Key", _apiKeyModel.apiKeyExport);
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
    public async Task<IActionResult> GetIssueStatuses()
    {
        return Ok(await _redmineApiController.GetIssueStatus(RedmineApiController.TYPE_IMPORT));
    }

    [HttpGet]
    public async Task<IActionResult> GetUserConflict()
    {
        List<RedmineUser> conflictUser = new List<RedmineUser>();
        var exportServerUsers = await _redmineApiController.GetUsers(RedmineApiController.TYPE_EXPORT, -1);
        foreach (var userId in _redmineUsersId) {
            RedmineUser user = await _redmineApiController.GetUserById(RedmineApiController.TYPE_IMPORT, userId);
            var userExist = exportServerUsers.FirstOrDefault(u => u.Mail == user.Mail);
            if (userExist == null) {
                conflictUser.Add(user);
            }
        }

        return Ok(new { success = true, conflictUser = conflictUser, exportServerUsers = exportServerUsers });
    }
}
