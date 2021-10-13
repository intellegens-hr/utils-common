cd ..
cd dotnetcore

get-childitem -include *.nupkg -recurse | remove-item

dotnet clean
dotnet build -c Release
dotnet pack -c Release

copy .\Intellegens.Commons\bin\Release\*.nupkg .
copy .\Intellegens.Commons.Db\bin\Release\*.nupkg .
copy .\Intellegens.Commons.Http\bin\Release\*.nupkg .
copy .\Intellegens.Commons.Mvc\bin\Release\*.nupkg .
copy .\Intellegens.Commons.Search\bin\Release\*.nupkg .
copy .\Intellegens.Commons.Search.Abstractions\bin\Release\*.nupkg .
copy .\Intellegens.Commons.Search.Postgres\bin\Release\*.nupkg .
copy .\Intellegens.Commons.Services\bin\Release\*.nupkg .
copy .\Intellegens.Commons.Services.Abstractions\bin\Release\*.nupkg .

dotnet nuget push "*.nupkg" --api-key $env:IntellegensNugetApiKey --source https://api.nuget.org/v3/index.json --skip-duplicate

del *.nupkg