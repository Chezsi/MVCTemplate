
using Microsoft.EntityFrameworkCore;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.Models;
using System.Linq.Expressions;

namespace MVCTemplate.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {

        void Update(Product product);

        void Remove(Product product);

        Product? CheckIfUnique(string name);

        Product? ContinueIfNoChangeOnUpdate(string name, int countryId);

        Product GetFirstOrDefault(Expression<Func<Product, bool>> predicate); // rest of getfirstordefault is in productrepository

        List<Product> ToList();

        IEnumerable<Product> GetAll(string? includeProperties = null);
    }
}
