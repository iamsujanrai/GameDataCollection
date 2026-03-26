using GameDataCollection.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GameDataCollection.DbContext.Sedding
{
    public static class SeedData
    {
        public static async Task SeedSpinDefaults(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetRequiredService<UserDbContext>();

            if (!await db.SpinSettings.AnyAsync())
            {
                db.SpinSettings.Add(new SpinSetting { CooldownHours = 24, MaxSpinsPerPeriod = 5 });
                await db.SaveChangesAsync();
            }

            if (!await db.SpinPrizes.AnyAsync())
            {
                db.SpinPrizes.AddRange(
                    new SpinPrize { Label = "$1",  Amount = 1,   Weight = 40, SortOrder = 0, IsActive = true },
                    new SpinPrize { Label = "$2",  Amount = 2,   Weight = 30, SortOrder = 1, IsActive = true },
                    new SpinPrize { Label = "$5",  Amount = 5,   Weight = 15, SortOrder = 2, IsActive = true },
                    new SpinPrize { Label = "$10", Amount = 10,  Weight = 10, SortOrder = 3, IsActive = true },
                    new SpinPrize { Label = "$20", Amount = 20,  Weight = 4,  SortOrder = 4, IsActive = true },
                    new SpinPrize { Label = "$50", Amount = 50,  Weight = 1,  SortOrder = 5, IsActive = true }
                );
                await db.SaveChangesAsync();
            }
        }

        public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            // Create Roles
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));

            if (!await roleManager.RoleExistsAsync(Roles.User))
                await roleManager.CreateAsync(new IdentityRole(Roles.User));

            // Create Admin User
            var adminEmail = "admin@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Admin"
                };

                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }
    }
}
