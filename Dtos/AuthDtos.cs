using System.ComponentModel.DataAnnotations;

namespace api.Dtos
{
    public record LoginDto(
        [Required] string Email, 
        [Required] string Password
    );

    public record RegisterDto(
        [Required] string Nombre,
        [Required] [EmailAddress] string Email,
        [Required] string Password,
        [Required] string Rol
    );
}
