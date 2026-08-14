using Jellyfin.Plugin.FileTransformation.Helpers;
using Jellyfin.Plugin.FileTransformation.Library;
using Jellyfin.Plugin.FileTransformation.Models;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.FileTransformation
{
    public static class PluginInterface
    {
        public static void RegisterTransformation(JObject payload)
        {
            IWebFileTransformationWriteService? writeService = PluginServiceRegistrator.WriteService;
            ILogger? logger = PluginServiceRegistrator.TransformationLogger;
            IServerApplicationHost? serverApplicationHost = PluginServiceRegistrator.ApplicationHost;

            if (writeService == null || logger == null || serverApplicationHost == null)
            {
                throw new InvalidOperationException(
                    "File Transformation has not completed its Jellyfin 12 service initialization.");
            }

            TransformationRegistrationPayload? castedPayload = payload.ToObject<TransformationRegistrationPayload>();

            if (castedPayload != null)
            {
                writeService.AddTransformation(castedPayload.Id, castedPayload.FileNamePattern, async (path, contents) =>
                {
                    await TransformationHelper.ApplyTransformation(path, contents, castedPayload, logger, serverApplicationHost);
                });
            }
        }
    }
}
