using System.Text;
using Microsoft.AspNetCore.Mvc;
using PREFINAL_H1.Models;
using PREFINAL_H1.Services;

namespace PREFINAL_H1.Controllers
{
    public class GitHubController : Controller
    {
        private readonly GitHubService _gitHubService;

        public GitHubController(GitHubService gitHubService)
        {
            _gitHubService = gitHubService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new FilterModel
            {
                TargetDate = "2026-08-29"
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExportCsv(FilterModel model)
        {
            var tasks = await _gitHubService.GetShippedTasksAsync(
                model.StudentName,
                model.TargetDate,
                model.Owner,
                model.Repository
            );

            
            var sortedTasks = tasks
                .OrderBy(t => t.Team)
                .ThenBy(t => t.Type)
                .ThenBy(t => t.Name)
                .ToList();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Name,Link,Team,Type");

            foreach (var task in sortedTasks)
            {
                string escapedName = "\"" + task.Name.Replace("\"", "\"\"") + "\"";
                csvBuilder.AppendLine($"{escapedName},{task.Link},{task.Team},{task.Type}");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(buffer, "text/csv", $"ShippedTasks_{model.StudentName}_{model.TargetDate}.csv");
        }
    }
}