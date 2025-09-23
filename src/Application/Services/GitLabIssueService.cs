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

        public async Task<string> GetIssueAsync(int issueIid)
        {
            var response = await _httpClient.GetAsync($"projects/{_projectId}/issues/{issueIid}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> CreateIssueAsync(GitLabIssueDto dto)
        {
            var payload = new Dictionary<string, object?>
            {
                ["title"] = dto.Title,
                ["description"] = dto.Description,
                ["state_event"] = dto.StateEvent
            };

            if (dto.AssigneeId.HasValue)
                payload["assignee_id"] = dto.AssigneeId.Value;

            if (dto.Labels != null && dto.Labels.Length > 0)
                payload["labels"] = string.Join(",", dto.Labels);

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"projects/{_projectId}/issues", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UpdateIssueAsync(int issueIid, GitLabIssueDto dto)
        {
            var payload = new Dictionary<string, object?>();

            if (!string.IsNullOrEmpty(dto.Title))
                payload["title"] = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description))
                payload["description"] = dto.Description;
            if (dto.AssigneeId.HasValue)
                payload["assignee_id"] = dto.AssigneeId.Value;
            if (!string.IsNullOrEmpty(dto.StateEvent))
                payload["state_event"] = dto.StateEvent;

            if (dto.Labels != null && dto.Labels.Length > 0)
                payload["add_labels"] = string.Join(",", dto.Labels);

            if (dto.RemoveLabels != null && dto.RemoveLabels.Length > 0)
                payload["remove_labels"] = string.Join(",", dto.RemoveLabels);

            var json = JsonSerializer.Serialize(payload);
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

