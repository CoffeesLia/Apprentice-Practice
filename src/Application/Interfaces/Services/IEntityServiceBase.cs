using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IEntityServiceBase<TEntity> where TEntity : EntityBase
    {
        Task<OperationResult> CreateAsync(TEntity item);
        Task<OperationResult> DeleteAsync(int id);
        Task<TEntity?> GetItemAsync(int id);
        Task<OperationResult> UpdateAsync(TEntity item);
    }
}