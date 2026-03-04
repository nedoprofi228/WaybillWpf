using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public class DriverLicenseRepository(ApplicationContext context) : BaseRepository<DriveLicense>(context), IDriverLicensesRepository
{
}