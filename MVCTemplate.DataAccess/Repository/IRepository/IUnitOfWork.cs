namespace MVCTemplate.DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    IProductRepository Product { get; } 

    ICategoryRepository Category { get; }
    IPersonRepository Person { get; }
    IPackageRepository Package { get; }
    IReportRepository Report { get; }
    IContractRepository Contract { get; }
    IManagerRepository Manager { get; }
    void Save();
}
