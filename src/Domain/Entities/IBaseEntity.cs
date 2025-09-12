namespace Stellantis.ProjectName.Domain.Entities
{
    public interface IBaseEntity
    {
        public int Id { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    public class BaseEntity : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

}