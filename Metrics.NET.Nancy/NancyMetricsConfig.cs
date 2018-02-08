using System;
using Metrics;
using Metrics.Reports;
using Nancy.Bootstrapper;

namespace Nancy.Metrics
{
    public class NancyMetricsConfig
    {
        public static readonly NancyMetricsConfig Disabled = new NancyMetricsConfig();

        private readonly MetricsContext _metricsContext;
        private readonly Func<HealthStatus> _healthStatus;
        private readonly IPipelines _nancyPipelines;

        private readonly bool _isDiabled;

        public NancyMetricsConfig(MetricsContext metricsContext, Func<HealthStatus> healthStatus, IPipelines nancyPipelines)
        {
            _metricsContext = metricsContext;
            _healthStatus = healthStatus;
            _nancyPipelines = nancyPipelines;
        }

        private NancyMetricsConfig()
        {
            _isDiabled = true;
        }

        /// <summary>
        /// Configure global NancyFx Metrics.
        /// Available global metrics are: Request Timer, Active Requests Counter, Error Meter
        /// <code>
        /// protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        /// {
        ///     base.ApplicationStartup(container, pipelines);
        /// 
        ///     NancyMetrics.Configure()
        ///         .WithGlobalMetrics(config => config.RegisterAllMetrics(pipelines))
        ///         .WithMetricsEndpoint();
        /// }
        /// </code>
        /// </summary>
        /// <param name="config">Action to configure which global metrics to enable.</param>
        /// <param name="context">Name of the MetricsContext where to register the NancyFx metrics.</param>
        /// <returns>This instance to allow chaining of the configuration.</returns>
        public NancyMetricsConfig WithNancyMetrics(Action<NancyGlobalMetrics> config, string context = "NancyFx")
        {
            if (_isDiabled)
            {
                return this;
            }

            var globalMetrics = new NancyGlobalMetrics(_metricsContext.Context(context), _nancyPipelines);
            config(globalMetrics);
            return this;
        }

        /// <summary>
        /// Expose the metrics information at:
        /// /metrics in human readable format
        /// /metrics/json in json format
        /// <code>
        /// protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        /// {
        ///     base.ApplicationStartup(container, pipelines);
        /// 
        ///     NancyMetrics.Configure()
        ///         .WithGlobalMetrics(config => config.RegisterAllMetrics(pipelines))
        ///         .WithMetricsEndpoint(m => m.RequiresAuthentication()); // to enable authentication
        /// }
        /// </code>
        /// </summary>
        /// <param name="metricsPath">Path where to expose the metrics</param>
        /// <returns>This instance to allow chaining of the configuration.</returns>
        public NancyMetricsConfig WithMetricsModule(string metricsPath = "/metrics")
        {
            if (_isDiabled)
            {
                return this;
            }

            return WithMetricsModule(m => { }, c => { }, metricsPath);
        }

        /// <summary>
        /// Expose the metrics information at:
        /// /metrics in human readable format
        /// /metrics/json in json format
        /// <code>
        /// protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        /// {
        ///     base.ApplicationStartup(container, pipelines);
        /// 
        ///     NancyMetrics.Configure()
        ///         .WithGlobalMetrics(config => config.RegisterAllMetrics(pipelines))
        ///         .WithMetricsEndpoint(m => m.RequiresAuthentication()); // to enable authentication
        /// }
        /// </code>
        /// </summary>
        /// <param name="config">Action that can configure the endpoint reports</param>
        /// <param name="metricsPath">Path where to expose the metrics</param>
        /// <returns>This instance to allow chaining of the configuration.</returns>
        public NancyMetricsConfig WithMetricsModule(Action<MetricsEndpointReports> config, string metricsPath = "/metrics")
        {
            if (_isDiabled)
            {
                return this;
            }

            return WithMetricsModule(m => { }, config, metricsPath);
        }

        /// <summary>
        /// Expose the metrics information at:
        /// /metrics in human readable format
        /// /metrics/json in json format
        /// <code>
        /// protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        /// {
        ///     base.ApplicationStartup(container, pipelines);
        /// 
        ///     NancyMetrics.Configure()
        ///         .WithGlobalMetrics(config => config.RegisterAllMetrics(pipelines))
        ///         .WithMetricsEndpoint(m => m.RequiresAuthentication()); // to enable authentication
        /// }
        /// </code>
        /// </summary>
        /// <param name="moduleConfig">Action that can configure the Metrics Module ( for example to apply authentication )</param>
        /// <param name="config">Action that can configure the endpoint reports</param>
        /// <param name="metricsPath">Path where to expose the metrics</param>
        /// <returns>This instance to allow chaining of the configuration.</returns>
        public NancyMetricsConfig WithMetricsModule(Action<INancyModule> moduleConfig, Action<MetricsEndpointReports> config, string metricsPath = "/metrics")
        {
            if (_isDiabled)
            {
                return this;
            }

            var reportsConfig = new MetricsEndpointReports(_metricsContext.DataProvider, _healthStatus);
            config(reportsConfig);
            MetricsModule.Configure(moduleConfig, reportsConfig, metricsPath);
            return this;
        }
    }
}
