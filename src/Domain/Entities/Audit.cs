namespace Stellantis.ProjectName.Domain.Entities
{
    public class Audit
    {
        public int Id { get; set; }
        public string Table { get; set; } = null!;
        public string Action { get; set; } = null!;
        public List<string> OldValues { get; set; } = [];
        public List<string> NewValues { get; set; } = [];
        public string CreatedBy { get; set; } = null!;
        public DateTime DateTime { get; set; }
    }
}
