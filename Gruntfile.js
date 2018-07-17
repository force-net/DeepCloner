module.exports = function (grunt) {
	var solutionPath = "DeepCloner.sln";
	var nugetOutFolder = "nuget";
	// Project configuration.
	grunt.initConfig({
		pkg: grunt.file.readJSON("package.json"),
		msbuild: {
			dev: {
				src: [solutionPath],
				options: {
					projectConfiguration: "Release",
					targets: ["Clean", "Rebuild"],
					stdout: true,
					maxCpuCount: 4,
					buildParameters: {
						WarningLevel: 2
					},
					verbosity: "Minimal" //Quiet, Minimal, Normal, Detailed, Diagnostic
				}
			},
			clean: {
				src: [solutionPath],
				options: {
					projectConfiguration: "Release",
					targets: ["Clean"],
					stdout: true,
					maxCpuCount: 4,
					buildParameters: {
						WarningLevel: 2
					},
					verbosity: "Minimal" //Quiet, Minimal, Normal, Detailed, Diagnostic
				}
			}

		},
		nugetrestore: {
			restore: {
				src: solutionPath,
				dest: "packages",
				options: {
					configFile: "nuget.config"
				}
			}
		},
		cleanfiles: {
			options: {
				folders: true
			},
			src: [
				nugetOutFolder,
				"DeepCloner*/bin/**",
				"DeepCloner*/obj/**"
			]
		},
		nugetpack: {
			dist: {
				src: "DeepCloner/DeepCloner.csproj",
				dest: nugetOutFolder
			},
			options: {
				build: true,
				symbols: true,
				properties: "Configuration=Release"
			}
		},
		// Tasks for pushing nuget package
		nugetpush: {
			packs: {
				src: [nugetOutFolder + "/*.nupkg", "!" + nugetOutFolder + "/*.symbols.nupkg"],
				options: {
					apiKey: process.env["ApiKey"],
					configFile: "nuget.config",
					source: "https://www.myget.org/F/geckoprivate/api/v2/package"
				}
			},
			symbols: {
				src: nugetOutFolder + "/*.symbols.nupkg",
				options: {
					apiKey: process.env["ApiKey"],
					configFile: "nuget.config",
					source: "https://www.myget.org/F/geckoprivate/symbols/api/v2/package"
				}
			}
		}
	});

	grunt.loadNpmTasks("grunt-msbuild");
	grunt.loadNpmTasks("grunt-nuget");
	grunt.loadNpmTasks("grunt-exec");
	grunt.loadNpmTasks("grunt-contrib-copy");
	grunt.loadNpmTasks("grunt-contrib-clean");

	grunt.renameTask("clean", "cleanfiles"); //Rename so that task-name 'clean' can be used as 'main' clean

	// Default task(s).
	grunt.registerTask("build", ["nugetrestore", "msbuild:dev"]);

	grunt.registerTask("clean", ["msbuild:clean", "cleanfiles"]);
	grunt.registerTask("push", ["cleanfiles", "build", "nugetpack", "nugetpush"]);
	grunt.registerTask("default", ["cleanfiles", "build"]);
};
