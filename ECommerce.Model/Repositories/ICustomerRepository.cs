using ECommerce.Model.Entities;

namespace ECommerce.Model.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByUserIdAsync(string userId);
}
