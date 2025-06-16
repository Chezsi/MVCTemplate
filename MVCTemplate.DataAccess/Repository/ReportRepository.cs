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
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        private readonly ApplicationDbContext _db;

        public ReportRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Add(Report report)
        {
            _db.Reports.Add(report);
        }

        public Report? Get(Expression<Func<Report, bool>> filter, string? includeProperties = null)
        {
            IQueryable<Report> query = _db.Reports;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return query.FirstOrDefault(filter);
        }

        public IEnumerable<Report> GetAll(string? includeProperties = null)
        {
            IQueryable<Report> query = _db.Reports;

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

        public void Remove(Report report)
        {
            _db.Reports.Remove(report);
        }

        public void RemoveRange(IEnumerable<Report> reports)
        {
            _db.Reports.RemoveRange(reports);
        }

        public void Update(Report report)
        {
            _db.Reports.Update(report);
        }

        public Report? ContinueIfNoChangeOnUpdate(string title, int id)
        {
            return _db.Reports.FirstOrDefault(r => r.Title == title && r.Id != id);
        }

        public Report? CheckIfUnique(string title)
        {
            return _db.Reports.FirstOrDefault(r => r.Title == title);
        }

        public Report GetFirstOrDefault(Expression<Func<Report, bool>> predicate)
        {
            return _db.Reports.FirstOrDefault(predicate);
        }

        public List<Report> ToList()
        {
            return _db.Reports.ToList();
        }
    }
}
