
using MVCTemplate.Models;
using System.Linq.Expressions;

namespace MVCTemplate.DataAccess.Repository.IRepository
{
    public interface IPersonRepository : IRepository<Person>
    {
        void Update(Person person);

        void Remove(Person person);

        Person? CheckIfUnique(string name);

        Person? ContinueIfNoChangeOnUpdate(string name, int Id);

        Person GetFirstOrDefault(Expression<Func<Person, bool>> predicate);
        bool Exists(int? id);
    }
}
