using Avs.StaticSiteHosting.Web.Models.Identity;
using Avs.StaticSiteHosting.Web.Services;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Linq;
using Avs.StaticSiteHosting.Web;
using Microsoft.Extensions.Logging;

namespace Avs.StaticSiteHosting.DataMigrator
{
    public class DbInitializer(MongoEntityRepository mongoRepository, PasswordHasher passwordHasher, ILogger<DbInitializer> logger)
    {
        public async Task InitDbAsync()
        {
            // 1. Check roles
            var rolesCollection = mongoRepository.GetEntityCollection<Role>(GeneralConstants.ROLES_COLLECTION);
            var roles = (await rolesCollection.FindAsync(r => true))
                .ToList();

            if (!roles.Any())
            {
                var userRole = new Role { Name = GeneralConstants.DEFAULT_USER_ROLE };
                var adminRole = new Role { Name = GeneralConstants.ADMIN_ROLE };
                await rolesCollection.InsertManyAsync(new[] { userRole, adminRole });
                logger.LogInformation("Base roles have been created.");
            }
            else
            {
                logger.LogInformation("Base roles already exist in database.");
            }

            // 2. Check admin user
            var usersCollection = mongoRepository.GetEntityCollection<User>(GeneralConstants.USERS_COLLECTION);
            var users = (await usersCollection.FindAsync(u => true)).ToList();

            if (!users.Any())
            {
                var adminRole = (await rolesCollection.FindAsync(r => r.Name == GeneralConstants.ADMIN_ROLE)).First();
                
                // Of course, no one will create a real user with password like this, this is just for demo
                var admin = new User()
                {
                    Name = "Admin",
                    Email = "Admin@staticsitehosting.com",
                    Roles = new[] { adminRole },
                    Status = UserStatus.Active,
                    Password = passwordHasher.HashPassword("@Admin111")
                };

                await usersCollection.InsertOneAsync(admin);
                logger.LogInformation("Admin user has been created.");
            }
            else
            {
                logger.LogInformation("Admin user already exists.");
            }

            logger.LogInformation("Db initialization completed.");
        }
    }
}
