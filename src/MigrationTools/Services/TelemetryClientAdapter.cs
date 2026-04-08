using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Elmah.Io.Client;
using Microsoft.Extensions.Configuration;
using MigrationTools.Services;

namespace MigrationTools
{
    public class TelemetryClientAdapter : ITelemetryLogger
    {
        private static IElmahioAPI elmahIoClient;
        private static IMigrationToolVersion _MigrationToolVersion;
        private readonly string _elmahLogId;
        private readonly bool _elmahEnabled;

        public TelemetryClientAdapter(IMigrationToolVersion migrationToolVersion, IConfiguration configuration)
        {
            _MigrationToolVersion = migrationToolVersion;

            var elmahApiKey = configuration.GetValue<string>("Telemetry:Elmah:ApiKey") ?? "";
            _elmahLogId = configuration.GetValue<string>("Telemetry:Elmah:LogId") ?? "";
            _elmahEnabled = !string.IsNullOrWhiteSpace(elmahApiKey) && !string.IsNullOrWhiteSpace(_elmahLogId);

            if (_elmahEnabled)
            {
                elmahIoClient = ElmahioAPI.Create(elmahApiKey, new ElmahIoOptions
                {
                    Timeout = TimeSpan.FromSeconds(30),
                    UserAgent = "Migration-Tools",
                });
                elmahIoClient.Messages.OnMessage += (sender, args) => args.Message.Version = migrationToolVersion.GetRunningVersion().versionString;
            }
        }

        private static string _sessionid = Guid.NewGuid().ToString();

        public string SessionId => _sessionid;

        public void TrackException(Exception ex, IDictionary<string, string> properties)
        {
            if (!_elmahEnabled)
            {
                Console.WriteLine($"Error occurred but telemetry is not configured. Configure Telemetry:Elmah:ApiKey and Telemetry:Elmah:LogId in appsettings.json to enable error reporting.");
                Console.WriteLine($"!! Check for latest version - We fix issues constantly - If not, please create a discussion on https://github.com/n8group-oss/azure-devops-migration-tools/discussions so we can get this fixed !!");
                return;
            }

            var baseException = ex.GetBaseException();
            var createMessage = new CreateMessage
            {
                DateTime = DateTime.UtcNow,
                Detail = ex.ToString(),
                Type = baseException.GetType().FullName,
                Title = baseException.Message ?? "An error occurred",
                Severity = "Error",
                Data = new List<Item>(),
                Source = baseException.Source,
                User = Environment.UserName,
                Hostname = System.Environment.GetEnvironmentVariable("COMPUTERNAME"),
                Application = "Migration-Tools",
                ServerVariables = new List<Item>
                    {
                        new Item("User-Agent", $"X-ELMAHIO-APPLICATION; OS={Environment.OSVersion.Platform}; OSVERSION={Environment.OSVersion.Version}; ENGINEVERSION={_MigrationToolVersion.GetRunningVersion().versionString}; ENGINE=Migration-Tools"),
                    }
            };
            createMessage.Data.Add(new Item("SessionId", SessionId));
            createMessage.Data.Add(new Item("Version", _MigrationToolVersion.GetRunningVersion().versionString));

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    createMessage.Data.Add(new Item(property.Key, property.Value));
                }

            }
           var result = elmahIoClient.Messages.CreateAndNotify(new Guid(_elmahLogId), createMessage);
           Console.WriteLine($"Error logged to Elmah.io! ");
           Console.WriteLine($"!! Check for latest version - We fix issues constantly - If not, please create a discussion on https://github.com/n8group-oss/azure-devops-migration-tools/discussions so we can get this fixed !!");
        }

        public void TrackException(Exception ex, IEnumerable<KeyValuePair<string, string>> properties = null)
        {
            if (properties == null)
            {
                TrackException(ex, null);
                return;
            }
            TrackException(ex, properties.ToDictionary(k => k.Key, v => v.Value));
        }

    }
}