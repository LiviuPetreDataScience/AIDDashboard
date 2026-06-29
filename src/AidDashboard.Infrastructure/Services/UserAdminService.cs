using AidDashboard.Application.Abstractions;
using AidDashboard.Application.Admin;
using AidDashboard.Application.Auth;
using AidDashboard.Domain.Identity;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="IUserAdminService"/>
public class UserAdminService : IUserAdminService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UserAdminService(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Username)
            .ToListAsync(cancellationToken);

        return users.Select(MapToDto).ToList();
    }

    public async Task<CreateUserResult> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = request.Username.Trim().ToLower();

        var usernameTaken = await _dbContext.Users
            .AnyAsync(user => user.Username.ToLower() == normalizedUsername, cancellationToken);
        if (usernameTaken)
        {
            return new CreateUserResult(null, UsernameAlreadyExists: true);
        }

        var user = new User
        {
            Username = request.Username.Trim(),
            DisplayName = request.DisplayName,
            Email = request.Email,
            Role = request.Role,
            IsActive = true,
            Provider = "Local",
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateUserResult(MapToDto(user), UsernameAlreadyExists: false);
    }

    public async Task<bool> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.DisplayName = request.DisplayName;
        user.Email = request.Email;
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int userId, ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return false;
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeactivateAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static UserDto MapToDto(User user) =>
        new(user.Id, user.Username, user.DisplayName, user.Email, user.Role, user.IsActive, user.MustChangePassword);
}
