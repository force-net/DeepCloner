module.exports = function (grunt) {

	var solutionPath = "DeepCloner.sln";
    const packageSource = "https://www.myget.org/F/geckoprivate/api/v2/package";
    const symbolsSource = "https://www.myget.org/F/geckoprivate/symbols/api/v2/package";
    const nugetConfigFile = "nuget.config";
    const nugetPath = "nuget";

	// Project configuration.
    var dotnetClean = function (path, config) { return `dotnet clean ${path} --configuration ${config}`; };
    var dotnetRestore = function (path) { return `dotnet restore ${path}`; };
    var dotnetBuild = function (path, config) { return `dotnet build ${path} --configuration ${config}`; };
    var dotnetTest = function (path, config) { return `dotnet test ${path} --configuration ${config} --no-build -v n`; };

    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON("package.json"),
        shell: {
            createNugetFolder: { command: "mkdir nuget" },
            dotnetClean: { command: dotnetClean(solutionPath, "Release") },
            dotnetRestore: { command: dotnetRestore(solutionPath) },
            dotnetBuild: { command: configuration => dotnetBuild(solutionPath, configuration) },
            dotnetTest: { command: dotnetTest(".\\DeepCloner.Tests\\DeepCloner.Tests.csproj", "Release") },
            clearNuget: { options: { stdout: true }, command: ".\\node_modules\\grunt-nuget\\libs\\nuget.exe locals all -clear" }
        },
        
        cleanfiles: {
            options: {
                folders: true
            },
            src: [
                "TestResults/",
                nugetPath + "/*.nupkg",
                "DeepCloner*/**/bin/**",
                "DeepCloner*/**/obj/**",
                "dist/"
            ],
            nuget: [
                "nuget/**"
            ]
        },
        copy: {
            nuget: {
                files: [
                    {
                        expand: true,
                        flatten: true,
                        cwd: "",
                        src: ["**/*.nupkg"],
                        dest: `${nugetPath}/`
                    }
                ]
            }
        },
        // Tasks for pushing nuget package
        nugetpush: {
            pushDynamicPackage: {
                src: [nugetPath + "/*.nupkg", "!" + nugetPath + "/*.symbols.nupkg"],
                options: {
                    apiKey: process.env["ApiKey"],
                    configFile: nugetConfigFile, // keep this to avoid pushing packages from dev pcs
                    source: packageSource
                }
            },
            pushDynamicSymbols: {
                src: nugetPath + "/*.symbols.nupkg",
                options: {
                    apiKey: process.env["ApiKey"],
                    configFile: nugetConfigFile, // keep this to avoid pushing packages from dev pcs
                    source: symbolsSource
                }
            }
        }
    });

    grunt.loadNpmTasks("grunt-nuget");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-shell");
    grunt.loadNpmTasks("grunt-nucheck");
    grunt.loadNpmTasks("grunt-contrib-copy");

    grunt.renameTask("clean", "cleanfiles"); //Rename so that task-name 'clean' can be used as 'main' clean
    grunt.registerTask("clearNuget", ["shell:clearNuget"]);
    grunt.registerTask("clean", ["clearNuget", "cleanfiles"]);

    // Default task(s).
    grunt.registerTask("buildRelease", ["clean", "shell:dotnetRestore", "shell:dotnetClean", "shell:dotnetBuild:Release"]);
    grunt.registerTask("buildDebug", ["clean", "shell:dotnetRestore", "shell:dotnetClean", "shell:dotnetBuild:Debug"]);
    grunt.registerTask("test", ["shell:dotnetTest"]);
    grunt.registerTask("default", ["cleanfiles", "shell:dotnetRestore", "shell:dotnetClean", "shell:dotnetBuild:Release", "test"]);
    grunt.registerTask("defaultFullClean", ["clean", "shell:dotnetRestore", "shell:dotnetClean", "shell:dotnetBuild:Release", "test"]);
    grunt.registerTask("push", ["buildRelease", "copy:nuget", "nugetpush"]);
};
