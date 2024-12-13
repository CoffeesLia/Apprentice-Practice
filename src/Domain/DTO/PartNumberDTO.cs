using Domain.Enum;

namespace Domain.DTO
{
    public class PartNumberDTO : BaseDTO
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public PartNumberType Type { get; set; }
    }
}
