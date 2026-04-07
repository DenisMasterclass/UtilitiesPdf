$apiProject = 'D:\PortoBank\dev\UtilitiesPdf\UtilitiesPdf.Api\UtilitiesPdf.Api.csproj'
$uiProject = 'D:\PortoBank\dev\UtilitiesPdf\UtilitiesPdf.FollowUp\UtilitiesPdf.FollowUp.csproj'

Start-Process powershell -ArgumentList '-NoExit', '-Command', "dotnet run --project `"$apiProject`""
Start-Process powershell -ArgumentList '-NoExit', '-Command', "dotnet run --project `"$uiProject`""
