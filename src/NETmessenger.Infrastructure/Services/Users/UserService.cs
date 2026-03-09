using Microsoft.EntityFrameworkCore;
using NETmessenger.Application.Abstractions.Users;
using NETmessenger.Application.Exceptions;
using NETmessenger.Contracts.Users;
using NETmessenger.Domain.Entities;
using NETmessenger.Infrastructure.Persistence;

namespace NETmessenger.Infrastructure.Services.Users;

public sealed class UserService(AppDbContext dbContext) : IUserService
{
    public async Task<IReadOnlyCollection<GetUserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Nickname)
            .Select(u => new GetUserDto(u.Id, u.Nickname, u.Name, u.PhoneNumber))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<GetUserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new GetUserDto(u.Id, u.Nickname, u.Name, u.PhoneNumber))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GetUserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        var nickname = NormalizeRequired(dto.Nickname, "Nickname is required.");
        var name = NormalizeRequired(dto.Name, "Name is required.");

        await EnsureNicknameIsUniqueAsync(nickname, null, cancellationToken);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nickname = nickname,
            Name = name,
            PhoneNumber = NormalizeOptional(dto.PhoneNumber)
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    public async Task<GetUserDto> UpdateAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var nickname = NormalizeRequired(dto.Nickname, "Nickname is required.");
        var name = NormalizeRequired(dto.Name, "Name is required.");

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
                   ?? throw new ResourceNotFoundException($"User '{userId}' was not found.");

        await EnsureNicknameIsUniqueAsync(nickname, userId, cancellationToken);

        user.Nickname = nickname;
        user.Name = name;
        user.PhoneNumber = NormalizeOptional(dto.PhoneNumber);

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    private async Task EnsureNicknameIsUniqueAsync(string nickname, Guid? exceptUserId, CancellationToken cancellationToken)
    {
        var loweredNickname = nickname.ToLowerInvariant();

        var exists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u =>
                (!exceptUserId.HasValue || u.Id != exceptUserId.Value) &&
                u.Nickname.ToLower() == loweredNickname,
                cancellationToken);

        if (exists)
        {
            throw new ConflictException("Nickname is already taken.");
        }
    }

    private static string NormalizeRequired(string value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException(errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static GetUserDto MapToDto(User user)
    {
        return new GetUserDto(user.Id, user.Nickname, user.Name, user.PhoneNumber);
    }
}
