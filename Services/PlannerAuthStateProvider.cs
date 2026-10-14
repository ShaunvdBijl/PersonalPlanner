using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalPlanner.Data.Models;

namespace PersonalPlanner.Services;

public class PlannerAuthStateProvider : AuthenticationStateProvider
{
    private AppUser? _currentUser;

    public AppUser? CurrentUser => _currentUser;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_currentUser is null)
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _currentUser.Id.ToString()),
            new Claim("FirebaseUid", _currentUser.FirebaseUid),
            new Claim(ClaimTypes.Email, _currentUser.Email),
            new Claim(ClaimTypes.Name, _currentUser.DisplayName)
        };

        var identity = new ClaimsIdentity(claims, "FirebaseCrossChecked");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void SetAuthenticatedUser(AppUser user)
    {
        _currentUser = user;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void SignOut()
    {
        _currentUser = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
