using AutoMapper;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.Domain.Entities;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Other service configurations...

        // AutoMapper configuration
        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MappingProfile());
        });

        IMapper mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);
    }
}
