using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using DevSecApi.Domain.Entities;
using DevSecApi.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DevSecApi.Infrastructure;

public class PayloadRepository : IPayloadRepository
{
    private readonly string _connectionString;
    public PayloadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Строка подключения DefaultConnection не найдена.");
    }

    public Task AddAsync(Payload entity) => Task.CompletedTask;
    public Task UpdateAsync(Payload entity) => Task.CompletedTask;
    public Task DeleteAsync(Guid id) => Task.CompletedTask;
    public Task<Payload?> GetByIdAsync(Guid id) => Task.FromResult<Payload?>(null);
    public Task SaveHtmlElementAsync(long id, string attributeValue, string fullHtml) => Task.CompletedTask;

    public async Task AddRangeAsync(IEnumerable<PageElement> elements)
    {
        using IDbConnection db = new NpgsqlConnection(_connectionString);

        const string sql = "INSERT INTO elements (attribute_value, full_html) VALUES (@AttributeValue, @FullHtml);";
        
        await db.ExecuteAsync(sql, elements);
    }
}
