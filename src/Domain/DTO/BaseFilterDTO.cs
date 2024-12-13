namespace Domain.DTO
{
    public class BaseFilterDTO
    {
        public int Page { get; set; }
        public int RowsPerPage { get; set; }
        public string? Sort { get; set; }
        public string? SortDir { get; set; }
    }
}
