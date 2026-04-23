using Orchestrate.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Orchestrate.Models;

namespace Orchestrate.Pages
{
    public static class Auth
    {
        public static void MapAuthEndPoints(this WebApplication app)
        {
            app.MapGet("/login", GetLoginForm);
            app.MapPost("/login", async (IDBClient db, HttpContext ctx) => await SubmitLoginAsync(db, ctx));
            app.MapPost("/logout", async (IDBClient db, HttpContext ctx) => await LogoutAsync(db, ctx))
                .RequireAuthorization();
            app.MapGet("/hello", async (IDBClient db, HttpContext ctx) =>
            {
                await ctx.Response.WriteAsJsonAsync("Hello");
            });
        }

        public static IResult GetLoginForm(IDBClient db, HttpContext ctx)
        {
            var html = """
                <html>
                <body>
                    <h2>Login</h2>
                    <form method="post" action="/login">
                        <input type="text" name="username" placeholder="Username" /><br/>
                        <input type="password" name="password" placeholder="Password" /><br/>
                        <button type="submit">Login</button>
                    </form>
                </body>
                </html>
            """;
            return Results.Content(html, "text/html");
        }

        public static async Task<IResult> SubmitLoginAsync(IDBClient db, HttpContext ctx)
        {
            var form = await ctx.Request.ReadFormAsync();
            var username = form["username"];
            var password = form["password"];

            var user = new User()
            {
                Id = 1,
                Email = "masrori@indo-soft.com",
                Password = "r6u0@2nfO"
            };  //await db.GetUserAsync(username);
            if (user == null || user.Password != password)
            {
                return Results.Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await ctx.SignInAsync("Cookies", principal);

            return Results.Redirect("/");
        }

        public static async Task<IResult> LogoutAsync(IDBClient db, HttpContext ctx)
        {
            await ctx.SignOutAsync("Cookies");
            return Results.Redirect("/login");
        }

    }
}
