using Lookif.Layers.Core.MainCore.Identities;
using System.Threading;
using System.Threading.Tasks; 

namespace Lookif.Layers.Data.Contracts
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByUserAndPass(string username, string password, CancellationToken cancellationToken);

        Task AddAsync(User user, string password, CancellationToken cancellationToken);
        Task UpdateSecuirtyStampAsync(User user, CancellationToken cancellationToken);
        Task UpdateLastLoginDateAsync(User user, CancellationToken cancellationToken);
    }
}