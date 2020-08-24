using Avs.StaticSiteHosting.Models.Identity;
using System;
using System.Linq;
using MongoDB.Driver;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Services;

namespace Avs.StaticSiteHosting
{
    public static class DbInitialization
    {
        public static async Task InitDbAsync(MongoEntityRepository mongoRepository, PasswordHasher passwordHasher)
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
                Console.WriteLine("Base roles have been created.");
            }
            else
            {
                Console.WriteLine("Base roles already exist in database.");
            }

            // 2. Check admin user
            var usersCollection = mongoRepository.GetEntityCollection<User>(GeneralConstants.USERS_COLLECTION);
            var users = (await usersCollection.FindAsync(u => true)).ToList();

            if (!users.Any())
            {
                var adminRole = (await rolesCollection.FindAsync(r => r.Name == GeneralConstants.ADMIN_ROLE)).First();
                var admin = new User()
                {
                    Name = "Admin",
                    Email = "Admin@staticsitehosting.com",
                    Roles = new[] { adminRole },
                    Status = UserStatus.Active,
                    Password = passwordHasher.HashPassword("@Admin111")
                };

                await usersCollection.InsertOneAsync(admin);
                Console.WriteLine("Admin user has been created.");
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
            }
            
            Console.WriteLine("Db initialization completed.");
        }
    }
}