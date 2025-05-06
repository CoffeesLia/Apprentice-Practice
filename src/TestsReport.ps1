Get-ChildItem -Path "./" -Recurse -Directory -Filter "TestResults" | Remove-Item -Recurse -Force
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:*/TestResults/*/coverage.cobertura.xml -targetdir:TestsCoverageReport -reporttypes:Html
start-process TestsCoverageReport/index.html