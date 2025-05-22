﻿namespace MVCTemplate.DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    IProductRepository Product { get; } 

    ICategoryRepository Category { get; }
    IPersonRepository Person { get; }
    void Save();
}
