using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using redmineGUI.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace redmineGUI.Controllers
{
    public class RedmineApiController
    {
        public static readonly int TYPE_IMPORT = 1;
        public static readonly int TYPE_EXPORT = 2;

        private readonly RedmineServerModel _exportServer;
        private readonly RedmineServerModel _importServer;

        public RedmineApiController(RedmineServerModel exportServer, RedmineServerModel importServer)
        {
            _exportServer = exportServer;
            _importServer = importServer;
        }

        public async Task<List<RedmineUser>> GetUsers(int type, int offset = 0, int limit = 50)
        {
            var server = this.GetServerType(type);
            string urlParams = GetUrlParamsLimit(offset, limit);

            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{server.BaseUrl}/users.json{urlParams}");
                request.Headers.Add("X-Redmine-API-Key", server.ApiKey);
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var usersResponse = JsonConvert.DeserializeObject<RedmineUsersResponse>(responseData);
                    return usersResponse.Users;
                }

                return new List<RedmineUser>();
            }
        }

        public async Task<RedmineUser> GetUserById(int type, int userId)
        {
            var server = this.GetServerType(type);

            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{server.BaseUrl}/users/{userId}.json");
                request.Headers.Add("X-Redmine-API-Key", server.ApiKey);
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var userResponse = JsonConvert.DeserializeObject<RedmineUserResponse>(responseData);
                    return userResponse.User;
                }

                return new RedmineUser();
            }
        }

        public async Task<List<RedmineProject>> GetProjects(int type, int offset = 0, int limit = 50)
        {
            var server = this.GetServerType(type);
            string urlParams = GetUrlParamsLimit(offset, limit);

            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{server.BaseUrl}/projects.json{urlParams}");
                request.Headers.Add("X-Redmine-API-Key", server.ApiKey);
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var projectsResponse = JsonConvert.DeserializeObject<RedmineProjectsResponse>(responseData);
                    return projectsResponse.Projects;
                }

                return new List<RedmineProject>{};
            }
        }

        public async Task<List<RedmineIssueStatus>> GetIssueStatus(int type)
        {
            var server = GetServerType(type);

            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{server.BaseUrl}/issue_statuses.json");
                request.Headers.Add("X-Redmine-API-Key", server.ApiKey);
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var issueStatusesResponse = JsonConvert.DeserializeObject<RedmineIssueStatusesResponse>(responseData);
                    return issueStatusesResponse.IssueStatuses;
                }

                return new List<RedmineIssueStatus>{};
            }
        } 

        private RedmineServerModel GetServerType(int Type)
        {
            return Type == 0 ? this._importServer : this._exportServer;
        }

        private static string GetUrlParamsLimit(int offset, int limit)
        {
            string urlParams = $"?offset={offset}&limit={limit}";
            if (offset == -1 || limit == -1)
            {
                urlParams = "";
            }

            return urlParams;
        }
    }
}