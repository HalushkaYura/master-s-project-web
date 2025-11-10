using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartClass.Application.Abstractions;
using SmartClass.Domaine.Primitives;
using SmartClass.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace SmartClass.Infrastructure.Data.Repositories
{
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;
        public BaseRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }
        public async Task<TEntity> GetFirstBySpecAsync(ISpecification<TEntity> specification)
        {
            var res = await ApplySpecification(specification).FirstOrDefaultAsync();
            return res;

        }

        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            var evaluator = new SpecificationEvaluator();
            return evaluator.GetQuery(_dbSet, specification);
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> GetByKeyAsync<TKey>(TKey key)
        {
            return await _dbSet.FindAsync(key);
        }
        public async Task<TEntity> GetByPairOfKeysAsync<TFirstKey, TSecondKey>
            (TFirstKey firstKey, TSecondKey secondKey)
        {
            return await _dbSet.FindAsync(firstKey, secondKey);
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            return (await _dbSet.AddAsync(entity)).Entity;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            await Task.Run(() => _dbContext.Entry(entity).State = EntityState.Modified);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            await Task.Run(() => _dbSet.Remove(entity));
        }
        /*public async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            await Task.Run(() => _dbSet.RemoveRange(entities));
        }*/

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task AddRangeAsync(List<TEntity> entities)
        {
            await _dbContext.AddRangeAsync(entities);
        }
        public async Task<IEnumerable<TEntity>> GetListAsync(
                    Expression<Func<TEntity,
                    bool>> filter = null,
                    Func<IQueryable<TEntity>,
                    IOrderedQueryable<TEntity>> orderBy = null,
                    string includeProperties = null)
        {
            // Створення запиту до бази даних на основі параметрів
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Виконання запиту та отримання результатів
            return await query.ToListAsync();
        }

        public async Task<TEntity> GetEntityAsync(
    Expression<Func<TEntity, bool>> filter = null,
    string includeProperties = null)
        {
            // Створення запиту до бази даних на основі параметрів
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            // Виконання запиту та отримання результату (перший знайдений об'єкт або null)
            return await query.FirstOrDefaultAsync();
        }

    }
}
