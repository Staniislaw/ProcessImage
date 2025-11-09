using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

using System.Runtime.Serialization;

namespace ProcessImage.Helpers
{
    public static class LoggerAppender
    {
        private static ILog GetLogger(string loggerName, string fileName)
        {
            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%date %-5level %message%newline"
            };
            patternLayout.ActivateOptions();

            var rollingFileAppender = new RollingFileAppender
            {
                AppendToFile = true,
                Name = loggerName,
                RollingStyle = RollingFileAppender.RollingMode.Date,
                File = $"logs\\{fileName}.log",
                Layout = patternLayout
            };
            rollingFileAppender.ActivateOptions();

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.AddAppender(rollingFileAppender);
            hierarchy.Root.Level = log4net.Core.Level.All;
            hierarchy.Configured = true;

            var log = LogManager.GetLogger(loggerName);
            var logger = (log4net.Repository.Hierarchy.Logger)log.Logger;
            logger.Additivity = false;
            logger.AddAppender(rollingFileAppender);

            return log;
        }

        public enum LogApplication
        {
            [EnumMember(Value = "ProcessImage")]
            ProcessImage = 1,
        }

        public static void LogInformation(LogApplication applicationType, string companyName, int companyId, string loggerType, string message)
        {
            var logger = GetLoggerForApplicationType(applicationType, companyName, companyId);

            switch (loggerType.ToLower())
            {
                case "info":
                    logger.Info(message);
                    break;
                case "warn":
                    logger.Warn(message);
                    break;
                case "error":
                    logger.Error(message);
                    break;
                default:
                    logger.Debug(message);
                    break;
            }
        }

        public static ILog GetLoggerForApplicationType(LogApplication logApplication, string userName, int userId)
        {
            ILog logger;
            switch (logApplication)
            {
                case LogApplication.ProcessImage:
                    logger = GetLogger($"{userName}_{userId}_ProcessImage", $"{userName}_{userId}_ProcessImage");
                    break;
                default:
                    logger = GetLogger($"{userName}_{userId}_Appender", $"{userName}_{userId}");
                    break;
            }
            return logger;
        }
    }
}
