using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Services;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

public class GitLabRepositoryServiceTests
{
    private readonly GitLabRepositoryService _service;

    public GitLabRepositoryServiceTests()
    {
        _service = new GitLabRepositoryService();
    }

    [Fact]
    public async Task CreateAsync_ValidRepository_ReturnsComplete()
    {
        var newRepo = new EntityGitLabRepository
        {
            Name = "Test Repo",
            Description = "Test Description",
            Url = "http://testurl.com",
            ApplicationId = 1
        };

        var result = await _service.CreateAsync(newRepo);

        Assert.Equal(OperationStatus.Success, result.Status);
        Assert.Equal(GitLabResource.RegisteredSuccessfully, result.Message);
    }

    [Fact]
    public async Task CreateAsync_InvalidRepository_ReturnsInvalidData()
    {
        var newRepo = new EntityGitLabRepository
        {
            Name = "",
            Description = "",
            Url = "",
            ApplicationId = 1
        };

        var result = await _service.CreateAsync(newRepo);

        Assert.Equal(OperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ExistingUrl_ReturnsConflict()
    {
        var newRepo1 = new EntityGitLabRepository
        {
            Name = "Test Repo 1",
            Description = "Test Description 1",
            Url = "http://testurl.com",
            ApplicationId = 1
        };

        var newRepo2 = new EntityGitLabRepository
        {
            Name = "Test Repo 2",
            Description = "Test Description 2",
            Url = "http://testurl.com",
            ApplicationId = 2
        };

        await _service.CreateAsync(newRepo1);
        var result = await _service.CreateAsync(newRepo2);

        Assert.Equal(OperationStatus.Conflict, result.Status);
        Assert.Equal(GitLabResource.ExistentRepositoryUrl, result.Message);
    }

    [Fact]
    public async Task GetRepositoryDetailsAsync_ExistingId_ReturnsRepository()
    {
        var newRepo = new EntityGitLabRepository
        {
            Name = "Test Repo",
            Description = "Test Description",
            Url = "http://testurl.com",
            ApplicationId = 1
        };

        await _service.CreateAsync(newRepo);
        var result = await _service.GetRepositoryDetailsAsync(newRepo.Id);

        Assert.NotNull(result);
        Assert.Equal(newRepo.Name, result.Name);
    }

    [Fact]
    public async Task GetRepositoryDetailsAsync_NonExistingId_ReturnsNull()
    {
        var result = await _service.GetRepositoryDetailsAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ValidRepository_ReturnsComplete()
    {
        var newRepo = new EntityGitLabRepository
        {
            Name = "Test Repo",
            Description = "Test Description",
            Url = "http://testurl.com",
            ApplicationId = 1
        };

        await _service.CreateAsync(newRepo);

        newRepo.Name = "Updated Repo";
        var result = await _service.UpdateAsync(newRepo, "Valid update");

        Assert.Equal(OperationStatus.Success, result.Status);
        Assert.Equal(GitLabResource.UpdatedSuccessfully, result.Message);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingRepository_ReturnsNotFound()
    {
        var newRepo = new EntityGitLabRepository
        {
            Id = 999,
            Name = "Test Repo",
            Description = "Test Description",
            Url = "http://testurl.com",
            ApplicationId = 1
        };

        var result = await _service.UpdateAsync(newRepo, "Valid update");

        Assert.Equal(OperationStatus.NotFound, result.Status);
        Assert.Equal(GitLabResource.RepositoryNotFound, result.Message);
    }

    [Fact]
    public async Task DeleteAsync_ExistingRepository_ReturnsComplete()
    {
        var newRepo = new EntityGitLabRepository
        {
            Name = "Test Repo",
            Description = "Test Description",
            Url = "http://testurl.com",
            ApplicationId = 1
        };

        await _service.CreateAsync(newRepo);
        var result = await _service.DeleteAsync(newRepo.Id);

        Assert.Equal(OperationStatus.Success, result.Status);
        Assert.Equal(GitLabResource.DeletedSuccessfully, result.Message);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingRepository_ReturnsNotFound()
    {
        var result = await _service.DeleteAsync(999);

        Assert.Equal(OperationStatus.NotFound, result.Status);
        Assert.Equal(GitLabResource.RepositoryNotFound, result.Message);
    }
}
