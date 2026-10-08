using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SurveyUP.Data.Enums;
using SurveyUP.Models;
using SurveyUP.Models.Tables;

namespace SurveyUP.Data
{
    /// <summary>
    /// Optional database bootstrap, driven by configuration:
    ///   Database:Provider  = SqlServer (default) | Sqlite
    ///   Seed:Roles         = true  -> make sure all application roles exist
    ///   Seed:AdminEmail / Seed:AdminPassword -> create the first administrator
    ///   Seed:DemoData      = true  -> add demo accounts and a sample survey (fictional data only)
    /// With SQLite the schema is created from the model (no migrations needed).
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var config = services.GetRequiredService<IConfiguration>();
            var domain = services.GetRequiredService<N3mikosContext>();
            var identity = services.GetRequiredService<SurveyUpIdDbContext2>();

            if (domain.Database.IsSqlite())
            {
                // SQLite needs index names unique per database (SQL Server only per table), so the
                // scripts are executed statement by statement and a duplicate index name is skipped.
                foreach (DbContext ctx in new DbContext[] { identity, domain })
                {
                    await EnsureSqliteSchemaAsync(ctx);
                }
            }

            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            bool demo = config.GetValue<bool>("Seed:DemoData");
            if (config.GetValue<bool>("Seed:Roles") || demo || !string.IsNullOrEmpty(config["Seed:AdminEmail"]))
            {
                foreach (Roles role in Enum.GetValues(typeof(Roles)))
                {
                    if (!await roleManager.RoleExistsAsync(role.ToString()))
                        await roleManager.CreateAsync(new ApplicationRole(role.ToString()));
                }
            }

            var adminEmail = config["Seed:AdminEmail"];
            var adminPassword = config["Seed:AdminPassword"];
            if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
                await EnsureUserAsync(userManager, adminEmail, adminPassword, "Admin", "Anna", Roles.Administrator);

            if (demo)
            {
                var password = config["Seed:DemoPassword"] ?? "Demo!2345";
                await EnsureUserAsync(userManager, "admin@demo.local", password, "Nowak", "Anna", Roles.Administrator);
                await EnsureUserAsync(userManager, "tworca@demo.local", password, "Kowalski", "Piotr", Roles.Twórca);
                await EnsureUserAsync(userManager, "sekretariat@demo.local", password, "Wiśniewska", "Marta", Roles.Sekretariat);
                await EnsureUserAsync(userManager, "student@demo.local", password, "Zieliński", "Jakub", Roles.Student);
                await DemoData.SeedAsync(domain, userManager);
            }
        }

        private static async Task EnsureSqliteSchemaAsync(DbContext ctx)
        {
            var connection = ctx.Database.GetDbConnection();
            await connection.OpenAsync();
            try
            {
                var statements = System.Text.RegularExpressions.Regex
                    .Split(ctx.Database.GenerateCreateScript(), @";\s*\r?\n")
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0);
                foreach (var statement in statements)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = statement;
                    try { await command.ExecuteNonQueryAsync(); }
                    catch (Microsoft.Data.Sqlite.SqliteException e) when (e.Message.Contains("already exists")) { /* existing schema or duplicate index name */ }
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        private static async Task EnsureUserAsync(UserManager<ApplicationUser> users, string email, string password, string surname, string firstName, Roles role)
        {
            var user = await users.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    Name = surname,
                    FirstName = firstName,
                    PhoneNumber = "000000000"
                };
                var result = await users.CreateAsync(user, password);
                if (!result.Succeeded)
                    throw new InvalidOperationException("Could not create seed user " + email + ": " + string.Join("; ", result.Errors.Select(e => e.Description)));
            }
            if (!await users.IsInRoleAsync(user, role.ToString()))
                await users.AddToRoleAsync(user, role.ToString());
        }
    }
}
