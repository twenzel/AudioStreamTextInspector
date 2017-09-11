// ARGUMENTS
var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");

// GLOBAL VARIABLES
var artifactsDirectory = Directory("./artifacts");
var projectFile = "./src/AudioStreamTextInspector/AudioStreamTextInspector.csproj";
var IsOnAppVeyorAndNotPR = AppVeyor.IsRunningOnAppVeyor && !AppVeyor.Environment.PullRequest.IsPullRequest;

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDirectory);

        if(BuildSystem.IsLocalBuild)
        {
            CleanDirectories(GetDirectories("./**/obj") + GetDirectories("./**/bin"));
        }
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore(projectFile);
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {        
		var settings = new DotNetCorePublishSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsDirectory
        };

		DotNetCorePublish(projectFile, settings);
    });
      

Task("Pack")
    .IsDependentOn("Restore")
    .WithCriteria(IsOnAppVeyorAndNotPR || string.Equals(target, "pack", StringComparison.OrdinalIgnoreCase))
    .Does(() =>
    {
        var settings = new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsDirectory
        };
     
        DotNetCorePack(projectFile, settings);       
    });

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Pack");

RunTarget(target);