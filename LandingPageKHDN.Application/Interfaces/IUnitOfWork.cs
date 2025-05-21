using LandingPageKHDN.Domain.Entities;
using LandingPageKHDN.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<AdminAccount> AdminAccounts { get; }
        IRepository<CompanyRegistration> CompanyRegistrations { get; }
        IRepository<ActionLog> ActionLogs { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

    }
}
