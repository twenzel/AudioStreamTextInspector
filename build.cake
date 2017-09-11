// ARGUMENTS
var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");

// GLOBAL VARIABLES
var artifactsDirectory = "./artifacts";
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
    .IsDependentOn("Build")
    .WithCriteria(IsOnAppVeyorAndNotPR || string.Equals(target, "pack", StringComparison.OrdinalIgnoreCase) || string.Equals(target, "push", StringComparison.OrdinalIgnoreCase))
    .Does(() =>
    {
        var settings = new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsDirectory
        };
     
        DotNetCorePack(projectFile, settings);       
    });
	
Task("Push")
    .IsDependentOn("Pack")
    .WithCriteria(IsOnAppVeyorAndNotPR || string.Equals(target, "push", StringComparison.OrdinalIgnoreCase))
    .Does(() =>
    {        
        var settings = new DotNetCoreNuGetPushSettings
		 {
			 Source = "https://www.nuget.org/api/v2/package",
			 ApiKey = EnvironmentVariable("NUGET_API_KEY")
		 };
		DotNetCoreNuGetPush("artifacts\\*.nupkg", settings);      
    });

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
	.IsDependentOn("Pack")
    .IsDependentOn("Push");

RunTarget(target);