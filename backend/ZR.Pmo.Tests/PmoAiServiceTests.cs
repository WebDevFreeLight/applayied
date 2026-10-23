using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ZR.Service.PMO;

namespace ZR.Service.Tests.PMO
{
    /// <summary>
    /// Fallback-chain tests for PmoAiService against a fake HTTP handler (no network): which
    /// providers are used, how failures are reported, and what the request carries. Each test uses
    /// its own host/key so the service's static cooldown map can't leak between tests.
    /// </summary>
    public class PmoAiServiceTests
    {
        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, string, HttpResponseMessage> _respond;
            public readonly List<(string Host, string Body)> Calls = new();
            public FakeHandler(Func<HttpRequestMessage, string, HttpResponseMessage> respond) => _respond = respond;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            {
                var body = request.Content == null ? "" : await request.Content.ReadAsStringAsync(ct);
                Calls.Add((request.RequestUri!.Host, body));
                return _respond(request, body);
            }
        }

        private static HttpResponseMessage Json(HttpStatusCode code, string json) =>
            new(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

        private static string Completion(string text) =>
            "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":" + System.Text.Json.JsonSerializer.Serialize(text) + "}}]}";

        private static PmoAiService Build(FakeHandler handler, params (string Name, string Key)[] providers)
            => Build(handler, new Dictionary<string, string>(), providers);

        private static PmoAiService Build(FakeHandler handler, Dictionary<string, string> extra, params (string Name, string Key)[] providers)
        {
            var settings = new Dictionary<string, string>(extra);
            for (int i = 0; i < providers.Length; i++)
            {
                settings[$"AI:Providers:{i}:Name"] = providers[i].Name;
                settings[$"AI:Providers:{i}:ApiKey"] = providers[i].Key;
                settings[$"AI:Providers:{i}:BaseUrl"] = $"https://{providers[i].Name.ToLowerInvariant()}.test/v1";
                settings[$"AI:Providers:{i}:Model"] = "m";
            }
            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            return new PmoAiService(new HttpClient(handler), config, NullLogger<PmoAiService>.Instance);
        }

        private static string Uniq(string name) => name + Guid.NewGuid().ToString("N")[..8];

        [Fact]
        public async Task Skips_placeholder_and_blank_keys_and_reports_none_configured()
        {
            var handler = new FakeHandler((_, _) => throw new InvalidOperationException("must not be called"));
            var ai = Build(handler, ("Groq", "YOUR_GROQ_API_KEY"), ("Gemini", ""));

            ai.IsConfigured.Should().BeFalse();
            ai.GetSkippedProviders().Should().HaveCount(2);

            var result = await ai.CompleteAsync("sys", "hi");

            result.Should().BeNull();
            handler.Calls.Should().BeEmpty();
            ai.LastError.Should().Contain("no AI provider is configured");
        }

        [Fact]
        public async Task Key_by_provider_name_enables_a_placeholder_entry()
        {
            var name = Uniq("Groq");
            var handler = new FakeHandler((_, _) => Json(HttpStatusCode.OK, Completion("hello")));
            var ai = Build(handler, new Dictionary<string, string> { [$"AI:Keys:{name}"] = "real-key-1" },
                (name, "YOUR_GROQ_API_KEY"));

            ai.IsConfigured.Should().BeTrue();
            (await ai.CompleteAsync("sys", "hi")).Should().Be("hello");
        }

        [Fact]
        public void Reports_key_by_name_that_matches_no_provider_or_is_empty()
        {
            var handler = new FakeHandler((_, _) => throw new InvalidOperationException("must not be called"));
            var ai = Build(handler, new Dictionary<string, string>
            {
                ["AI:Keys:Gorq"] = "real-key-typo",
                ["AI:Keys:Groq"] = " "
            }, ("Groq", "YOUR_GROQ_API_KEY"));

            ai.IsConfigured.Should().BeFalse();
            var skipped = string.Join(" | ", ai.GetSkippedProviders().Select(s => s.ToString()));
            skipped.Should().Contain("AI:Keys:Gorq").And.Contain("matches no provider name")
                .And.Contain("AI:Keys:Groq").And.Contain("is set but empty")
                .And.NotContain("real-key-typo");
        }

        [Fact]
        public async Task Falls_back_to_next_provider_and_sends_max_tokens()
        {
            string a = Uniq("A"), b = Uniq("B");
            var handler = new FakeHandler((req, _) => req.RequestUri!.Host.StartsWith(a.ToLowerInvariant())
                ? Json(HttpStatusCode.Unauthorized, "{\"error\":{\"message\":\"Invalid API Key\"}}")
                : Json(HttpStatusCode.OK, Completion("{\"name\":\"Upgrade\"}")));
            var ai = Build(handler, (a, "k-" + a), (b, "k-" + b));

            var result = await ai.AutofillAsync("project", "name", "a project");

            result.Should().Be("{\"name\":\"Upgrade\"}");
            ai.LastError.Should().BeNull();
            handler.Calls.Should().HaveCount(2);
            handler.Calls.Should().OnlyContain(c => c.Body.Contains("\"max_tokens\":"));
        }

        [Fact]
        public async Task All_failing_reports_each_provider_and_why()
        {
            string a = Uniq("A"), b = Uniq("B"), c = Uniq("C");
            var handler = new FakeHandler((req, _) =>
            {
                var host = req.RequestUri!.Host;
                if (host.StartsWith(a.ToLowerInvariant()))
                    return Json(HttpStatusCode.Forbidden, "[{\"error\":{\"code\":403,\"message\":\"Your API key was reported as leaked.\"}}]");
                if (host.StartsWith(b.ToLowerInvariant()))
                    return Json(HttpStatusCode.OK, "{\"choices\":[{\"message\":{\"content\":null}}]}");
                throw new HttpRequestException("No such host is known.");
            });
            var ai = Build(handler, (a, "k-" + a), (b, "k-" + b), (c, "k-" + c));

            var result = await ai.CompleteAsync("sys", "hi");

            result.Should().BeNull();
            ai.LastError.Should().Contain("all 3 AI provider(s) failed")
                .And.Contain($"{a}: HTTP 403 Your API key was reported as leaked.")
                .And.Contain($"{b}: response had no message content")
                .And.Contain($"{c}: HttpRequestException: No such host is known.");
            ai.LastError.Should().NotContain("k-" + a);
        }

        [Fact]
        public async Task Permanent_failure_parks_the_provider_for_later_requests()
        {
            string a = Uniq("A"), b = Uniq("B");
            var handler = new FakeHandler((req, _) => req.RequestUri!.Host.StartsWith(a.ToLowerInvariant())
                ? Json(HttpStatusCode.PaymentRequired, "{\"error\":{\"message\":\"Insufficient Balance\"}}")
                : Json(HttpStatusCode.OK, Completion("ok")));

            (await Build(handler, (a, "k-" + a), (b, "k-" + b)).CompleteAsync("s", "u")).Should().Be("ok");
            (await Build(handler, (a, "k-" + a), (b, "k-" + b)).CompleteAsync("s", "u")).Should().Be("ok");

            handler.Calls.Count(x => x.Host.StartsWith(a.ToLowerInvariant())).Should().Be(1);
        }
    }
}
