using ECommerce.Model.Data;
using ECommerce.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Model.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<Customer?> GetByUserIdAsync(string userId) =>
        await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
}
