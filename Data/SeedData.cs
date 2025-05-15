using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TE_Project.Entities;

namespace TE_Project.Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { AdminRole.SuperAdmin, AdminRole.RegularAdmin };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Use a non-generic ILogger instead of ILogger<SeedData>
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Ensure database is created and migrated
                await context.Database.EnsureCreatedAsync();

                // Seed roles
                await SeedRolesAsync(roleManager);

                // Seed plants if none exist
                if (!context.Plants.Any())
                {
                    logger.LogInformation("Seeding plants...");
                    var sectors = new List<Plant>
                    {
                        new Plant { Name = "Sector 1" },
                        new Plant { Name = "Sector 2" },
                        new Plant { Name = "Sector 3" },
                        new Plant { Name = "Sector 4" },
                        new Plant { Name = "Sector 5" }
                    };

                    context.Plants.AddRange(sectors);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Plants seeded successfully.");
                }

                // Seed super admin if it doesn't exist
                if (!userManager.Users.Any(u => u.IsSuperAdmin))
                {
                    logger.LogInformation("Seeding super admin user...");
                    var superAdmin = new User
                    {
                        UserName = "admin@example.com",
                        Email = "admin@example.com",
                        FullName = "Super Admin",
                        PlantId = 1,
                        IsSuperAdmin = true,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(superAdmin, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(superAdmin, AdminRole.SuperAdmin);
                        logger.LogInformation("Super admin created successfully.");
                    }
                    else
                    {
                        logger.LogError("Failed to create super admin: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw; 
            }
        }
    }
}