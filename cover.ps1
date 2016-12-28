nuget install -OutputDirectory packages -Version 4.6.519 OpenCover
nuget install -OutputDirectory packages -Version 2.5.1 ReportGenerator

.\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe `
	-register:user -target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test src\Pingu.Tests\Pingu.Tests.csproj" -output:"coverage.xml" -oldstyle `
	-filter:+[Pingu]* -threshold:1

.\packages\ReportGenerator.2.5.1\tools\ReportGenerator.exe -reports:coverage.xml -targetdir:coverage

start coverage\index.htm