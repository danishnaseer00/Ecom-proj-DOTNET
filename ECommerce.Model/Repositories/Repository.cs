using System.Linq.Expressions;
using ECommerce.Model.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Model.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context) { _context = context; _dbSet = context.Set<T>(); }

    public virtual async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);
    public virtual async Task<IReadOnlyList<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<T> AddAsync(T entity) { await _dbSet.AddAsync(entity); await _context.SaveChangesAsync(); return entity; }
    public virtual async Task UpdateAsync(T entity) { _dbSet.Update(entity); await _context.SaveChangesAsync(); }
    public virtual async Task DeleteAsync(T entity) { _dbSet.Remove(entity); await _context.SaveChangesAsync(); }
}
