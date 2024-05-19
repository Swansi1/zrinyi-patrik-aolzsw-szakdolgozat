using Microsoft.AspNetCore.Mvc;
using redmineGUI.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
namespace redmineGUI.Controllers;

public class MigrationController : Controller
{
    private static ApiKeyModel _apiKeyModel;

    public IActionResult Index()
    {
        return View();
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
    public IActionResult SaveApiKey(ApiKeyModel model)
    {
        _apiKeyModel = model;

        // Set a debug message
        TempData["DebugMessage"] =_apiKeyModel.ApiKey;

        // Redirect to Index action
        return RedirectToAction("Index");
    }

    // [HttpGet]
    // public async Task<IActionResult> GetUsers()
    // {
    //     if (_apiKeyModel == null)
    //     {
    //         return BadRequest("API Key is not set.");
    //     }

    //     // var users = await FetchUsersFromRedmine();
    //     // return PartialView("_Users", users);
    // }

    // private async Task<List<RedmineUser>> FetchUsersFromRedmine()
    // {
    //     var client = new HttpClient();
    //     client.DefaultRequestHeaders.Add("X-Redmine-API-Key", _apiKeyModel.ApiKey);
    //     var response = await client.GetAsync($"{_apiKeyModel.BaseUrl}/users.json");
    //     var content = await response.Content.ReadAsStringAsync();
    //     var json = JObject.Parse(content);
    //     var users = json["users"].ToObject<List<RedmineUser>>();
    //     return users;
    // }
}
