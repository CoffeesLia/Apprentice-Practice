using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Stellantis.ProjectName.Application.DtoService;

namespace Stellantis.ProjectName.Application.Services
{
    public class BranchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _projectId;

        public BranchService(HttpClient httpClient, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(configuration);

            var projectId = configuration["Branch:ProjectId"];
            var token = configuration["Branch:PRIVATE-TOKEN"];
            var baseUrl = configuration["Branch:BaseUrl"];

            ArgumentNullException.ThrowIfNull(projectId);
            ArgumentNullException.ThrowIfNull(token);
            ArgumentNullException.ThrowIfNull(baseUrl);

            _httpClient = httpClient;
            _projectId = projectId;
            _httpClient.BaseAddress = new Uri(baseUrl);

            // Usando Authorization: Bearer
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // GET: Listar branches
        public async Task<List<BranchDtoService>> GetBranchesAsync()
        {
            var endpoint = new Uri($"projects/{_projectId}/repository/branches", UriKind.Relative);
            var response = await _httpClient.GetAsync(endpoint).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var branchesJson = JsonSerializer.Deserialize<List<JsonElement>>(json);

            var branches = new List<BranchDtoService>();
            if (branchesJson != null)
            {
                foreach (var branch in branchesJson)
                {
                    branches.Add(new BranchDtoService
                    {
                        Name = branch.GetProperty("name").GetString() ?? "",
                        Author = branch.GetProperty("commit").GetProperty("author_name").GetString() ?? "",
                        LastUpdated = branch.GetProperty("commit").GetProperty("committed_date").GetDateTime()
                    });
                }
            }

            return branches;
        }

        // Criar branch
        public async Task<string> CreateBranchAsync(CreateBranchRequest dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var endpoint = new Uri(
                $"projects/{_projectId}/repository/branches?branch={dto.NewBranchName}&ref={dto.SourceBranch}",
                UriKind.Relative);

            var response = await _httpClient.PostAsync(endpoint, null).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        // Deletar branch
        public async Task<string> DeleteBranchAsync(string branchName)
        {
            ArgumentNullException.ThrowIfNull(branchName);

            var endpoint = new Uri(
                $"projects/{_projectId}/repository/branches/{branchName}",
                UriKind.Relative);

            var response = await _httpClient.DeleteAsync(endpoint).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}