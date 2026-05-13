using System.Net.Http.Json;
using System.Security.Claims;
using Ladestander.Web.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Ladestander.Web.Controllers;

[Route("web-auth")]
public class WebAuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WebAuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        var form = await HttpContext.Request.ReadFormAsync();

        var username = form["Username"].ToString();
        var password = form["Password"].ToString();

        var client = _httpClientFactory.CreateClient("LadestanderApi");

        var request = new LoginRequestDto(username, password);

        var response = await client.PostAsJsonAsync("api/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                return Redirect("/login?error=rate-limit");
            }

            return Redirect("/login?error=1");
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        if (loginResponse is null)
        {
            return Redirect("/login?error=1");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, loginResponse.Username),
            new Claim(ClaimTypes.Role, loginResponse.Role),
            new Claim("access_token", loginResponse.Token)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return Redirect("/customers");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect("/login");
    }
}