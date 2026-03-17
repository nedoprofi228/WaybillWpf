# 3.1.1 SQL-определения запросов на выборку (не менее 10)

В базе данных используется Entity Framework Core для работы с PostgreSQL. Ниже представлены запросы на выборку, используемые для получения данных в приложении: сначала их реализация с помощью LINQ (EF Core), а затем их эквивалент на языке SQL.

**1. Получение всех автомобилей с указанием типа топлива:**

*Entity Framework (LINQ):*
```csharp
var cars = await context.Cars
    .Include(c => c.FuelType)
    .ToListAsync();
```

*SQL:*
```sql
SELECT c."Id", c."Model", c."CarNumber", c."FuelRate", f."Name" AS "FuelTypeName"
FROM "Cars" c
LEFT JOIN "FuelTypes" f ON c."FuelTypeId" = f."Id";
```

**2. Получение всех путевых листов для определенного водителя:**

*Entity Framework (LINQ):*
```csharp
var waybills = await context.Waybills
    .Where(w => w.DriverId == 1)
    .ToListAsync();
```

*SQL:*
```sql
SELECT "Id", "AtCreated", "LogistId", "CarId", "WaybillStatus", "ReasonOfDecline"
FROM "Waybills"
WHERE "DriverId" = 1;
```

**3. Получение списка всех пользователей с ролью «Водитель»:**

*Entity Framework (LINQ):*
```csharp
var drivers = await context.Users
    .OfType<Driver>()
    .ToListAsync();
```

*SQL:*
```sql
SELECT "Id", "Login", "FullName"
FROM "Users"
WHERE "UserType" = 'Driver';
```

**4. Выборка деталей путевого листа по ID путевого листа:**

*Entity Framework (LINQ):*
```csharp
var details = await context.WaybillDetails
    .Where(wd => wd.WaybillId == 5)
    .ToListAsync();
```

*SQL:*
```sql
SELECT "Id", "DepartureDateTime", "ArrivalDateTime", "StartMealing", "EndMealing"
FROM "WaybillDetails"
WHERE "WaybillId" = 5;
```

**5. Подсчет суммарного расхода топлива по путевым листам для конкретной машины:**

*Entity Framework (LINQ):*
```csharp
var fuelConsumptionByCar = await context.Waybills
    .Where(w => w.CarId == carId)
    .SelectMany(w => w.WaybillDetails)
    .GroupBy(wd => w.CarId)
    .Select(g => new 
    {
        CarId = g.Key,
        TotalFuel = g.Sum(wd => wd.FuelConsumed)
    })
    .ToListAsync();
```

*SQL:*
```sql
SELECT w."CarId", SUM(wd."FuelConsumed") AS "TotalFuel"
FROM "Waybills" w
JOIN "WaybillDetails" wd ON w."Id" = wd."WaybillId"
GROUP BY w."CarId";
```

**6. Получение всех путевых листов, созданных за сегодняшний день:**

*Entity Framework (LINQ):*
```csharp
var todayWaybills = await context.Waybills
    .Where(w => w.AtCreated.Date == DateTime.UtcNow.Date)
    .ToListAsync();
```

*SQL:*
```sql
SELECT * FROM "Waybills"
WHERE DATE("AtCreated") = CURRENT_DATE;
```

**7. Выборка заданий (маршрутов) для конкретного путевого листа:**

*Entity Framework (LINQ):*
```csharp
var tasks = await context.WaybillTasks
    .Where(wt => wt.WaybillId == 12)
    .ToListAsync();
```

*SQL:*
```sql
SELECT "Date", "DeparturePoint", "ArrivalPoint", "Mileage", "CustomerName"
FROM "WaybillTasks"
WHERE "WaybillId" = 12;
```

**8. Получение путевых листов со статусом «Черновик» (Draft - 0):**

*Entity Framework (LINQ):*
```csharp
var draftWaybills = await context.Waybills
    .Where(w => w.WaybillStatus == WaybillStatus.Draft)
    .ToListAsync();
```

*SQL:*
```sql
SELECT "Id", "AtCreated", "DriverId", "CarId"
FROM "Waybills"
WHERE "WaybillStatus" = 0;
```

**9. Выборка автомобилей с расходом топлива больше 10 литров:**

*Entity Framework (LINQ):*
```csharp
var highFuelCars = await context.Cars
    .Where(c => c.FuelRate > 10)
    .ToListAsync();
```

*SQL:*
```sql
SELECT "Model", "CarNumber", "FuelRate"
FROM "Cars"
WHERE "FuelRate" > 10;
```

**10. Выборка общих затрат на заправку по каждой детали путевого листа:**

