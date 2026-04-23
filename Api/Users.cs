using Orchestrate.Data;
using System.Buffers.Binary;

namespace Orchestrate.Api
{
    public static class Users
    {
        public static void MapUserEndPoints(this WebApplication app)
        {
            app.MapGet("/api/users", GetUsersAsync);
            app.MapGet("/api/users/{id:int}", GetByIdAsync);
        }
        public static async Task<IResult> GetUsersAsync(IDBClient db, HttpContext ctx)
        {
            var cursorBase64 = ctx.Request.Query["page"].FirstOrDefault();

            int page = 1;
            int pageSize = 10;

            if (!string.IsNullOrEmpty(cursorBase64))
            {
                var bytes = Convert.FromBase64String(cursorBase64);

                if (bytes.Length >= 8)
                {
                    page = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0, 4));
                    pageSize = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(4, 4));
                }
            }
            var users = await db.GetUsersAsync(page, pageSize);

            Users.Execute("", "", new Column[]
            {
                new Column("i.applicantID", DbDataType.String),
                new Column("a.firstName", DbDataType.Int32),
            });
            return Results.Ok(new
            {
                page,
                pageSize,
                data = users
            });
        }
        public static async Task<IResult> GetByIdAsync(IDBClient db, int id)
        {
            var bytes = Array.Empty<byte>();
            Results.File(bytes, "applciation/octet-stream");
        }
        public static string Execute(string sForm, string sWhere, params Column[] columns)
        {

        }
    }
}
