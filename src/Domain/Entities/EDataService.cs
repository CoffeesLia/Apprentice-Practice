namespace Stellantis.ProjectName.Domain.Entities
{
    public class EDataService
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int ApplicationId { get; set; }
        public Application? Application { get; set; }
    }

    public class Application
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}