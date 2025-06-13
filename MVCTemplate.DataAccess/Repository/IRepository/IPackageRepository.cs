
using MVCTemplate.Models;
using System.Linq.Expressions;

namespace MVCTemplate.DataAccess.Repository.IRepository
{
    public interface IPackageRepository : IRepository<Package>
    {
        void Update(Package package);

        void Remove(Package package);

        Package? CheckIfUnique(string name);

        Package? ContinueIfNoChangeOnUpdate(string name, int countryId);

        Package GetFirstOrDefault(Expression<Func<Package, bool>> predicate);
    }
}
