using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Metrics;
using Metrics.NET.Nancy.Tests.Utils;
using Nancy;
using Nancy.Testing;
using Xunit;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace Metrics.Tests.NancyAdapter
{
    public class NancyAdapterGlobalMetrics
    {
        public class ActiveRequestsModule : NancyModule
        {
            public ActiveRequestsModule(Task trigger, TaskCompletionSource<int> request1, TaskCompletionSource<int> request2)
                : base("/concurrent")
            {
                Get["/request1"] = _ => { request1.SetResult(0); Task.WaitAll(trigger); return HttpStatusCode.OK; };
                Get["/request2"] = _ => { request2.SetResult(0); Task.WaitAll(trigger); return HttpStatusCode.OK; };
            }
        }

        public class TestModule : NancyModule
        {
            public TestModule(TestClock clock)
                : base("/test")
            {
                Get["/action"] = _ =>
                {
                    clock.Advance(TimeUnit.Milliseconds, 100);
                    return Response.AsText("response");
                };

                Post["/post"] = _ =>
                {
                    clock.Advance(TimeUnit.Milliseconds, 200);
                    return HttpStatusCode.OK;
                };

                Put["/put"] = _ =>
                {
                    clock.Advance(TimeUnit.Milliseconds, 200);
                    return HttpStatusCode.OK;
                };

                Patch["/patch"] = _ =>
                {
                    clock.Advance(TimeUnit.Milliseconds, 200);
                    return HttpStatusCode.OK;
                };

                Get["/error"] = _ => { throw new InvalidOperationException(); };
            }
        }

        private readonly TestContext _context = new TestContext();
        private readonly MetricsConfig _config;

        private readonly Browser _browser;

        private readonly TaskCompletionSource<int> _requestTrigger = new TaskCompletionSource<int>();
        private readonly TaskCompletionSource<int> _result1 = new TaskCompletionSource<int>();
        private readonly TaskCompletionSource<int> _result2 = new TaskCompletionSource<int>();

        public NancyAdapterGlobalMetrics()
        {
            _config = new MetricsConfig(_context);
            _browser = new Browser(with =>
            {
                with.ApplicationStartup((c, p) =>
                {
                    _config.WithNancy(p);
                });

                with.Module(new TestModule(_context.Clock));
                with.Module(new ActiveRequestsModule(_requestTrigger.Task, _result1, _result2));
            });
        }

        [Fact]
        public void NancyMetrics_ShouldBeAbleToRecordTimeForAllRequests()
        {
            _context.TimerValue("NancyFx", "Requests").Rate.Count.Should().Be(0);

            _browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

            var timer = _context.TimerValue("NancyFx", "Requests");

            timer.Rate.Count.Should().Be(1);
            timer.Histogram.Count.Should().Be(1);

            timer.Histogram.Max.Should().Be(100);
            timer.Histogram.Min.Should().Be(100);

            _browser.Post("/test/post").StatusCode.Should().Be(HttpStatusCode.OK);

            timer = _context.TimerValue("NancyFx", "Requests");

            timer.Rate.Count.Should().Be(2);
            timer.Histogram.Count.Should().Be(2);

            timer.Histogram.Max.Should().Be(200);
            timer.Histogram.Min.Should().Be(100);
        }

        [Fact]
        public void NancyMetrics_ShouldBeAbleToCountErrors()
        {
            _context.MeterValue("NancyFx", "Errors").Count.Should().Be(0);
            Assert.Throws<Exception>(() => _browser.Get("/test/error"));
            _context.MeterValue("NancyFx", "Errors").Count.Should().Be(1);
            Assert.Throws<Exception>(() => _browser.Get("/test/error"));
            _context.MeterValue("NancyFx", "Errors").Count.Should().Be(2);
        }

        [Fact]
        public void NancyMetrics_ShouldBeAbleToCountActiveRequests()
        {
            _context.CounterValue("NancyFx", "Active Requests").Count.Should().Be(0);
            var request1 = Task.Factory.StartNew(() => _browser.Get("/concurrent/request1"));

            _result1.Task.Wait();
            _context.CounterValue("NancyFx", "Active Requests").Count.Should().Be(1);

            var request2 = Task.Factory.StartNew(() => _browser.Get("/concurrent/request2"));
            _result2.Task.Wait();
            _context.CounterValue("NancyFx", "Active Requests").Count.Should().Be(2);

            _requestTrigger.SetResult(0);
            Task.WaitAll(request1, request2);
            _context.CounterValue("NancyFx", "Active Requests").Count.Should().Be(0);
        }        

        [Fact]
        public void NancyMetrics_ShoulBeAbleToRecordPostPutAndPatchRequestSize()
        {
            _context.HistogramValue("NancyFx", "Post, Put & Patch Request Size").Count.Should().Be(0);

            _browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

            _context.HistogramValue("NancyFx", "Post, Put & Patch Request Size").Count.Should().Be(0);

            _browser.Post("/test/post", ctx =>
            {
                ctx.Header("Content-Length", "content".Length.ToString());
                ctx.Body("content");
            }).StatusCode.Should().Be(HttpStatusCode.OK);

            _browser.Put("/test/put", ctx =>
            {
                ctx.Header("Content-Length", "content".Length.ToString());
                ctx.Body("content");
            }).StatusCode.Should().Be(HttpStatusCode.OK);

            _browser.Patch("/test/patch", ctx =>
            {
                ctx.Header("Content-Length", "content".Length.ToString());
                ctx.Body("content");
            }).StatusCode.Should().Be(HttpStatusCode.OK);

            _context.HistogramValue("NancyFx", "Post, Put & Patch Request Size").Count.Should().Be(3);
            _context.HistogramValue("NancyFx", "Post, Put & Patch Request Size").Min.Should().Be("content".Length);
            _context.HistogramValue("NancyFx", "Post, Put & Patch Request Size").Max.Should().Be("content".Length);
        }

        [Fact]
        public void NancyMetrics_ShouldBeAbleToRecordTimeForEachRequests()
        {
            _context.TimerValue("NancyFx", "Requests").Rate.Count.Should().Be(0);

            _browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

            var timer = _context.TimerValue("NancyFx", "GET /test/action");

            timer.Rate.Count.Should().Be(1);
            timer.Histogram.Count.Should().Be(1);

            _browser.Post("/test/post").StatusCode.Should().Be(HttpStatusCode.OK);

            timer = _context.TimerValue("NancyFx", "POST /test/post");

            timer.Rate.Count.Should().Be(1);
            timer.Histogram.Count.Should().Be(1);
        }
    }
}
