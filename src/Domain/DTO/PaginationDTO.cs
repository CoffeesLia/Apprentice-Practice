namespace Domain.DTO
{
    public class PaginationDTO<T> where T : class
    {
        public List<T>? Result { get; set; }
        public int Total { get; set; }
    }
}
