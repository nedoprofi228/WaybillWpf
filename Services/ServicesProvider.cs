using Microsoft.Extensions.DependencyInjection;
using WaybillWpf.DataBase;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.ViewModels;
using WaybillWpf.ViewModels.Admin;
using WaybillWpf.Views;
using WaybillWpf.Views.Admin;
using System;

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
        .AddScoped<IStatisticService, StatisticService>()
        .AddScoped<IDriverManagementService, DriverManagementService>()
        .AddScoped<ICarManagementService, CarManagementService>()
        .AddScoped<IWaybillFlowService, WaybillFlowService>()
        .AddScoped<ICurrentUserService, CurrentUserService>()
    
        // --- ViewModels & Views ---
        .AddTransient<RegistrationViewModel>()
        .AddTransient<RegistrationView>()

        .AddTransient<LoginViewModel>()
        .AddTransient<LoginView>()

        .AddTransient<AdminMainViewModel>()
        .AddTransient<AdminMainView>()

        .AddTransient<AdminDriversViewModel>()
        .AddTransient<AdminDriversView>()

        .AddTransient<AdminCarsViewModel>()
        .AddTransient<AdminCarsView>()

        .AddTransient<AdminStatisticsViewModel>()
        .AddTransient<AdminStatisticsView>()

        .AddTransient<CarEditorViewModel>()
        .AddTransient<CarEditorView>()

        .AddTransient<DriverEditorViewModel>()
        .AddTransient<DriverEditorView>()

        .AddTransient<ManagerWaybillViewModel>()
        .AddTransient<ManagerWaybillView>()

        .AddTransient<NewWaybillViewModel>()
        .AddTransient<NewWaybillView>()

        .AddTransient<WaybillDetailEditorViewModel>()
        .AddTransient<WaybillDetailEditorView>()
        
        
        .BuildServiceProvider();
    

    public static T? GetService<T>()
    {
        return _serviceProvider.GetService<T>();
    }
}