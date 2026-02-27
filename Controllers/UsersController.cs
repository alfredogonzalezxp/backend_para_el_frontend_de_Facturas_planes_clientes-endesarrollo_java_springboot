using api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Email,
                    u.Rol
                })
                .ToListAsync();

            return Ok(users);
        }

        /*
         * TEMPLATE: GET by ID
         * URL: GET /api/users/5
         * 
         * {id} in the route is a "Route Parameter".
         * Whatever number the user puts in the URL gets passed to the method.
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            // FindAsync: Searches the table by Primary Key (Id).
            // If not found, returns null.
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" }); // HTTP 404
            }

            return Ok(new
            {
                user.Id,
                user.Nombre,
                user.Email,
                user.Rol
            });
        }

        /*
         * TEMPLATE: UPDATE
         * URL: PUT /api/users/5
         * 
         * Receives the user ID from the URL and new data from the body.
         * Finds the existing user, updates the fields, and saves.
         */
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" }); // HTTP 404
            }

            // Update only the fields that were sent
            user.Nombre = dto.Nombre;
            user.Email = dto.Email;
            user.Rol = dto.Rol;

            // SaveChangesAsync: Commits the changes to the database (UPDATE query)
            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully" });
        }

        /*
         * TEMPLATE: DELETE
         * URL: DELETE /api/users/5
         * 
         * Finds the user by ID, removes it from the context, and saves.
         */
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" }); // HTTP 404
            }

            // Remove: Stages the deletion (like Add, but in reverse)
            _context.Users.Remove(user);

            // SaveChangesAsync: Executes DELETE FROM users WHERE id = ...
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully" });
        }
    }

    /*
     * DTO for the Update endpoint.
     * This defines what fields the frontend must send in the body.
     * You can add or remove fields as needed.
     */
    public class UpdateUserDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
