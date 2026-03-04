using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;

namespace WaybillWpf.Domain.Interfaces;

public interface ICurrentUserService
{
    public User? CurrentUser { get; set; }
    
    public bool IsAuthenticated {get;}
    public bool IsAdmin { get; }
    public bool IsManager { get; }
    public bool IsDriver { get; }
    
    public void Logout();
}