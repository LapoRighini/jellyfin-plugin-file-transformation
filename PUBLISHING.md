# Publishing this fork

1. Create a GitHub fork named `jellyfin-plugin-file-transformation` and push this project to its `main` branch.
2. Replace `REPLACE_WITH_YOUR_GITHUB_USER` in the project file and `manifest.json` with the GitHub account or organisation name.
3. Push a tag such as `v3.0.0.0`.
4. The release workflow builds `FileTransformation.zip`, publishes the GitHub release, calculates the required MD5 checksum, and prepends the release to `manifest.json`.
5. Add the resulting URL to Jellyfin: `https://raw.githubusercontent.com/<owner>/jellyfin-plugin-file-transformation/main/manifest.json`.

The plugin is intentionally marked for the `12.0.0.0` ABI. It must not be offered to Jellyfin 10.11 installations.

## Dependent plugins

The public reflection surface remains `Jellyfin.Plugin.FileTransformation.PluginInterface.RegisterTransformation`. This preserves the integration contract used by dependent plugins such as Media Bar, Home Sections, and Jellyfin Enhanced. Those plugins still need their own Jellyfin 12 builds; install only versions whose manifest declares `targetAbi` `12.0.0.0` and lists this plugin GUID as a dependency.
