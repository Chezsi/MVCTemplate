using Microsoft.EntityFrameworkCore;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.DataAccess.Repository.IRepository;
using MVCTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MVCTemplate.DataAccess.Repository
{
    public class ManagerRepository : Repository<Manager>, IManagerRepository
    {
        private readonly ApplicationDbContext _db;

        public ManagerRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public bool Exists(int id)
        {
            return _db.Managers.Any(m => m.Id == id);
        }

        public void Add(Manager manager)
        {
            _db.Managers.Add(manager);
        }

        public IEnumerable<Manager> GetAll(string? includeProperties = null)
        {
            IQueryable<Manager> query = _db.Managers;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return query.ToList();
        }

        public void Remove(Manager manager)
        {
            _db.Managers.Remove(manager);
        }

        public void RemoveRange(IEnumerable<Manager> managers)
        {
            _db.Managers.RemoveRange(managers);
        }

        public void Update(Manager updatedManager)
        {
            var existing = _db.Managers.FirstOrDefault(m => m.Id == updatedManager.Id);
            if (existing == null) return;

            existing.Name = updatedManager.Name;
            existing.Email = updatedManager.Email;
            existing.UpdatedAt = DateTime.Now;

            _db.SaveChanges();
        }

        public Manager? GetFirstOrDefault(Expression<Func<Manager, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
