using Microsoft.AspNetCore.Mvc;
using redmineGUI.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
namespace redmineGUI.Controllers;

public class MigrationController : Controller
{
    private static ApiKeyModel _apiKeyModel;

    public async Task<IActionResult> Index()
    {
        var issues = await GetIssuesAsync(_apiKeyModel.baseUrlExport, _apiKeyModel.apiKeyExport);

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
            default:
                return PartialView("_DefaultStep");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveApiKey([FromBody] ApiKeyModel model)
    {
        _apiKeyModel = model;

        // Return success response
        return Json(new { success = true, message = "API keys saved successfully.", apikeyModel= _apiKeyModel });
    }

    public async Task<List<RedmineIssue>> GetIssuesAsync(string baseUrl, string apiKey)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-Redmine-API-Key", apiKey);
            HttpResponseMessage response = await client.GetAsync($"{baseUrl}/issues.json");

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
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-Redmine-API-Key", _apiKeyModel.apiKeyExport);
            HttpResponseMessage response = await client.GetAsync($"{_apiKeyModel.baseUrlExport}/projects.json?offset={offset}&limit={limit}");

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var projectsResponse = JsonConvert.DeserializeObject<RedmineProjectsResponse>(responseData);
                return Ok(projectsResponse.Projects);
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }
    }
}
