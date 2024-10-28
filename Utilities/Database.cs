using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

public class DbConnection : IDisposable
{
    private readonly string _connectionString;
    private readonly string _databaseName;
    private IMongoClient _client;
    private IMongoDatabase _database;

    public DbConnection(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MongoConnection");
        _databaseName = configuration.GetSection("MongoSettings:DatabaseName").Value;
        _client = new MongoClient(_connectionString);
        _database = _client.GetDatabase(_databaseName);
    }

    public IMongoDatabase GetDatabase()
    {
        return _database;
    }

    public void Dispose()
    {
        _client = null;
        _database = null;
    }
}