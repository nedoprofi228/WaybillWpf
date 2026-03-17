using Microsoft.Extensions.DependencyInjection;
using WaybillWpf.DataBase;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.ViewModels;
using WaybillWpf.ViewModels.Admin;
using WaybillWpf.Views;
using WaybillWpf.Views.Admin;
using WaybillWpf.DataBase.Repositories.EF;
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
        .AddTransient<IWaybillTasksRepository, WaybillTaskRepository>()
        .AddScoped<IFuelTypesRepository, FuelTypeRepository>()

        // --- СЕРВИСЫ БИЗНЕС-ЛОГИКИ ---
        .AddScoped<IConvertToWordService, ConvertToWordService>()
        .AddScoped<IAuthService, AuthService>()
        .AddScoped<IStatisticService, StatisticService>()
        .AddScoped<IDriverManagementService, DriverManagementService>()
        .AddScoped<ICarManagementService, CarManagementService>()
        .AddScoped<IWaybillFlowService, WaybillFlowService>()
        .AddScoped<ICurrentUserService, CurrentUserService>()
        .AddScoped<IWaybillStateTransitionsService, WaybillStateTransitionsService>()
        .AddScoped<IConvertToExcelService, WaybillExcelExporter>()

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

        .AddTransient<AdminFuelTypesViewModel>()
        .AddTransient<AdminFuelTypesView>()

        .AddTransient<CarEditorViewModel>()
        .AddTransient<CarEditorView>()

        .AddTransient<FuelTypeEditorViewModel>()
        .AddTransient<FuelTypeEditorView>()

        .AddTransient<ManagerWaybillViewModel>()
        .AddTransient<ManagerWaybillView>()

        .AddTransient<NewWaybillViewModel>()
        .AddTransient<NewWaybillView>()

        .AddTransient<WaybillDetailEditorViewModel>()
        .AddTransient<WaybillDetailEditorView>()

        .AddTransient<WaybillTaskEditorViewModel>()
        .AddTransient<WaybillTaskEditorView>()

        .AddTransient<DriverManagementView>()
        .AddTransient<DriverManagementViewModel>()

        .AddTransient<DriverRegistrationViewModel>()
        .AddTransient<DriverRegistrationView>()

        .AddTransient<ReasonEditorViewModel>()
        .AddTransient<ReasonEditorView>()

        .BuildServiceProvider();


    public static T? GetService<T>()
    {
        return _serviceProvider.GetService<T>();
    }
}