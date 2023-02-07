using Microsoft.EntityFrameworkCore;
using UnitTestProjesi.Models;

namespace UnitTestProjesi.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly fw_UnitTestProjeContext _dbContext;
        private readonly DbSet<T> _dbSet;
        public Repository(fw_UnitTestProjeContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public async Task Create(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
           return await _dbSet.FindAsync(id);
        }

        public async Task Update(T entity)
        {
            _dbContext.Entry(entity).State= EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
}
