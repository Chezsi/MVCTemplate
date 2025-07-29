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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public Product? CheckIfUnique(string name)
        {
            return _db.Products.FirstOrDefault(i => i.Name == name);
        }

        public Product? ContinueIfNoChangeOnUpdate(string name, int countryId)
        {
            return _db.Products.FirstOrDefault(i => i.Name == name && i.Id != countryId);
        }

        public Product GetFirstOrDefault(Expression<Func<Product, bool>> predicate)
        {
            return _db.Products.FirstOrDefault(predicate);
        } //since it cant be in interface repository

        public List<Product> ToList()
        {
            return _db.Products.ToList();
        }

        public void Update(Product updatedProduct)
        {
            var existing = _db.Products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (existing == null) return;

            // Manually update fields
            existing.Name = updatedProduct.Name;
            existing.Description = updatedProduct.Description;
            existing.Quantity = updatedProduct.Quantity;
            existing.UpdatedAt = DateTime.Now;
                // so CreatedAt is not being given a new value
            _db.SaveChanges();
        }

        public IEnumerable<Product> GetAll(string? includeProperties = null)
        {
            IQueryable<Product> query = _db.Products;

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

    }
}
