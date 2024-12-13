namespace Domain.DTO
{
    public class PartNumberFilterDTO : BaseFilterDTO
    {
        public string? Code { get; set; }
        public string? Description { get; set; }

        public int? Type { get; set; }

    }
}
