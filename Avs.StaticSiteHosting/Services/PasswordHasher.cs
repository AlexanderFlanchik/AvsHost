using BCryptHasher = BCrypt.Net.BCrypt;

namespace Avs.StaticSiteHosting.Services
{
    public class PasswordHasher
    {
        public static string PasswordSalt => BCryptHasher.GenerateSalt(4);
        public string HashPassword(string enteredPassword)
        {
            var hashed = BCryptHasher.HashPassword(enteredPassword, PasswordSalt);
            return hashed;
        }

        public bool VerifyPassword(string passwordHashed, string passwordEntered)
        {
            try
            {
                return BCryptHasher.Verify(passwordEntered, passwordHashed);
            }
            catch
            {
                return false;
            }
        }
    }
}