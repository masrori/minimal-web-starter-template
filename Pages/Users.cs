using Orchestrate.Data;
using Orchestrate.Extensions;
using Orchestrate.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Orchestrate.Pages
{
    public static class Users
    {
        public static void MapUserEndPoints(this WebApplication app)
        {
            var group = app.MapGroup("/manage-users").RequireAuthorization();

            group.MapGet("/", Index);
            group.MapGet("/{id:guid}", GetById);
            group.MapPost("/", CreateAsync);
            group.MapPut("/{id:guid}", async (Guid id, User user, IDBClient db, HttpContext ctx) => await UpdateAsync(id, user, db, ctx));
            group.MapDelete("/{id:guid}", DeleteAsync);
        }
        private static async Task<IResult> Index(IDBClient db)
        {
            byte[] data = await Table.Select(new Column[]
            {
                new Column("applicantName", DbDataType.String),
                new Column("applicantAddress", DbDataType.String)
            }).From("""
                applicants AS ap
                INNER JOIN
                """)
            .Where((s) =>
            {
                return "";
            })
            .ExecuteAsync(db);
            return Results.Bytes(data, "application/octet-stream");
        }
        private static async Task<IResult> CreateAsync(User user, IDBClient db, HttpContext context)
        {
            string sql = """
                INSERT INTO users (id, firstName, lastName, email, phone, created_date, creator)
                VALUES (@id, @firstName, @lastName, @email, @phone, @created_date, @creator)
                """;
            bool result = await db.ExecuteNonQueryAsync(sql, new DbParameter[]
            {
                db.CreateParameter("@id", user.Id, System.Data.DbType.Guid),
                db.CreateParameter("@firstName", user.FirstName, System.Data.DbType.AnsiString),
                db.CreateParameter("@lastName", user.LastName, System.Data.DbType.AnsiString),
                db.CreateParameter("@email", user.Email, System.Data.DbType.AnsiString),
                db.CreateParameter("@creator", context.GetUserID(), System.Data.DbType.Guid)
            });
            if (result)
            {
                return Results.Redirect("/users");
            }
            return Results.Redirect("/users");
        }
        private static async Task<IResult> UpdateAsync(Guid id, [FromBody] User user, IDBClient db, HttpContext ctx)
        {
            string sql = """                
                UPDATE users
                SET
                firstName = @firstName,
                lastName = @lastName,
                email = @email,
                phone = @phone,
                editor = @editor,
                editedDate = GETDATE()
                WHERE id = @id
                """;
            var parameters = new DbParameter[]
            {
                db.CreateParameter("@firstName", user.FirstName, System.Data.DbType.AnsiString),
                db.CreateParameter("@lastName", user.LastName, System.Data.DbType.AnsiString),
                db.CreateParameter("@email", user.Email, System.Data.DbType.AnsiString),
                db.CreateParameter("@phone", user.Email, System.Data.DbType.AnsiString),
                db.CreateParameter("@editor", ctx.GetUserID(), System.Data.DbType.Guid),
                db.CreateParameter("@id", user.Id, System.Data.DbType.Guid)
            };
            await db.ExecuteNonQueryAsync(sql, parameters);
            return Results.Redirect("/users");
        }
        private static async Task<IResult> DeleteAsync(Guid id, IDBClient db, HttpContext ctx)
        {
            await db.ExecuteNonQueryAsync("UPDATE users SET deleted_date = GETDATE(), deleted_by = @user", db.CreateParameter("@user", ctx.GetUserID()));
            return Results.Redirect("/users");
        }
        private static async Task<IResult> GetById(IDBClient db, HttpContext context, string id)
        {
            if (Guid.TryParse(id, out var user_id))
            {

            }
            return Results.Redirect("/users");
        }
        private static async Task<IResult> UpdateAsync(IDBClient db, HttpContext context)
        {
            return Results.Redirect("/users");
        }
    }
}
