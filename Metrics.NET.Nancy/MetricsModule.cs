using System;
using System.IO;
using System.Linq;
using Metrics.Endpoints;
using Metrics.Reports;

namespace Nancy.Metrics
{
    public class MetricsModule : NancyModule
    {
        private struct ModuleConfig
        {
            public readonly string ModulePath;
            public readonly Action<INancyModule> ModuleConfigAction;
            public readonly NancyMetricsEndpointHandler Handler;

            public ModuleConfig(Action<INancyModule> moduleConfig, NancyMetricsEndpointHandler handler, string metricsPath)
            {
                ModuleConfigAction = moduleConfig;
                ModulePath = metricsPath;
                Handler = handler;
            }
        }

        private static ModuleConfig _config;

        private static readonly object[] NoCacheHeaders = {
            new { Header = "Cache-Control", Value = "no-cache, no-store, must-revalidate" },
            new { Header = "Pragma", Value = "no-cache" },
            new { Header = "Expires", Value = "0" }
        };

        internal static void Configure(Action<INancyModule> moduleConfig, MetricsEndpointReports reports, string metricsPath)
        {
            var handler = new NancyMetricsEndpointHandler(reports.Endpoints);
            _config = new ModuleConfig(moduleConfig, handler, metricsPath);
        }

        public MetricsModule()
            : base(_config.ModulePath ?? "/")
        {
            if (string.IsNullOrEmpty(_config.ModulePath))
            {
                return;
            }

            _config.ModuleConfigAction?.Invoke(this);

            Get["/"] = _ =>
            {
                if (!Request.Url.Path.EndsWith("/"))
                {
                    return Response.AsRedirect(Request.Url.ToString() + "/");
                }
                var gzip = AcceptsGzip();
                var response = Response.FromStream(FlotWebApp.GetAppStream(!gzip), "text/html");
                if (gzip)
                {
                    response.WithHeader("Content-Encoding", "gzip");
                }
                return response;
            };

            Get["/{path*}"] = p =>
            {
                var path = (string)p.path;
                var endpointResponse = _config.Handler.Process(path, Request);
                return endpointResponse != null ? GetResponse(endpointResponse) : HttpStatusCode.NotFound;
            };
        }

        private static Response GetResponse(MetricsEndpointResponse endpointResponse)
        {
            var response = new Response
            {
                StatusCode = (HttpStatusCode) endpointResponse.StatusCode,
                ContentType = endpointResponse.ContentType,
                Contents = stream =>
                {
                    using (var writer = new StreamWriter(stream, endpointResponse.Encoding))
                    {
                        writer.Write(endpointResponse.Content);
                    }
                }
            };

            return response.WithHeaders(NoCacheHeaders);
        }

        private bool AcceptsGzip()
        {
            return Request.Headers.AcceptEncoding.Any(e => e.Equals("gzip", StringComparison.OrdinalIgnoreCase));
        }
    }
}
