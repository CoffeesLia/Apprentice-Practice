namespace Stellantis.ProjectName.Domain.Entities
{
    public class DocumentData : BaseEntity
    {
        public required string Name { get; set; } = string.Empty;
        public required Uri Url { get; set; }
        public ApplicationData? ApplicationData { get; set; }
        public int ApplicationId { get; set; }
    }
}
