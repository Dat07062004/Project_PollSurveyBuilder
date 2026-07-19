using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

// Allow self-signed dev SSL certificates when proxying in development
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};
builder.Services.AddSingleton(new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) });

var app = builder.Build();

app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseDefaultFiles();
app.UseStaticFiles();

// Hop-by-hop headers that should not be forwarded in HTTP/2 & HTTP/3
var hopByHopHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization",
    "TE", "Trailers", "Transfer-Encoding", "Upgrade", "Proxy-Connection"
};

// Transparent API Proxy middleware for standalone Web UI project
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path != null && (path.StartsWith("/api") || path.StartsWith("/hubs")))
    {
        var client = context.RequestServices.GetRequiredService<HttpClient>();

        // Try HTTPS port 7076 first, then HTTP port 5000 fallback
        string[] targetBases = new[] { "https://localhost:7076", "http://localhost:5000" };

        foreach (var targetBase in targetBases)
        {
            var targetUri = new Uri($"{targetBase}{context.Request.Path}{context.Request.QueryString}");
            try
            {
                var reqMsg = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);
                if (context.Request.ContentLength > 0 || context.Request.ContentType != null)
                {
                    reqMsg.Content = new StreamContent(context.Request.Body);
                    if (context.Request.ContentType != null)
                        reqMsg.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType);
                }

                foreach (var header in context.Request.Headers)
                {
                    if (!header.Key.StartsWith("Host", StringComparison.OrdinalIgnoreCase) && !hopByHopHeaders.Contains(header.Key))
                    {
                        reqMsg.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }

                var respMsg = await client.SendAsync(reqMsg, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
                context.Response.StatusCode = (int)respMsg.StatusCode;

                foreach (var header in respMsg.Headers)
                {
                    if (!hopByHopHeaders.Contains(header.Key))
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }
                }
                foreach (var header in respMsg.Content.Headers)
                {
                    if (!hopByHopHeaders.Contains(header.Key))
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }
                }

                await respMsg.Content.CopyToAsync(context.Response.Body);
                return;
            }
            catch
            {
                // Try next target base
            }
        }
    }

    await next();
});

app.Run();
