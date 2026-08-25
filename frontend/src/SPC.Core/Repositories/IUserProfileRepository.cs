using SPC.Core.Models;

namespace SPC.Core.Repositories;

public interface IUserProfileRepository
{
    Task<IReadOnlyList<UserProfileDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<UserProfileDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveAsync(UserProfileDto profile, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
