using NLog;
using System;

namespace SQLiteDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. 设置日志配置
            LogManager.Setup()
                .LoadConfigurationFromFile("NLog.config");

            // 2. 获取日志记录器

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info("程序启动");

            try
            {
                // 从 ConfigHelper 获取加密密钥
                string encryptionKey = ConfigHelper.GetEncryptionKey();

                // 初始化服务
                var encryptionService = new EncryptionService(encryptionKey);
                var dbService = new DatabaseService(encryptionService, logger);
                var syncManager = new SyncManager(dbService, logger);

                // 执行同步
                syncManager.ExecuteSync();

                // 模拟业务逻辑
                if (dbService.IsMainDatabaseAvailable())
                {
                    var recentData = dbService.GetRecentDataFromMain();
                    // 处理数据...
                }
                else
                {
                    dbService.SaveToTempStorage("Sample Data");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "程序崩溃");
            }
        }
    }
}