*Entity Framework (LINQ):*
```csharp
// TotalFuelCost является вычисляемым свойством в Entity (RefueledAmount * FuelPriceAtRefueling)
var refuelingCosts = await context.WaybillDetails
    .Where(wd => wd.RefueledAmount > 0)
    .Select(wd => new
    {
        WaybillDetailId = wd.Id,
        wd.WaybillId,
        TotalCost = wd.TotalFuelCost
    })
    .ToListAsync();
```

*SQL:*
```sql
SELECT "Id", "WaybillId", ("RefueledAmount" * "FuelPriceAtRefueling") AS "TotalCost"
FROM "WaybillDetails"
WHERE "RefueledAmount" > 0;
```

---

# 3.1.2 SQL-определения представлений (не менее 5)

В Entity Framework представления можно заменить LINQ-запросами с проекцией в DTO/анонимные типы, однако для отчета часто приводятся как виртуальные таблицы на стороне БД.

**1. Представление для отображения полной информации об автомобиле:**

*Entity Framework (Собственный DTO):*
```csharp
var carFullInfo = await context.Cars
    .Include(c => c.FuelType)
    .Select(c => new
    {
        c.Id,
        c.Model,
        c.CarNumber,
        c.FuelRate,
        FuelType = c.FuelType.Name,
        Price = c.FuelType.Price
    })
    .ToListAsync();
```

*SQL-представление:*
```sql
CREATE VIEW "vw_CarFullInfo" AS
SELECT c."Id", c."Model", c."CarNumber", c."FuelRate", f."Name" AS "FuelType", f."Price"
FROM "Cars" c
LEFT JOIN "FuelTypes" f ON c."FuelTypeId" = f."Id";
```

**2. Представление для отображения водителей и их лицензий:**

*Entity Framework (Собственный DTO):*
```csharp
var driverLicensesInfo = await context.Users
    .OfType<Driver>()
    .Include(d => d.DriveLicense)
    .Select(d => new
    {
        d.Id,
        d.FullName,
        d.Login,
        LicenseNumber = d.DriveLicense.Number,
        Category = d.DriveLicense.Category
    })
    .ToListAsync();
```

*SQL-представление:*
```sql
CREATE VIEW "vw_DriverLicenses" AS
SELECT u."Id", u."FullName", u."Login", d."Number" AS "LicenseNumber", d."Category"
FROM "Users" u
JOIN "DriveLicenses" d ON u."DriverLicenseId" = d."Id"
WHERE u."UserType" = 'Driver';
```

**3. Представление сводной информации по путевым листам:**

*Entity Framework (Собственный DTO):*
```csharp
var waybillSummary = await context.Waybills
    .Include(w => w.Driver)
    .Include(w => w.Car)
    .Select(w => new
    {
        WaybillId = w.Id,
        w.AtCreated,
        w.WaybillStatus,
        DriverName = w.Driver.FullName,
        CarModel = w.Car.Model,
        w.Car.CarNumber
    })
    .ToListAsync();
```

*SQL-представление:*
```sql
CREATE VIEW "vw_WaybillSummary" AS
SELECT w."Id" AS "WaybillId", w."AtCreated", w."WaybillStatus",
       u."FullName" AS "DriverName", c."Model" AS "CarModel", c."CarNumber"
FROM "Waybills" w
JOIN "Users" u ON w."DriverId" = u."Id"
JOIN "Cars" c ON w."CarId" = c."Id";
```

**4. Представление для подсчета фактически пройденного расстояния по путевому листу:**

*Entity Framework (Собственный DTO, Distance задана в Entity):*
```csharp
var distanceInfo = await context.WaybillDetails
    .Select(wd => new
    {
        wd.WaybillId,
        wd.DepartureDateTime,
        wd.ArrivalDateTime,
        Distance = wd.Distance,
        wd.FuelConsumed
    })
    .ToListAsync();
```

*SQL-представление:*
```sql
CREATE VIEW "vw_WaybillDistance" AS
SELECT "WaybillId", "DepartureDateTime", "ArrivalDateTime",
       ("EndMealing" - "StartMealing") AS "Distance",
       "FuelConsumed"
FROM "WaybillDetails";
```

**5. Представление списка заданий вместе с информацией о водителе:**

*Entity Framework (Собственный DTO):*
```csharp
var tasksWithDrivers = await context.WaybillTasks
    .Include(wt => wt.Waybill)
    .ThenInclude(w => w.Driver)
    .Select(wt => new
    {
        TaskId = wt.Id,
        wt.DeparturePoint,
        wt.ArrivalPoint,
        wt.Mileage,
        wt.WaybillId,
        DriverName = wt.Waybill.Driver.FullName
    })
    .ToListAsync();
```

