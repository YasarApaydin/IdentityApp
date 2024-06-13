using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Models
{
    public class IdentitySeedData
    {
        private const string adminUser = "admin";
        private const string adminPassword = "Admin_123";

 

        public static async void IdentityTestUser(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();
            if (context.Database.GetAppliedMigrations().Any()) {
                context.Database.Migrate();
            }
            var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await userManager.FindByNameAsync(adminUser);
            if(user == null)
            {
                user = new AppUser
                {
                    FullName = "Yaşar Apaydın",
                    UserName = adminUser,
                   Email= "admin@yasarapaydin.com",
                   PhoneNumber= "01111111111"
                };


                await userManager.CreateAsync(user, adminPassword);

            }
        
        
        
        }

    }
}
