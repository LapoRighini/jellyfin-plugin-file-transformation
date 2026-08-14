using Jellyfin.Plugin.FileTransformation.Configuration;
using Jellyfin.Plugin.FileTransformation.Library;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.FileTransformation
{
    public class FileTransformationPlugin : BasePlugin<PluginConfiguration>, IHasPluginConfiguration, IHasWebPages
    {
        public static FileTransformationPlugin Instance { get; private set; } = null!;
        
        public override Guid Id => Guid.Parse("5e87cc92-571a-4d8d-8d98-d2d4147f9f90");

        public override string Name => "File Transformation";
        
        private readonly IWebFileTransformationWriteService m_writeService;
        
        public FileTransformationPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, IServiceProvider serviceProvider, IWebFileTransformationWriteService writeService) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            
            // Jellyfin 12 no longer supplies a usable IServiceProvider to plugin
            // constructors. Prefer the long-lived service created during registration.
            m_writeService = PluginServiceRegistrator.WriteService ?? writeService;

            foreach (PluginDefinedTransformation transformation in Configuration.Transformations)
            {
                m_writeService.AddTransformation(transformation.Id, transformation.FilenamePattern, (path, contents) => HandlePluginConfigTransformation(path, contents, transformation));
            }
        }

        public override void UpdateConfiguration(BasePluginConfiguration configuration)
        {
            PluginDefinedTransformation[] previousTransforms = Configuration.Transformations;
            
            base.UpdateConfiguration(configuration);
            
            if (configuration is PluginConfiguration newConfig)
            {
                // Once the configuration has been updated we need to remove any that have been removed, add ones that have been added and update everything else
                foreach (PluginDefinedTransformation previousTransform in previousTransforms)
                {
                    if (!newConfig.Transformations.Any(x => x.Id == previousTransform.Id))
                    {
                        m_writeService.RemoveTransformation(previousTransform.Id);
                    }
                }

                foreach (PluginDefinedTransformation newTransform in newConfig.Transformations)
                {
                    if (previousTransforms.Any(x => x.Id == newTransform.Id))
                    {
                        m_writeService.UpdateTransformation(newTransform.Id, newTransform.FilenamePattern, (path, contents) => HandlePluginConfigTransformation(path, contents, newTransform));
                    }
                    else
                    {
                        m_writeService.AddTransformation(newTransform.Id, newTransform.FilenamePattern, (path, contents) => HandlePluginConfigTransformation(path, contents, newTransform));
                    }
                }
            }
        }

        private async Task HandlePluginConfigTransformation(string path, Stream contents, PluginDefinedTransformation transformation)
        {
            if (string.IsNullOrWhiteSpace(transformation.ReplaceText) || string.IsNullOrWhiteSpace(transformation.SearchText))
            {
                return;
            }
                    
            using StreamReader reader = new StreamReader(contents, leaveOpen: true);
            string currentContent = await reader.ReadToEndAsync();
                    
            contents.Seek(0, SeekOrigin.Begin);

            string transformedString = currentContent.Replace(transformation.SearchText, transformation.ReplaceText);
                    
            using StreamWriter textWriter = new StreamWriter(contents, null, -1, true);
            textWriter.Write(transformedString);
        }
        
        public IEnumerable<PluginPageInfo> GetPages()
        {
            string? prefix = GetType().Namespace;

            yield return new PluginPageInfo
            {
                Name = Name,
                EnableInMainMenu = true,
                EmbeddedResourcePath = $"{prefix}.Configuration.config.html"
            };
        }
    }
}
