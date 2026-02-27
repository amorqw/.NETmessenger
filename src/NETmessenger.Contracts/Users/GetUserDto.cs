namespace NETmessenger.Contracts.Users;

public record GetUserDto(Guid UniqueUserId, string Nickname, string Name, string PhoneNumber);