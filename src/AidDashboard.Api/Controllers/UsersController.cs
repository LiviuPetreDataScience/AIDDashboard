using AidDashboard.Application.Admin;
using AidDashboard.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>Administrative user management. Restricted to administrators.</summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;

    public UsersController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userAdminService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userAdminService.CreateAsync(request, cancellationToken);
        if (result.UsernameAlreadyExists)
        {
            return Conflict(new { message = "A user with that username already exists." });
        }

        return CreatedAtAction(nameof(GetAll), new { id = result.User!.Id }, result.User);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var updated = await _userAdminService.UpdateAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var reset = await _userAdminService.ResetPasswordAsync(id, request, cancellationToken);
        return reset
            ? NoContent()
            : BadRequest(new { message = "User not found or the new password is invalid (minimum 6 characters)." });
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var deactivated = await _userAdminService.DeactivateAsync(id, cancellationToken);
        return deactivated ? NoContent() : NotFound();
    }
}
