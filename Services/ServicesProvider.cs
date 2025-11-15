using Microsoft.Extensions.DependencyInjection;
using WaybillWpf.Core.Interfaces;
using WaybillWpf.DataBase;

namespace WaybillWpf.Services;

public class ServicesProvider
{
    private static readonly IServiceProvider _serviceProvider = new ServiceCollection()
        .AddDbContextFactory<ApplicationContext>()
    
        // --- Репозитории ---
        .AddScoped<ICarsRepository, CarRepository>()
        .AddScoped<IDriverLicensesRepository, DriverLicenseRepository>()
        .AddScoped<IDriversRepository, DriverRepository>()
        .AddScoped<IUsersRepository, UserRepository>()
        .AddScoped<IWaybillsRepository, WaybillRepository>()
        .AddScoped<IWaybillDetailsRepository, WaybillDetailsRepository>()

        // --- СЕРВИСЫ БИЗНЕС-ЛОГИКИ ---
        .AddScoped<IAuthService, AuthService>() 
        .AddScoped<IStatisticService, StatisticService>() // <-- Новый
        .AddScoped<IDriverManagementService, DriverManagementService>() // <-- Новый
        .AddScoped<ICarManagementService, CarManagementService>() // <-- Новый
        .AddScoped<IWaybillFlowService, WaybillFlowService>() // <-- Новый
    
        .BuildServiceProvider();
    

    public static T? GetService<T>()
    {
        return _serviceProvider.GetService<T>();
    }
    
}