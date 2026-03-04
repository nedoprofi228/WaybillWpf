using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using WaybillWpf.Domain.Enums;

namespace WaybillWpf.Domain.Entities;

public class User: BaseEntity
{
    [Required]
    public string Login { get; set; }
    [Required]
    public string FullName { get; set; }
    
    [Required]
    public string Password { get; set; }
    

    public UserRole Role { get; set; }

    public User(string fullName, string login, string password, UserRole role)
    {
        Login = login;
        Password = password;
        FullName = fullName;
        Role = role;
    }

    public User()
    {
        
    }
}