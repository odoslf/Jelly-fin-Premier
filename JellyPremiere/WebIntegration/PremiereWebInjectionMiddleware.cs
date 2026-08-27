using System.Security.Cryptography;
using System.Text;
using MediaBrowser.Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JellyPremiere.WebIntegration;

public sealed partial class PremiereWebInjectionMiddleware
{
    private const string Marker = "data-jellypremiere-client";
    private const string CommunityAssemblyName = "Jellyfin.Plugin.Community";
    private readonly RequestDelegate _next;
    private readonly IApplicationPaths _applicationPaths;
    private readonly ILogger<PremiereWebInjectionMiddleware> _logger;

    public PremiereWebInjectionMiddleware(RequestDelegate next, IApplicationPaths applicationPaths, ILogger<PremiereWebInjectionMiddleware> logger)
    {
        _next = next;
        _applicationPaths = applicationPaths;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (JellyPremierePlugin.Instance?.Configuration.EnableClientInjection == false || IsCommunityLoaded())
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var path = context.Request.Path.Value?.TrimEnd('/') ?? string.Empty;
        if ((HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
            && IsWebIndex(path)
            && await TryServeInjectedIndex(context).ConfigureAwait(false))
        {
            return;
        }

        await _next(context).ConfigureAwait(false);
    }

    private async Task<bool> TryServeInjectedIndex(HttpContext context)
    {
        var webPath = _applicationPaths.WebPath;
        if (string.IsNullOrWhiteSpace(webPath)) return false;
        var indexPath = Path.Combine(webPath, "index.html");
        if (!File.Exists(indexPath)) return false;

        try
        {
            var html = await File.ReadAllTextAsync(indexPath, Encoding.UTF8, context.RequestAborted).ConfigureAwait(false);
            if (!html.Contains(Marker, StringComparison.OrdinalIgnoreCase))
            {
                var closingBody = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                if (closingBody < 0) return false;
                html = html.Insert(closingBody, BuildScriptTag());
            }

            var payload = Encoding.UTF8.GetBytes(html);
            var digest = Convert.ToHexString(SHA256.HashData(payload));
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
            context.Response.Headers["ETag"] = $"\"jellypremiere-index-{digest[..24]}\"";
            context.Response.ContentLength = payload.LongLength;
            if (!HttpMethods.IsHead(context.Request.Method))
                await context.Response.Body.WriteAsync(payload, context.RequestAborted).ConfigureAwait(false);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !context.RequestAborted.IsCancellationRequested)
        {
            LogInjectionFailure(_logger, exception);
            return false;
        }
    }

    private static bool IsWebIndex(string path)
        => path.EndsWith("/web", StringComparison.OrdinalIgnoreCase)
           || path.EndsWith("/web/index.html", StringComparison.OrdinalIgnoreCase)
           || path.EndsWith("/web/index.htm", StringComparison.OrdinalIgnoreCase);

    private static bool IsCommunityLoaded()
        => AppDomain.CurrentDomain.GetAssemblies().Any(assembly => string.Equals(assembly.GetName().Name, CommunityAssemblyName, StringComparison.Ordinal));

    private static string BuildScriptTag()
    {
        var version = typeof(JellyPremierePlugin).Assembly.GetName().Version?.ToString() ?? "1.0.1.0";
        return $"<script {Marker}=\"{version}\" src=\"../JellyPremiere/ClientScript.js?v={version}\" defer></script>";
    }

    [LoggerMessage(EventId = 2101, Level = LogLevel.Error, Message = "JellyPremiere Web integration failed.")]
    private static partial void LogInjectionFailure(ILogger logger, Exception exception);
}
