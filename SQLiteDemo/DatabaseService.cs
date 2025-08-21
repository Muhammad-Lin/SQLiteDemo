
using Dapper;
using NLog;
using SQLiteDemo.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace SQLiteDemo
{
    public class DatabaseService
    {
        public readonly string _mainDbConnectionString;
        public readonly string _tempDbConnectionString;
        private readonly EncryptionService _encryption;
        private readonly ILogger _logger;

        public DatabaseService(EncryptionService encryption, ILogger logger)
        {
            _mainDbConnectionString = ConfigHelper.GetMainDbConnectionString();
            _tempDbConnectionString = ConfigHelper.GetTempDbConnectionString();
            _encryption = encryption;
            _logger = logger;
            InitializeSQLite();
        }


        private void InitializeSQLite()
        {
            using (var conn = new SQLiteConnection(_tempDbConnectionString))
            {
                conn.Execute(@"
                CREATE TABLE IF NOT EXISTS TempData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EncryptedData TEXT NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IsSynced BOOLEAN DEFAULT 0
                )");
            }
        }

        public bool IsMainDatabaseAvailable()
        {
            try
            {
                using (var conn = new SqlConnection(_mainDbConnectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "主数据库连接失败");
                return false;
            }
        }

        public IEnumerable<DataModel> GetRecentDataFromMain()
        {
            using (var conn = new SqlConnection(_mainDbConnectionString))
            {
                return conn.Query<DataModel>(
                    "SELECT Id, EncryptedData, CreatedAt FROM [SelectTable] WHERE CreatedAt >= @OneMonthAgo",
                    new { OneMonthAgo = DateTime.UtcNow.AddMonths(-1) });
            }
        }

        public void SaveToTempStorage(string data)
        {
            var encryptedData = _encryption.Encrypt(data);
            using (var conn = new SQLiteConnection(_tempDbConnectionString))
            {
                conn.Execute(
                    "INSERT INTO TempData (EncryptedData) VALUES (@EncryptedData)",
                    new { EncryptedData = encryptedData });
            }
        }

        public IEnumerable<DataModel> GetUnsyncedDataFromTemp()
        {
            using (var conn = new SQLiteConnection(_tempDbConnectionString))
            {
                return conn.Query<DataModel>(
                    "SELECT Id, EncryptedData, CreatedAt FROM TempData WHERE IsSynced = 0");
            }
        }

        public void SyncToMainDatabase(IEnumerable<DataModel> data)
        {
            using (var conn = new SqlConnection(_mainDbConnectionString))
            {
                conn.Open();
                foreach (var item in data)
                {
                    try
                    {
                        var decryptedData = _encryption.Decrypt(item.EncryptedData);
                        conn.Execute(
                            "INSERT INTO [SelectTable] (Data, CreatedAt) VALUES (@Data, @CreatedAt)",
                            new { Data = decryptedData, CreatedAt = item.CreatedAt });

                        using (var tempConn = new SQLiteConnection(_tempDbConnectionString))
                        {
                            tempConn.Execute(
                                "UPDATE TempData SET IsSynced = 1 WHERE Id = @Id",
                                new { item.Id });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"同步失败: ID={item.Id}");
                    }
                }
            }
        }
    }
}
