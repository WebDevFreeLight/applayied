using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;
using ZR.Service.PMO;

namespace ZR.Service.Tests.PMO
{
    /// <summary>
    /// The ML bridge forwards to the Python service. These tests use a fake HTTP handler: no
    /// Python process is needed.
    /// </summary>
    public class PmoMlServiceTests
    {
        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;
            public HttpRequestMessage Last;
            public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            {
                Last = request;
                return Task.FromResult(_respond(request));
            }
        }

        private static PmoMlService Build(FakeHandler handler, string apiKey = null)
        {
            var settings = new Dictionary<string, string> { ["ML:BaseUrl"] = "http://ml.test:8001/" };
            if (apiKey != null) settings["ML:ApiKey"] = apiKey;
            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            return new PmoMlService(new HttpClient(handler), config, NullLogger<PmoMlService>.Instance);
        }

        private static HttpResponseMessage Json(HttpStatusCode code, string json) =>
            new(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

        [Fact]
        public async Task Passes_json_through_and_builds_query()
        {
            var handler = new FakeHandler(_ => Json(HttpStatusCode.OK, "{\"available\":true,\"items\":[{\"task_id\":\"2111968329936801792\"}]}"));
            var ml = Build(handler, apiKey: "k1");

            var res = await ml.GetAsync("predict/task-delay", new Dictionary<string, string>
            {
                ["lang"] = "ar", ["project_id"] = "2001", ["empty"] = ""
            });

            res["available"]!.Value<bool>().Should().BeTrue();
            res["items"]![0]!["task_id"]!.Value<string>().Should().Be("2111968329936801792");
            handler.Last.RequestUri!.ToString().Should().Be("http://ml.test:8001/predict/task-delay?lang=ar&project_id=2001");
            handler.Last.Headers.GetValues("X-ML-Key").Should().ContainSingle("k1");
        }

        [Fact]
        public async Task Unreachable_service_returns_unavailable_instead_of_throwing()
        {
            var ml = Build(new FakeHandler(_ => throw new HttpRequestException("No such host is known.")));

            var res = await ml.GetAsync("health");

            res["available"]!.Value<bool>().Should().BeFalse();
            res["message"]!.Value<string>().Should().Contain("unreachable");
        }

        [Fact]
        public async Task Http_error_is_reported_with_status()
        {
            var ml = Build(new FakeHandler(_ => Json(HttpStatusCode.Unauthorized, "{\"detail\":\"Invalid or missing X-ML-Key\"}")));

            var res = await ml.PostAsync("train", new { problems = new[] { "task_delay" } });

            res["available"]!.Value<bool>().Should().BeFalse();
            res["message"]!.Value<string>().Should().Contain("401");
        }

        [Theory]
        [InlineData("../secrets.png")]
        [InlineData("models/x/model.joblib")]
        [InlineData("")]
        public async Task Chart_path_is_restricted(string path)
        {
            var handler = new FakeHandler(_ => throw new InvalidOperationException("must not be called"));
            (await Build(handler).GetChartAsync(path)).Should().BeNull();
        }
    }
}
