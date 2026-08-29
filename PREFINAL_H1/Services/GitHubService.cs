using System.Net.Http.Headers;
using System.Text.Json;
using PREFINAL_H1.Models;

namespace PREFINAL_H1.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GitHubService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<TaskModel>> GetShippedTasksAsync(string studentName, string targetDateStr, string owner, string repository)
        {
            var tasks = new List<TaskModel>();
            var token = _configuration["GitHub:Token"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository))
                return tasks;

            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitHubCsvApp", "1.0"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // closed or merged
            string issueUrl = $"https://api.github.com/repos/{owner}/{repository}/issues?state=closed&per_page=100";
            var response = await _httpClient.GetAsync(issueUrl);
            if (!response.IsSuccessStatusCode) return tasks;

            var jsonContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonContent);

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                //closed_at or merged_at?
                if (element.TryGetProperty("closed_at", out var closedAtProp) && closedAtProp.ValueKind != JsonValueKind.Null)
                {
                    string closedAt = closedAtProp.GetString() ?? "";
                    if (closedAt.StartsWith(targetDateStr))
                    {
                        var userObj = element.GetProperty("user");
                        string author = userObj.GetProperty("login").GetString() ?? "";

                        if (author.Equals(studentName, StringComparison.OrdinalIgnoreCase))
                        {
                            string title = element.GetProperty("title").GetString() ?? "";
                            string htmlUrl = element.GetProperty("html_url").GetString() ?? "";

                            string type = "Backend";
                            string lowerTitle = title.ToLower();
                            if (lowerTitle.Contains("ui") || lowerTitle.Contains("view") || lowerTitle.Contains("html") || lowerTitle.Contains("css"))
                                type = "Frontend";
                            else if (lowerTitle.Contains("test") || lowerTitle.Contains("qa") || lowerTitle.Contains("bug"))
                                type = "QA";

                            tasks.Add(new TaskModel
                            {
                                Name = title,
                                Link = htmlUrl,
                                Team = "DefaultTeam",
                                Type = type
                            });
                        }
                    }
                }
            }

            return tasks;
        }
    }
}