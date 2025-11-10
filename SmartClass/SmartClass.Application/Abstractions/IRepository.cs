using Ardalis.Specification;
using SmartClass.Domaine.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Abstractions
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> GetByKeyAsync<TKey>(TKey key);
        Task<TEntity> GetByPairOfKeysAsync<TKey, TKey1>(TKey key, TKey1 key1);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
        Task<int> SaveChangesAsync();
        Task AddRangeAsync(List<TEntity> entities);
        Task<TEntity> GetFirstBySpecAsync(ISpecification<TEntity> specification);
        Task<IEnumerable<TEntity>> GetListAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null);
        Task<TEntity> GetEntityAsync(
              Expression<Func<TEntity, bool>> filter = null,
              string includeProperties = null);
    }
}
