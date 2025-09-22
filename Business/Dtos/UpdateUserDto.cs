using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Dtos
{
    public  class UpdateUserDto
    {
        [Required] public string FirstName { get; set; } = null!;
        [Required] public string LastName { get; set; } = null!;
        [Required, EmailAddress] public string Email { get; set; } = null!;

        [MinLength(6)] public string? NewPassword { get; set; } 

    }
}
