using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Builds.OSX;

internal class GenerateMacPList : IMiddleware<BuildContext>
{
    public bool ShouldRun(BuildContext context) => context.Runtime == BuildRuntime.Osx;
    public async Task<BuildContext> OnInvoke(BuildContext context, ContextDelegate<BuildContext> next)
    {
        var plistPath = Path.Combine(context.MacBundle?.ContentPath!, "Info.plist");

        var version = GetType().Assembly.GetName().Version?.ToString(3);
        if (version == null)
        {
            return context with { Failed = true, Reason = $"Failed to read the version from assembly {GetType().Assembly.GetName().Name}" };
        }

        Logger.Trace($"Product version: {version}");
        var plist = CreatePList(version, "Gamecure", "Gamecure.GUI", "goals.icns");

        await File.WriteAllTextAsync(plistPath, plist);
        return await next(context);
    }

    private static string CreatePList(string version, string bundleName, string executable, string iconName) => @$"
<?xml version=""1.0"" encoding=""UTF-8"" ?>
<!DOCTYPE plist PUBLIC "" -//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version = ""1.0"" >
    <dict>
        <key>CFBundleIconFile</key>
        <string>{iconName}</string>
        <key>CFBundleIdentifier</key>
        <string>com.goals</string>
        <key>CFBundleName</key>
        <string>{bundleName}</string>
        <key>CFBundleVersion</key>
        <string>{version}</string>
        <key>LSMinimumSystemVersion</key>
        <string>10.12</string>
        <key>CFBundleExecutable</key>
        <string>{executable}</string>
        <key>CFBundleInfoDictionaryVersion</key>
        <string>6.0</string>
        <key>CFBundlePackageType</key>
        <string>APPL</string>
        <key>CFBundleShortVersionString</key>
        <string>1.0</string>
        <key>NSHighResolutionCapable</key>
        <true/>
    </dict>
</plist>";
}