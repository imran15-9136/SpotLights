using SpotLights.Shared.Extensions;
using SpotLights.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotLights.Shared.Entities.Identity;
using SpotLights.Domain.Model.Identity;
using SpotLights.Core.Interfaces.Identity;
using SpotLights.Infrastructure.Identity;

namespace SpotLights.Interfaces;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
  private readonly IUserService _userProvider;
  private readonly UserManager _userManager;

  public UserController(IUserService userProvider, UserManager userManager)
  {
    _userProvider = userProvider;
    _userManager = userManager;
  }

  [HttpGet("items")]
  public async Task<IEnumerable<UserInfoDto>> GetItemsAsync()
  {
    bool isAdmin = User.IsAdmin();
    return await _userProvider.GetAsync(isAdmin);
  }

  [HttpGet("{id:int}")]
  public async Task<UserInfoDto?> GetAsync([FromRoute] int id)
  {
    bool isAdmin = User.IsAdmin();
    return await _userProvider.GetAsync(id, isAdmin);
  }

  [HttpPut("{id:int?}")]
  public async Task<IActionResult> EditorAsync(
      [FromRoute] int? id,
      [FromBody] UserEditorDto input
  )
  {
    bool isAdmin = User.IsAdmin();

    if (!id.HasValue)
    {
      if (!isAdmin && input.Type == UserType.Administrator)
      {
        return StatusCode(
            403,
            new { error = "User does not have permission to add user." }
        );
      }

      UserInfo user =
          new(input.UserName)
          {
            NickName = input.NickName,
            Email = input.Email,
            Avatar = input.Avatar,
            Bio = input.Bio,
            Type = input.Type,
          };
      Microsoft.AspNetCore.Identity.IdentityResult result = await _userManager.CreateAsync(
          user,
          input.Password!
      );
      if (!result.Succeeded)
      {
        Microsoft.AspNetCore.Identity.IdentityError error = result.Errors.First();
        return Problem(detail: error.Description, title: error.Code);
      }
    }
    else
    {
      UserInfo user = await _userProvider.FindAsync(id.Value);
      user.NickName = input.NickName;
      user.Avatar = input.Avatar;
      user.Bio = input.Bio;
      user.Type = input.Type;

      if (
          !isAdmin
          && (user.Type == UserType.Administrator || input.Type == UserType.Administrator)
      )
      {
        return StatusCode(
            403,
            new { error = "User does not have permission to update user." }
        );
      }

      Microsoft.AspNetCore.Identity.IdentityResult result = await _userManager.UpdateAsync(
          user
      );
      if (result.Succeeded)
      {
        if (!string.IsNullOrEmpty(input.Password))
        {
          string token = await _userManager.GeneratePasswordResetTokenAsync(user);
          result = await _userManager.ResetPasswordAsync(user, token, input.Password);

          if (result.Succeeded)
            return Ok();
        }
        return Ok();
      }
      Microsoft.AspNetCore.Identity.IdentityError error = result.Errors.First();
      return Problem(detail: error.Description, title: error.Code);
    }

    return Ok();
  }

  [HttpGet("isAdminUser")]
  public bool IsAdminUser()
  {
    return User.IsAdmin();
  }
}
