using MVCTemplate.Models;
using System.Linq.Expressions;

namespace MVCTemplate.DataAccess.Repository.IRepository
{
    public interface IReportRepository : IRepository<Report>
    {
        void Update(Report report);

        void Remove(Report report);

        Report? CheckIfUnique(string title);

        Report? ContinueIfNoChangeOnUpdate(string title, int id);

        Report GetFirstOrDefault(Expression<Func<Report, bool>> predicate);
        List<Report> ToList();
    }
}
