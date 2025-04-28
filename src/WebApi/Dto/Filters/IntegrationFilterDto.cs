namespace Stellantis.ProjectName.WebApi.Dto.Filters;

public class IntegrationFilterDto : FilterDto
{
    public string? Name { get; set; }
    public ApplicationDataDto ApplicationDataDto { get; set; } = null!;
}
