using Microsoft.EntityFrameworkCore;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.DataAccess.Repository.IRepository;
using MVCTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MVCTemplate.DataAccess.Repository
{
    public class PersonRepository : Repository<Person>, IPersonRepository
    {
        private readonly ApplicationDbContext _db;

        public PersonRepository(ApplicationDbContext db) : base(db) 
        {
            _db = db;
        }

        public bool Exists(int? id)
        {
            if (!id.HasValue)
                return false;

            return _db.Persons.Any(p => p.Id == id.Value);
        }

        public void Add(Person person) // for the app person
        {
            _db.Persons.Update(person);
        }

        public Person? CheckIfUnique(string name) 
        {
            return _db.Persons.FirstOrDefault(i => i.Name == name);
        }

        public Person? ContinueIfNoChangeOnUpdate(string name, int Id)
        {
            return _db.Persons.FirstOrDefault(i => i.Name == name && i.Id != Id);
        }

        public void Delete(Person entity) // unused
        {
            throw new NotImplementedException();
        }

        public Person Get(Expression<Func<Person, bool>> filter, string? includeProperties = null)
        {
            var person = _db.Set<Person>().FirstOrDefault(filter);
            if (person != null)
            {
                _db.Set<Person>().Remove(person);
                _db.SaveChanges();
                return person;
            }

            throw new InvalidOperationException("Person not found.");
        }


        public Person GetFirstOrDefault(Expression<Func<Person, bool>> predicate)
        {
            return _db.Persons.FirstOrDefault(predicate);
        }

        public Person GetFirstOrDefault(Expression<Func<Category, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public void Remove(Person person)
        {
            _db.Set<Person>().Remove(person);
            // _db.SaveChanges();
        }

        public void RemoveRange(IEnumerable<Person> entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Person person)
        {
            _db.Persons.Update(person);
        }

        IEnumerable<Person> IRepository<Person>.GetAll(string? includeProperties)
        {
            return _db.Persons.ToList();
        }

        public List<Product> ToList()
        {
            return _db.Products.ToList(); //not referenced
        }
    }
}
