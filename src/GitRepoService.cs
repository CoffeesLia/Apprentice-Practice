using 
namespace Stellantis.ProjectName.Application.Services
{
    public class GitRepoService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<GitRepo> validator)
        : EntityServiceBase<GitRepo, IGitRepoRepository>(unitOfWork, localizerFactory, validator), IGitRepoService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(GitResource));
        protected override IGitRepoRepository Repository => UnitOfWork.GitRepoRepository;


        public override async Task<OperationResult> CreateAsync(GitRepo item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Validação do objeto GitRepo
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificação se a URL já existe
            if (await Repository.VerifyUrlAlreadyExistsAsync(item.Url).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(GitResource.AlreadyExists)]);
            }

            // Verificação se os campos obrigatórios estão preenchidos
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Description) || string.IsNullOrWhiteSpace(item.Url))
            {
                return OperationResult.InvalidData(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(item.Name), _localizer["NameRequired"]),
                    new ValidationFailure(nameof(item.Description), _localizer["DescriptionRequired"]),
                    new ValidationFailure(nameof(item.Url), _localizer["UrlRequired"])
                }));
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<GitRepo>> GetListAsync(GitRepoFilter filter)
        {
            return await Repository.GetListAsync(filter).ConfigureAwait(false);
        }

        public async Task<GitRepo?> GetItemAsync(int id)
        {
            return await Repository.GetByIdAsync(id).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(GitRepo item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Validação do objeto GitRepo
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificação se a URL já existe para outro repositório
            var existingRepo = await Repository.GetByUrlAsync(item.Url).ConfigureAwait(false);
            if (existingRepo != null && existingRepo.Id != item.Id)
            {
                return OperationResult.Conflict(_localizer[nameof(GitResource.AlreadyExists)]);
            }

            // Verificação se os campos obrigatórios estão preenchidos
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Description) || string.IsNullOrWhiteSpace(item.Url))
            {
                return OperationResult.InvalidData(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(item.Name), _localizer["NameRequired"]),
                    new ValidationFailure(nameof(item.Description), _localizer["DescriptionRequired"]),
                    new ValidationFailure(nameof(item.Url), _localizer["UrlRequired"])
                }));
            }

            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var repo = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (repo == null)
            {
                return OperationResult.NotFound(_localizer[nameof(OperationResult.NotFound)]);
            }

            await Repository.DeleteAsync(id).ConfigureAwait(false);
            return OperationResult.Complete(_localizer[nameof(GitResource.DeletedSuccessfully)]);
        }
    }
}
