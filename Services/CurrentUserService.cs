using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Exceptions;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.Services;

public class CurrentUserService: ICurrentUserService
{
    private User? _currentUser;

    public User? CurrentUser
    {
        get => _currentUser;
        set
        {
            if(value == null)
                return;

            if (_currentUser != null)
                throw new UserAlreadyAuthorized();
            
            _currentUser = value;
        }
    }
    
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
    public bool IsEmployee => CurrentUser?.Role == UserRole.Employee;
    
    public void Logout()
    {
        _currentUser = null;
    }
}