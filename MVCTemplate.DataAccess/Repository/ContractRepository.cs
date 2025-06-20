using Microsoft.EntityFrameworkCore;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.DataAccess.Repository.IRepository;
using MVCTemplate.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Contract = MVCTemplate.Models.Contract;

namespace MVCTemplate.DataAccess.Repository
{
    public class ContractRepository : Repository<Contract>, IContractRepository
    {
        private readonly ApplicationDbContext _db;

        public ContractRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Add(Contract contract)
        {
            _db.Contracts.Update(contract);
        }

        public Contract? CheckIfUnique(string name)
        {
            return _db.Contracts.FirstOrDefault(i => i.Name == name);
        }

        public Contract? ContinueIfNoChangeOnUpdate(string name, int countryId)
        {
            return _db.Contracts.FirstOrDefault(i => i.Name == name && i.Id != countryId);
        }

        public void Delete(Contract entity)
        {
            throw new NotImplementedException();
        }

        public Contract Get(Expression<Func<Contract, bool>> filter, string? includeProperties = null)
        {
            var contract = _db.Set<Contract>().FirstOrDefault(filter);
            if (contract != null)
            {
                _db.Set<Contract>().Remove(contract);
                _db.SaveChanges();
                return contract;
            }

            throw new InvalidOperationException("Contract not found."); // for delete
        }

        public IEnumerable<Contract> GetAll(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }

        public Contract GetFirstOrDefault(Expression<Func<Contract, bool>> predicate)
        {
            return _db.Contracts.FirstOrDefault(predicate);
        } //since it cant be in interface repository

        public Contract GetNoTracking(Expression<Func<Contract, bool>> filter)
        {
            return _db.Set<Contract>()
                      .AsNoTracking()
                      .FirstOrDefault(filter); // for delete to stop erroring
        }

        public void Remove(Contract contract)
        {
            _db.Set<Contract>().Remove(contract);
        }

        public void RemoveRange(IEnumerable<Contract> entity)
        {
            throw new NotImplementedException();
        }

        public List<Contract> ToList()
        {
            return _db.Contracts.ToList();
        }

        public void Update(Contract updatedContract)
        {
            var existing = _db.Contracts.FirstOrDefault(c => c.Id == updatedContract.Id);
            if (existing == null) return;

            existing.Name = updatedContract.Name;
            existing.Description = updatedContract.Description;
            existing.Validity = updatedContract.Validity;
            existing.PersonId = updatedContract.PersonId;
            existing.UpdatedAt = DateTime.Now;

            _db.SaveChanges();
        }

        Contract? IContractRepository.CheckIfUnique(string name)
        {
            return _db.Contracts.FirstOrDefault(i => i.Name == name);
        }

        Contract? IContractRepository.ContinueIfNoChangeOnUpdate(string name, int Id)
        {
            return _db.Contracts.FirstOrDefault(i => i.Name == name && i.Id != Id);
        }

        public void Attach(Contract entity)
        {
            _db.Contracts.Attach(entity); // for delete to stop erroring
        }
    }
}
