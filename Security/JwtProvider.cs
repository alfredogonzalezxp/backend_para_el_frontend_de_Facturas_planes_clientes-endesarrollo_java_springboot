using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace api.Security
{
    public interface IJwtProvider
    {
        string Generate(User user);
    }

    public class JwtProvider : IJwtProvider
    {
        /*
         * SUMMARY EXPLANATION:
         * private readonly IConfiguration _configuration;
         * 
         * 1. IConfiguration:
         *    - This is the interface to access your "appsettings.json" file.
         *    - It allows you to read settings like "Jwt:Key", "ConnectionStrings", etc.
         * 
         * 2. readonly:
         *    - Safety rule: This variable can ONLY be assigned a value inside the Constructor.
         *    - Once set, it cannot be changed elsewhere in the class.
         * 
         * 3. _configuration:
         *    - The variable that holds the configuration data so we can use it later (e.g., inside Generate()).
         */
        private readonly IConfiguration _configuration;
        public JwtProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Generate(User user)
        {
            /*
             * SUMMARY EXPLANATION:
             * 
             * 1. Get the Password ("The Secret"):
             *    - Retrieves the secret string from appsettings.json (e.g., "super-secret-key").
             *    - This key is the only thing proving the token is real.
             */
            var secretKey = _configuration["Jwt:Key"];

            /*
             * 2. Prepare the Key ("The Ink"):
             *    - Converts the string password into Bytes (computer readable format).
             *    - Wraps it in a "SymmetricSecurityKey" object (Symmetric = same key to lock and unlock).
             */
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));

            /*
             * 3. Create the Signature ("The Wax Seal"):
             *    - Combines the Key (securityKey) + The Algorithm (HmacSha256).
             *    - This "credentials" object is used to effectively "sign" the token so no one can fake it.
             */
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            /*
             * SUMMARY EXPLANATION:
             * 
             * 4. Create the Claims ("The ID Card Details"):
             *    - "Claims" are pieces of information written INSIDE the token.
             *    - When the frontend sends this token back, we can read these values to know who they are.
             */
            var claims = new[]
            {
                // 1. Who is this? (ID)
                // "NameIdentifier" is the standard way to store the User ID.
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                // 2. How to contact? (Email)
                // Storing the email so we don't have to look it up in the database again.
                new Claim(ClaimTypes.Email, user.Email),

                // 3. What can they do? (Role)
                // "Role" tells the app if they are "Admin", "User", etc. used for [Authorize(Roles="Admin")]
                new Claim(ClaimTypes.Role, user.Rol)
            };

            /*
             * SUMMARY EXPLANATION:
             * 
             * 5. Build the Final Token ("The Assembly"):
             *    - This creates the actual "Passport" object using all the parts we prepared.
             */
            var token = new JwtSecurityToken(
                // a) Issuer (Who made this?):
                // Defined in appsettings.json (e.g., "my-api").
                _configuration["Jwt:Issuer"],

                // b) Audience (Who is this for?):
                // Defined in appsettings.json (e.g., "my-frontend-app").
                _configuration["Jwt:Audience"],

                // c) Claims (The Info inside):
                // The list of ID, Email, Role we created earlier.
                claims,

                // d) Expiration (The Expiry Date):
                // The token is valid for 60 minutes from NOW. 
                // After 60 mins, the user is logged out automatically.
                expires: DateTime.Now.AddMinutes(60),

                // e) Signature (The Stamp):
                // Uses our Secret Key + Algorithm to "seal" the token.
                signingCredentials: credentials);

            /*
             * SUMMARY EXPLANATION:
             * 
             * 6. Convert to String ("The Translator"):
             *    - "JwtSecurityTokenHandler" is a class that knows how to write tokens.
             *    - .WriteToken(token):
             *      - Takes the complex C# "token" object (which is in memory).
             *      - Converts it into the final Compact String format (e.g., "eyJhbGci...").
             *      - This string is what you actually send back to the user/frontend.
             */
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
