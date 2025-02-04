using Stellantis.ProjectName.Domain.Enums;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class PartNumber(string code, string description, PartNumberType type) : BaseEntity
    {
        public string Code { get; set; } = code;
        public string Description { get; } = description;
        public PartNumberType Type { get; } = type;
        public virtual ICollection<PartNumberSupplier> Suppliers { get; } = [];
        public virtual ICollection<VehiclePartNumber> Vehicles { get; } = [];
    }
}
