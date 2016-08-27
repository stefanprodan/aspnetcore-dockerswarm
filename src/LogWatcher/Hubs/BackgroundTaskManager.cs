using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogWatcher
{
    public class BackgroundTaskManager
    {
        private Task _keepAlive = null;
        private Task _changeFeed = null;
        private RethinkDbKeepAlive _keepAliveService;
        private LogChangeHandler _logChangeHandlerService;
        private readonly ILogger<BackgroundTaskManager> _logger;

        public BackgroundTaskManager(RethinkDbKeepAlive keepAliveService, LogChangeHandler logChangeHandlerService, ILogger<BackgroundTaskManager> logger)
        {
            _keepAliveService = keepAliveService;
            _logChangeHandlerService = logChangeHandlerService;
            _logger = logger;
        }

        public void StartKeepAlive()
        {
            if(_keepAlive == null || _keepAlive.IsCompleted)
            {
                _logger.LogInformation("KeepAlive Task started");

                _keepAlive = Task.Factory.StartNew(_keepAliveService.Start, TaskCreationOptions.LongRunning).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        var flattened = task.Exception.Flatten();
                        _logger.LogError(1001, flattened, $"KeepAlive error: {flattened.Message}");
                    }

                    _logger.LogCritical("KeepAlive Task exited");
                });
            }
        }

        public void StartLogChangeFeed()
        {
            if (_changeFeed == null || _changeFeed.IsCompleted)
            {
                _logger.LogInformation("LogChangeFeed Task started");

                _changeFeed = Task.Factory.StartNew(_logChangeHandlerService.HandleUpdates, TaskCreationOptions.LongRunning).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        var flattened = task.Exception.Flatten();
                        _logger.LogError(1001, flattened, $"LogChangeFeed error: {flattened.Message}");
                    }

                    _logger.LogCritical("LogChangeFeed Task exited");
                });
            }
        }
    }
}
