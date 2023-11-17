using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MultipleDbContextsTransaction.Contexts;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;

namespace MultipleDbContextsTransaction
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }

    public interface IUnitOfWork<out TDbContext> : IUnitOfWork where TDbContext : DbContext
    {
        TDbContext DataContext { get; }
    }

    internal class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ReadOnlyCollection<DbContext> _dbContexts;

        public DbTransaction? Transaction { get; private set; } = null;

        public UnitOfWork(IEnumerable<DbContext> dbContexts)
        {
            _dbContexts = dbContexts.ToList().AsReadOnly();
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default)
        {
            var transaction = await _dbContexts[0].Database.BeginTransactionAsync(isolationLevel, ct); // check isolation
            Transaction = transaction.GetDbTransaction();

            for (var i = 1; i < _dbContexts.Count; i++)
            {
                await _dbContexts[i].Database.UseTransactionAsync(Transaction, cancellationToken: ct);
            }
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (Transaction == null)
            {
                await BeginTransactionAsync(ct: ct);
            }

            try
            {
                await SaveChangesAsync(ct);

                await Transaction!.CommitAsync(ct); //ToDo: Catch and rollback

                PostCommitCleanUp();
            }
            catch (Exception e)
            {
                await Transaction!.RollbackAsync(ct);

                PostCommitCleanUp();

                Console.WriteLine(e);
                throw;
            }
        }

        private void PostCommitCleanUp()
        {
            Transaction?.Dispose();
            Transaction = null;

            for (var i = 0; i < _dbContexts.Count; i++)
            {
                // due to bug
                _dbContexts[i].Database.CurrentTransaction?.Dispose();
            }
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            foreach (var dbContext in _dbContexts)
            {
                await dbContext.SaveChangesAsync(ct);
            }
        }

        public void Dispose()
        {
            Transaction?.Dispose();
            Console.WriteLine("Disposed UoW");
        }
    }

    internal class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext> where TDbContext : DbContext, ISharedTransactionDbContext
    {
        private readonly IUnitOfWork _unitOfWork;

        public TDbContext DataContext { get; }

        public UnitOfWork(IUnitOfWork unitOfWork, TDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            DataContext = dbContext;
        }

        public Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default)
        {
            return _unitOfWork.BeginTransactionAsync(isolationLevel, ct);
        }

        public Task CommitTransactionAsync(CancellationToken ct = default)
        {
            return _unitOfWork.CommitTransactionAsync(ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