*SQL-представление:*
```sql
CREATE VIEW "vw_TasksWithDrivers" AS
SELECT wt."Id" AS "TaskId", wt."DeparturePoint", wt."ArrivalPoint", wt."Mileage",
       w."Id" AS "WaybillId", u."FullName" AS "DriverName"
FROM "WaybillTasks" wt
JOIN "Waybills" w ON wt."WaybillId" = w."Id"
JOIN "Users" u ON w."DriverId" = u."Id";
```

---

# 3.1.3 SQL-определения хранимых процедур (не менее 2)

Хранимые процедуры в Entity Framework заменены бизнес-логикой и вызовами `SaveChanges()`. 

**1. Процедура для обновления статуса путевого листа:**

*Entity Framework (Бизнес-логика):*
```csharp
public async Task UpdateWaybillStatusAsync(int waybillId, WaybillStatus newStatus, string reason = null)
{
    var waybill = await context.Waybills.FindAsync(waybillId);
    if (waybill != null)
    {
        waybill.WaybillStatus = newStatus;
        waybill.ReasonOfDecline = reason;
        await context.SaveChangesAsync();
    }
}
```

*SQL (Хранимая процедура):*
```sql
CREATE PROCEDURE "UpdateWaybillStatus"(
    p_WaybillId INT,
    p_NewStatus INT,
    p_Reason TEXT DEFAULT NULL
)

LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE "Waybills"
    SET "WaybillStatus" = p_NewStatus,
        "ReasonOfDecline" = p_Reason
    WHERE "Id" = p_WaybillId;
END;
$$;
```

**2. Процедура для добавления нового автомобиля:**

*Entity Framework (Добавление сущности):*
```csharp
public async Task AddNewCarAsync(string model, string carNumber, float fuelRate, int fuelTypeId)
{
    var car = new Car
    {
        Model = model,
        CarNumber = carNumber,
        FuelRate = fuelRate,
        FuelTypeId = fuelTypeId
    };
    context.Cars.Add(car);
    await context.SaveChangesAsync();
}
```

*SQL (Хранимая процедура):*
```sql
CREATE PROCEDURE "AddNewCar"(
    p_Model TEXT,
    p_CarNumber TEXT,
    p_FuelRate REAL,
    p_FuelTypeId INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO "Cars" ("Model", "CarNumber", "FuelRate", "FuelTypeId")
    VALUES (p_Model, p_CarNumber, p_FuelRate, p_FuelTypeId);
END;
$$;
```

---

# 3.1.4 SQL-определения триггеров (не менее 2)

Вместо триггеров на БД Entity Framework позволяет ограничивать операции настройкой связей (`SetNull`/`Restrict`), либо переопределять метод `SaveChanges()` для заполнения данных перед сохранением.

**1. Триггер для запрета удаления автомобиля, если он привязан к путевому листу:**

*Entity Framework (Проверка бизнес-логики в репозитории):*
```csharp
public async Task DeleteCarSafeAsync(int carId)
{
    bool hasWaybills = await context.Waybills.AnyAsync(w => w.CarId == carId);
    if (hasWaybills)
    {
        throw new InvalidOperationException("Невозможно удалить автомобиль: он используется в путевых листах.");
    }
    
    var car = await context.Cars.FindAsync(carId);
    if (car != null)
    {
        context.Cars.Remove(car);
        await context.SaveChangesAsync();
    }
}
```

*SQL-триггер:*
```sql
CREATE OR REPLACE FUNCTION "PreventCarDeletion"()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM "Waybills" WHERE "CarId" = OLD."Id") THEN
        RAISE EXCEPTION 'Невозможно удалить автомобиль: он используется в путевых листах.';
    END IF;
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER "trg_PreventCarDeletion"
BEFORE DELETE ON "Cars"
FOR EACH ROW
EXECUTE FUNCTION "PreventCarDeletion"();
```

**2. Триггер для автоматического заполнения даты создания деталей путевого листа:**

*Entity Framework (задано через конструктор/инициализатор в сущности):*
```csharp
// В классе WaybillDetails задано инициализатором свойства:
public class WaybillDetails : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    // ...
}

// Альтернатива через перехват SaveChanges в контексте:
public override int SaveChanges()
{
    var entries = ChangeTracker.Entries<WaybillDetails>()
        .Where(e => e.State == EntityState.Added);

    foreach (var entry in entries)
    {
        entry.Entity.CreatedAt = DateTime.Now;
    }
    return base.SaveChanges();
}
```

*SQL-триггер:*
```sql
CREATE OR REPLACE FUNCTION "SetWaybillDetailsCreatedAt"()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW."CreatedAt" IS NULL THEN
        NEW."CreatedAt" := CURRENT_TIMESTAMP;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER "trg_SetWaybillDetailsCreatedAt"
BEFORE INSERT ON "WaybillDetails"
FOR EACH ROW
EXECUTE FUNCTION "SetWaybillDetailsCreatedAt"();
```
