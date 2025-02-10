using Stellantis.ProjectName.Domain.Enums;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class SupplierPartNumberFilterDto : FilterDto
    {
        internal int SupplierId { get; set; }
    }
}
