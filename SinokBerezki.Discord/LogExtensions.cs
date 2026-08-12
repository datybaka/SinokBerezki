using Discord;
using Microsoft.Extensions.Logging;

namespace SinokBerezki.Discordy;

internal static class LogExtensions
{
    public static LogLevel ToLogLevel(this LogSeverity severity) => severity switch
    {
        LogSeverity.Critical => LogLevel.Critical,
        LogSeverity.Error => LogLevel.Error,
        LogSeverity.Warning => LogLevel.Warning,
        LogSeverity.Info => LogLevel.Information,
        LogSeverity.Verbose => LogLevel.Debug,
        LogSeverity.Debug => LogLevel.Trace,
        _ => LogLevel.Information
    };
}