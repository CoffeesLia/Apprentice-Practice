namespace Stellantis.ProjectName.WebApi.Dto.Filters;
internal class IntegrationFilterDto : FilterDto
{
    public string? Name { get; set; }
    public ApplicationDataDto ApplicationDataDto { get; set; } = null!;
}
