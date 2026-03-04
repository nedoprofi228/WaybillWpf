using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

class CarRepositorySql(string connectionString, string tableName) : IBaseRepository<Car>, ICarsRepository
{
    public async Task<bool> AddAsync(Car entity)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            string query = $"""
                INSERT INTO {tableName} ("Model", "CarNumber", "FuelRate") VALUES (@Model, @CarNumber, @FuelRate)
            """;

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.AddWithValue("@Model", entity.Model);
                command.Parameters.AddWithValue("@CarNumber", entity.CarNumber);
                command.Parameters.AddWithValue("@FuelRate", entity.FuelRate);
                await command.ExecuteNonQueryAsync();
            }
            System.Console.WriteLine("машина добавлена");
            return await Task.FromResult(true);
        }
        catch(Exception e){
            System.Console.WriteLine(e.Message);
            return await Task.FromResult(false);
        }
    }

    public async Task<bool> DeleteAsync(Car entity)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            string query = $"""
                    DELETE FROM {tableName} WHERE "Id" = @Id
                """;

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.AddWithValue("@Id", entity.Id);
                command.ExecuteNonQuery();
            }

            return await Task.FromResult(true);
        }
        catch(Exception e){
            System.Console.WriteLine(e.Message);
            return await Task.FromResult(false);
        }
    }

    public async Task<ICollection<Car>> GetAllAsync()
    {
        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = $"""
                SELECT * FROM {tableName}
            """;

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    var reader = await command.ExecuteReaderAsync();
                    var cars = new List<Car>();
                    while (await reader.ReadAsync())
                    {
                        cars.Add(new Car
                        {
                            Id = (int)reader["Id"],
                            Model = (string)reader["Model"],
                            CarNumber = (string)reader["CarNumber"],
                            FuelRate = (float)reader["FuelRate"]
                        });
                    }
                    return await Task.FromResult(cars);
                }
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
            return await Task.FromResult<ICollection<Car>>([]);
        }
        
    }

    public async Task<Car?> GetByIdAsync(int id)
    {
        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = $"""
                SELECT * FROM {tableName} WHERE "Id" = @Id
            """;

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@Id", id);
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        return new Car
                        {
                            Id = (int)reader["Id"],
                            Model = (string)reader["Model"],
                            CarNumber = (string)reader["CarNumber"],
                            FuelRate = (float)reader["FuelRate"]
                        };
                    }
                    return await Task.FromResult<Car?>(null);
                }
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
            return await Task.FromResult<Car?>(null);
        }
    }

    public async Task<bool> UpdateAsync(Car entity)
    {
        try{
            using(var connetion = new NpgsqlConnection(connectionString)){
                await connetion.OpenAsync();

                string querry = $"""
                UPDATE {tableName} SET "Model" = @Model, "CarNumber" = @CarNumber, "FuelRate" = @FuelRate WHERE "Id" = @Id
                """;

                using(var command = connetion.CreateCommand()){
                    command.CommandText = querry;
                    command.Parameters.AddWithValue("@Model", entity.Model);
                    command.Parameters.AddWithValue("@CarNumber", entity.CarNumber);
                    command.Parameters.AddWithValue("@FuelRate", entity.FuelRate);
                    command.Parameters.AddWithValue("@Id", entity.Id);
                    command.ExecuteNonQuery();
                }
                return await Task.FromResult(true);
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
            return await Task.FromResult(false);
        }
    }
}