using NETmessenger.Contracts.Users;

namespace NETmessenger.Web.Services.Users;

public interface IUserService
{
    Task<IReadOnlyCollection<GetUserDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<GetUserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<GetUserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken);
    Task<GetUserDto> UpdateAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken);
}
