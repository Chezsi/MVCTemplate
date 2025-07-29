using MVCTemplate.Models;
using System.Collections.Generic;

namespace MVCTemplate.DataAccess.Repository.IRepository
{
    public interface IManagerRepository : IRepository<Manager>
    {
        void Update(Manager manager);

        void Remove(Manager manager);

        bool Exists(int id);

        IEnumerable<Manager> GetAll(string? includeProperties = null);

        Manager? GetFirstOrDefault(System.Linq.Expressions.Expression<System.Func<Manager, bool>> predicate);
    }
}
