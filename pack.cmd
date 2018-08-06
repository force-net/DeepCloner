dotnet build -c BuildCore DeepCloner\DeepCloner.Core.csproj
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\sn.exe" -R DeepCloner\bin\BuildCore\net40\DeepCloner.dll private.snk 
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\sn.exe" -R DeepCloner\bin\BuildCore\netstandard1.3\DeepCloner.dll private.snk
.tools\nuget.exe pack
xcopy *.nupkg .tools
del *.nupkg

