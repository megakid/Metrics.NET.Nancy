
using System;

namespace Metrics.NET.Nancy.Sample.Common
{
    public class UserValueHistogramSample
    {
        private readonly Histogram _histogram =
            Metric.Histogram("Results", Unit.Items);

        public void Process(string documentId)
        {
            var results = GetResultsForDocument(documentId);
            _histogram.Update(results.Length, documentId);
        }

        private int[] GetResultsForDocument(string documentId)
        {
            return new int[new Random().Next() % 100];
        }

        public static void RunSomeRequests()
        {
            for (int i = 0; i < 30; i++)
            {
                var documentId = new Random().Next() % 10;
                new UserValueHistogramSample().Process("document-" + documentId.ToString());
            }
        }
    }
}
