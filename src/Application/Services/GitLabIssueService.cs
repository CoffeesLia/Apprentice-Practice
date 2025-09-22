using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Stellantis.ProjectName.Application.DtoService;

namespace Stellantis.ProjectName.Application.Services
{
    public class GitLabIssueService
    {
        private readonly HttpClient _httpClient;
        private readonly string _projectId;
        private readonly string _baseUrl;

        public GitLabIssueService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _baseUrl = configuration["GitLab:BaseUrl"];
            _projectId = configuration["GitLab:ProjectId"];

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", configuration["GitLab:Token"]);
        }

        public async Task<string> CreateIssueAsync(GitLabIssueDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/projects/{_projectId}/issues", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UpdateIssueAsync(int issueIid, GitLabIssueDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/projects/{_projectId}/issues/{issueIid}", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CloseIssueAsync(int issueIid)
        {
            var dto = new { state_event = "close" }; // só precisa disso
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/projects/{_projectId}/issues/{issueIid}", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
