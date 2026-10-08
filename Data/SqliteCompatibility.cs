using System;
using System.Data.Common;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SurveyUP.Data
{
    /// <summary>
    /// Registers the few SQL Server functions used in column defaults (getdate, newid)
    /// so the same model can run on SQLite for local development and demos.
    /// </summary>
    public class SqliteCompatibilityInterceptor : DbConnectionInterceptor
    {
        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            Register(connection);
        }

        public override System.Threading.Tasks.Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, System.Threading.CancellationToken cancellationToken = default)
        {
            Register(connection);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private static void Register(DbConnection connection)
        {
            if (connection is SqliteConnection sqlite)
            {
                sqlite.CreateFunction("getdate", () => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sqlite.CreateFunction("newid", () => Guid.NewGuid().ToString().ToUpperInvariant());
            }
        }
    }
}
