using NETmessenger.Contracts.Users;
using NETmessenger.Domain;
using NETmessenger.Domain.Entities;
using NETmessenger.Web.Infrastructure;
using NETmessenger.Web.Services.Exceptions;

namespace NETmessenger.Web.Services.Users;

public sealed class UserService(AppDbContext dbContext) : IUserService
{
    
    private readonly AppDbContext _dbContext = dbContext;
    public Task<IReadOnlyCollection<GetUserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = _dbContext.Users
            .Select(MapToDto)
            .OrderBy(u => u.Nickname, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<GetUserDto>>(users);
    }

    public Task<GetUserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        return Task.FromResult(user != null
            ? MapToDto(user)
            : null);
    }

    public Task<GetUserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        EnsureValid(dto.Nickname, dto.Name);
        
        //EnsureNicknameIsUnique(dto.Nickname, null);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nickname = dto.Nickname.Trim(),
            Name = dto.Name.Trim(),
            PhoneNumber = NormalizePhone(dto.PhoneNumber)
        };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        return Task.FromResult(MapToDto(user));
    }

    public Task<GetUserDto> UpdateAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken)
    {
        EnsureValid(dto.Nickname, dto.Name);
        
        var user = _dbContext.Users.FirstOrDefault(c => c.Id == userId);
        
        if (user == null)
        {
            throw new ResourceNotFoundException($"User '{userId}' was not found.");
        }

        //EnsureNicknameIsUnique(dto.Nickname, userId);

        user.Nickname = dto.Nickname.Trim();
        user.Name = dto.Name.Trim();
        user.PhoneNumber = NormalizePhone(dto.PhoneNumber);

        return Task.FromResult(MapToDto(user));
    }

    /* Закомментил, т.к. не работает
    private void EnsureNicknameIsUnique(string nickname, Guid? exceptUserId)
    {
        //Я не знаю, как починить string.Equals
        var alreadyUsed = _dbContext.Users.Any(u =>
            (!exceptUserId.HasValue || u.Id != exceptUserId.Value) &&
            string.Equals(u.Nickname, nickname.Trim(), StringComparison.OrdinalIgnoreCase));

        if (alreadyUsed)
        {
            throw new ConflictException("Nickname is already taken.");
        }
    }
    */
    
    private static void EnsureValid(string nickname, string name)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            throw new DomainValidationException("Nickname is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Name is required.");
        }
    }

    private static string? NormalizePhone(string? phoneNumber)
    {
        return string.IsNullOrWhiteSpace(phoneNumber)
            ? null
            : phoneNumber.Trim();
    }

    private static GetUserDto MapToDto(User user)
    {
        return new GetUserDto(
            UserId: user.Id,
            Nickname: user.Nickname,
            Name: user.Name,
            PhoneNumber: user.PhoneNumber);
    }
}
