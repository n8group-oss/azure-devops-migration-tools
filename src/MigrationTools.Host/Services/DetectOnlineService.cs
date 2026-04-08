using System;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using MigrationTools.Services;
using Serilog;

namespace MigrationTools.Host.Services
{
    public class DetectOnlineService : IDetectOnlineService
    {
        private readonly ITelemetryLogger _Telemetry;
        private ILogger<DetectOnlineService> _logger;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

        public DetectOnlineService(ITelemetryLogger telemetry, ILogger<DetectOnlineService> logger)
        {
            _Telemetry = telemetry;
            _logger = logger;
        }

        public bool IsOnline()
        {
            _logger.LogDebug("DetectOnlineService::IsOnline");
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("DetectOnlineService:IsOnline", ActivityKind.Client))
            {
                activity?.SetTag("url.full", "https://dns.google");
                activity?.SetTag("server.address", "dns.google");
                activity?.SetTag("http.request.method", "HEAD");

                DateTime startTime = DateTime.Now;
                Stopwatch mainTimer = Stopwatch.StartNew();
                //////////////////////////////////
                bool isOnline = false;
                string responce = "none";
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Head, "https://dns.google");
                    var response = _httpClient.Send(request);
                    responce = ((int)response.StatusCode).ToString();
                    isOnline = response.IsSuccessStatusCode;
                    mainTimer.Stop();
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    activity?.SetTag("http.response.status_code", responce);
                }
                catch (Exception ex)
                {
                    mainTimer.Stop();
                    // Likely no network is even available
                    Log.Warning("Unable to verify network connectivity: {Message}", ex.Message);
                    responce = "error";
                    isOnline = false;
                    activity?.SetStatus(ActivityStatusCode.Error);
                    activity?.SetTag("http.response.status_code", "500");
                }
                /////////////////
                mainTimer.Stop();
                return isOnline;
            }
        }
    }
}
