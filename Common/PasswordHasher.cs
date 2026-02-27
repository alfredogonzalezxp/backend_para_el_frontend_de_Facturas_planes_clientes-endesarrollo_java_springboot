namespace api.Common
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hashedPassword);
    }

    public class PasswordHasher : IPasswordHasher
    {
        /*
         * SUMMARY EXPLANATION:
         * Why no "new BCrypt()"?
         * 
         * 1. Static Class:
         *    - "BCrypt.Net.BCrypt" is a STATIC class. 
         *    - It is a global "toolbox" that cannot be created/instantiated.
         *    - You cannot write: var x = new BCrypt(); // Error
         * 
         * 2. Direct Access:
         *    - You access its tools directly by name: 
         *      BCrypt.Net.BCrypt.HashPassword(...)
         *    - Think of it like "Math" (Math.Max, Math.Min). You don't create "new Math()".
         *
         * 3. Where does it come from?
         *    - It is NOT in this file. 
         *    - It is a "NuGet Package" (External Library) installed in your project.
         *    - Check "api.csproj": <PackageReference Include="BCrypt.Net-Next" ... />
         *    - The compiler knows where to find it because you installed that package.
         */
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
