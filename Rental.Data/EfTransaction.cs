using Microsoft.EntityFrameworkCore.Storage;
using MyProject.Core.Interfaces;

namespace MyProject.Data
{
    public class EfTransaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken ct) => _transaction.CommitAsync(ct);

        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
