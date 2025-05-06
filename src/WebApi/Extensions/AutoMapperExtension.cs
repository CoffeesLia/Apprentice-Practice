using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Stellantis.ProjectName.WebApi.Mapper;

namespace Stellantis.ProjectName.WebApi.Extensions
{
    internal static class AutoMapperExtension
    {
        public static IServiceCollection RegisterMapper(this IServiceCollection services)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddCollectionMappers();
                mc.AddProfile(new AutoMapperProfile());
            });

            mappingConfig.AssertConfigurationIsValid();
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            return services;
        }
    }
}
