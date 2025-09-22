using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Stellantis.ProjectName.Application.DtoService;

namespace Stellantis.ProjectName.Application.Services
{
    public class GitLabIssueService
    {
        private readonly HttpClient _httpClient;
        private readonly string _projectId;

        public GitLabIssueService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _projectId = configuration["GitLab:ProjectId"]!;
        }

        public async Task<string> CreateIssueAsync(GitLabIssueDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Apenas o endpoint relativo, sem baseUrl e sem token
            var response = await _httpClient.PostAsync($"projects/{_projectId}/issues", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UpdateIssueAsync(int issueIid, GitLabIssueDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"projects/{_projectId}/issues/{issueIid}", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CloseIssueAsync(int issueIid)
        {
            var dto = new { state_event = "close" };
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"projects/{_projectId}/issues/{issueIid}", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
