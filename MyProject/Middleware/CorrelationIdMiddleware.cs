using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MyProject.Middleware
{
    // נותן לכל בקשה מזהה ייחודי, מכניס אותו ל-scope של הלוגים (כך שכל שורת לוג של הבקשה כוללת אותו)
    // ומחזיר אותו ללקוח ב-header. הוא גם מופיע כ-traceId בכל תשובת שגיאה
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-ID";
        private static readonly Regex ValidId = new("^[A-Za-z0-9-]{1,64}$", RegexOptions.Compiled);

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // מזהה שהלקוח שלח נשמר רק אם הוא תקין, כדי שלא ייכנס ללוג טקסט חופשי
            var incoming = context.Request.Headers[HeaderName].ToString();
            var correlationId = ValidId.IsMatch(incoming) ? incoming : Guid.NewGuid().ToString("N");

            context.TraceIdentifier = correlationId;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogInformation("HTTP {Method} {Path} started", context.Request.Method, context.Request.Path);

                await _next(context);

                _logger.LogInformation(
                    "HTTP {Method} {Path} finished with {StatusCode} in {ElapsedMs} ms",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
