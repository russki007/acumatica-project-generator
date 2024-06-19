#tool nuget:?package=xunit.runner.console&version=2.2.0

var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Debug");
var verbosity = Argument("Verbosity", Verbosity.Quiet);
var outputPath = Argument<DirectoryPath>("outputPath", Context.Environment.WorkingDirectory.Combine("artifacts"));
var siteDir = Directory(Argument("SitePath", "../../Site"));
if (HasEnvironmentVariable("ACC_SITE_PATH")) siteDir = Directory(EnvironmentVariable("ACC_SITE_PATH"));
var cpuCount = Argument<int?>("cpuCount", null);
var version = Argument("PackageVersion", "1.0.0");

const string PackageName = "#PROJECT#";
const string CsPackExe = "cspack.exe";
private DirectoryPath BuildDir;

public static class Paths {
       public static DirectoryPath RootDir => ".";
       public static DirectoryPath SrcDir => $"{RootDir}/src";
       public static DirectoryPath TestsDir => $"{RootDir}/tests";
       public static DirectoryPath ExtentionsDir => $"{RootDir}/assets";
       public static FilePath ProjectFile => $"{SrcDir}/#PROJECT#/#PROJECT#.csproj";
       public static FilePath CsPackExePath => $"{RootDir}/tools/cspack/{CsPackExe}";
}

Setup(ctx => {
	Context.Tools.RegisterFile(MakeAbsolute(Paths.CsPackExePath));
	if (DirectoryExists(outputPath)) CleanDirectory(outputPath);
	EnsureDirectoryExists(outputPath);
	BuildDir = $"{Paths.RootDir}/build/build_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
	EnsureDirectoryExists(BuildDir);
});

NuGetRestoreSettings CreateNuGetRestoreSettings(Verbosity verbosity) {
   var settings = new NuGetRestoreSettings();
   if (verbosity == Verbosity.Quiet || verbosity == Verbosity.Minimal)
       settings.Verbosity = NuGetVerbosity.Quiet;
   return settings;
}

Task("Restore")
    .Does(() => {
	Information("Restore NuGet Packages");
	NuGetRestore(Paths.ProjectFile, CreateNuGetRestoreSettings(verbosity));
});

MSBuildSettings CreateMSBuildSettings(Verbosity verbosity) {
    var settings = new MSBuildSettings()
        .SetConfiguration(configuration)
        .SetVerbosity(verbosity)
        .SetMaxCpuCount(cpuCount)
		//.UseToolVersion(MSBuildToolVersion.VS2019)
        .WithTarget("build");

    if (verbosity == Verbosity.Quiet || verbosity == Verbosity.Minimal)
        settings.ArgumentCustomization = args => args.Append("/consoleloggerparameters:ErrorsOnly");
    
    return settings;
}


Task("Compile")
	.Does(() => 
{
	MSBuild(Paths.ProjectFile, 	
		CreateMSBuildSettings(verbosity));
	 		//.WithProperty("OutputPath", $"{buildDir}/bin"));

	var binDir = $"{BuildDir}/bin";
	if (!DirectoryExists(binDir)) {
		CreateDirectory(binDir);
	}
	CopyFiles($"{Paths.ProjectFile.GetDirectory()}/bin/{configuration}/**/{PackageName}*.(dll|pdb|xml)", binDir);
});


Task("Package")
	.Does(() => 
{
	EnsureDirectoryExists($"{BuildDir}/_project");
	EnsureDirectoryExists($"{BuildDir}/Pages/MA");
	EnsureDirectoryExists($"{BuildDir}/ReportsDefault");

	CopyFiles($"{Paths.ExtentionsDir}/Pages/**/*.(aspx|cs)", $"{BuildDir}/Pages/MA");
	CopyFiles($"{Paths.ExtentionsDir}/Reports/**/*.rpx", $"{BuildDir}/ReportsDefault");
	CopyFiles($"{Paths.ExtentionsDir}/**/*.xml",			$"{BuildDir}/_project");

	var packageDescription = $"#PROJECT# v{version}";
	Information($"Assembling customization package '{packageDescription}'");
	Information("Path to site directory: {0}", siteDir);
	FilePath csPackExePath = Context.Tools.Resolve(CsPackExe);
	IEnumerable<string> redirectedStandardOutput;
	var exitCodeWithArgument = StartProcess(csPackExePath, new ProcessSettings {
             Arguments = new ProcessArgumentBuilder()
				.Append($"/description \"{packageDescription}\"")
				.Append($"/website \"{siteDir}\"")
				.Append($"/in {BuildDir} /out {outputPath}/{PackageName}.{version}.zip")
				.Append($"/include \"{BuildDir}/bin/#PROJECT#.dll\" \"bin\\#PROJECT#.dll\""),
             RedirectStandardOutput = true
             },
            out redirectedStandardOutput
     	);

	Information("{0}", String.Join(Environment.NewLine, redirectedStandardOutput));
	Information("Exit code: {0}", exitCodeWithArgument);
});


Task("UnitTests")
    .Does(() =>
{
    var testProjects = GetFiles($"{Paths.TestsDir}/**/*Tests.csproj").ToArray();
    foreach (var testProject in testProjects) {
        MSBuild(testProject, CreateMSBuildSettings(verbosity));
    }

    XUnit2($"{Paths.TestsDir}/**/bin/{configuration}/**/*.*Tests.dll",
        new XUnit2Settings {
		    XmlReport = true,
		    OutputDirectory = outputPath
    });
});


/*
Task("Publish")
    .IsDependentOn("Package")
	.WithCriteria(() => BuildSystem.IsRunningOnTeamCity)
    .Does(() =>
{
    //BuildSystem.TeamCity.PublishArtifacts(Paths.PackageOutputPath);
});
*/


Task("Default")
	.IsDependentOn("Restore")
	.IsDependentOn("Compile")
	.IsDependentOn("Package")
	.Does(()=> 
{
	DeleteDirectory(BuildDir, new DeleteDirectorySettings {
		Recursive = true,
		Force = true
	});
});

RunTarget(target);

