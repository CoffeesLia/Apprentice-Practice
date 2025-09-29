using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Stellantis.ProjectName.Application.DtoService;

namespace Stellantis.ProjectName.Application.Services
{
    public class GitLabIssueService(HttpClient httpClient, IConfiguration configuration)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly string _projectId = configuration["GitLab:ProjectId"]!;

        public async Task<string> GetIssueAsync(int issueIid)
        {
            var response = await _httpClient.GetAsync($"projects/{_projectId}/issues/{issueIid}").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<GitLabIssueDto[]> ListIssuesAsync()
        {
            // Chama o GitLab para listar todas as issues do projeto
            var response = await _httpClient.GetAsync($"projects/{_projectId}/issues");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GitLabIssueDto[]>(json)!;
        }


        public async Task<string> CreateIssueAsync(GitLabIssueDto dto)
        {
            var payload = new Dictionary<string, object?>
            {
                ["title"] = dto.Title,
                ["description"] = dto.Description,
            };

            if (dto.AssigneeId.HasValue)
                payload["assignee_id"] = dto.AssigneeId.Value;

            if (dto.Labels != null && dto.Labels.Length > 0)
                payload["labels"] = string.Join(",", dto.Labels);

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"projects/{_projectId}/issues", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
            {
                // GitLab não aceita "open", só "close" ou "reopen"
                payload["state_event"] = dto.StateEvent == "open" ? "reopen" : dto.StateEvent;
            }

            if (dto.Labels != null && dto.Labels.Length > 0)
                payload["add_labels"] = string.Join(",", dto.Labels);

            if (dto.RemoveLabels != null && dto.RemoveLabels.Length > 0)
                payload["remove_labels"] = string.Join(",", dto.RemoveLabels);

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"projects/{_projectId}/issues/{issueIid}", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<string> CloseIssueAsync(int issueIid)
        {
            var dto = new { state_event = "close" };
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"projects/{_projectId}/issues/{issueIid}", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}

