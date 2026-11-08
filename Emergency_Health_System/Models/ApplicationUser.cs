using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Emergency_Health_System.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string? FullName { get; set; }
}
