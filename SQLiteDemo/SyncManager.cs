using NLog;
using System.Linq;

namespace SQLiteDemo
{
    public class SyncManager
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger _logger;

        public SyncManager(DatabaseService dbService, ILogger logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        public void ExecuteSync()
        {
            if (_dbService.IsMainDatabaseAvailable())
            {
                _logger.Info("主数据库可用，开始同步...");
                var unsyncedData = _dbService.GetUnsyncedDataFromTemp();
                if (unsyncedData.Any())
                {
                    _dbService.SyncToMainDatabase(unsyncedData);
                    _logger.Info($"同步完成，共处理 {unsyncedData.Count()} 条记录");
                }
                else
                {
                    _logger.Info("无未同步数据");
                }
            }
            else
            {
                _logger.Warn("主数据库不可用，使用临时存储");
            }
        }
    }
}
