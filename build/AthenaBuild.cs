using System;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Xml;
using System.Xml.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

internal class AthenaBuild : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<AthenaBuild>(x => x.Build);

    [Parameter("Configuration to build: Release (Default) or Debug")]
    private readonly Configuration Configuration = Configuration.Release;

    [Parameter("apk or aab (default)")] 
    private readonly string PackageFormat = "aab";

    [Parameter("Signing keystore alias")]
    [Secret]
    private readonly string AndroidSigningKeyAlias;

    [Parameter("Signing key pass")]
    [Secret]
    private readonly string AndroidSingingKeyPassword;

    [Parameter("Signing key store")]
    private readonly string AndroidSigningKeyStore = BuildDirectory / "signing" / "googleplay.jks";

    [Parameter]
    private readonly string Solution;

    private AbsolutePath SourceDirectory => RootDirectory / "src";
    private static AbsolutePath BuildDirectory => RootDirectory / "build";
    private static AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetClean(_ => _
                .SetProject(Path.Combine(SourceDirectory, Solution)));
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(_ => _ 
                .SetProjectFile(Path.Combine(SourceDirectory, Solution)));
        });

    Target UpdateVersionNumber => _ => _
        .Executes(
            UpdateVersionNumberCore);

    Target Build => _ => _
        .DependsOn(UpdateVersionNumber)
        .DependsOn(Restore)
        .Executes(BuildCore);

    private void BuildCore()
    {
        DotNetTasks.DotNetPublish(_ => _
            .SetProject(Path.Combine(SourceDirectory, Solution))
            .SetConfiguration(Configuration)
            .SetFramework("net9.0-android")
            .AddProcessAdditionalArguments("--no-restore")
            .AddProperty("AndroidPackageFormats", PackageFormat)
            .AddProperty("AndroidKeyStore", true)
            .AddProperty("AndroidSigningKeyStore", AndroidSigningKeyStore)
            .AddProperty("AndroidSigningKeyAlias", AndroidSigningKeyAlias)
            .AddProperty("AndroidSigningKeyPass", AndroidSingingKeyPassword)
            .AddProperty("AndroidSigningStorePass", AndroidSingingKeyPassword)
            .SetOutput(OutputDirectory));

        Serilog.Log.Warning("Don't forget to commit the changes in version.props!");
    }

    private void UpdateVersionNumberCore()
    {
        XDocument versionDoc = XDocument.Load(RootDirectory / "version.props");

        var group = versionDoc.Root.Element("PropertyGroup");
        string displayVersion = group.Element("ApplicationDisplayVersion").Value;
        int appVersion = int.Parse(group.Element("ApplicationVersion").Value);
        appVersion++;

        group.Element("ApplicationVersion").Value = appVersion.ToString();
        versionDoc.Save(RootDirectory / "version.props");

        Serilog.Log.Debug($"ApplicationVersion=[{appVersion}], ApplicationDisplayVersion=[{displayVersion}]");
    }

}
