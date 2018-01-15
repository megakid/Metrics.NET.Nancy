using System.Collections.Generic;
using System.Linq;
using Metrics.Endpoints;

namespace Nancy.Metrics
{
    internal class NancyMetricsEndpointHandler : AbstractMetricsEndpointHandler<Request>
    {
        public NancyMetricsEndpointHandler(IEnumerable<MetricsEndpoint> endpoints) : base(endpoints) { }

        protected override MetricsEndpointRequest CreateRequest(Request requestInfo)
        {
            var headers = requestInfo.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray());
            return new MetricsEndpointRequest(headers);
        }
    }
}
