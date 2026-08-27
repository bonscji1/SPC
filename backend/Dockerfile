FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY frontend/global.json frontend/Directory.Build.props frontend/
COPY frontend/src/SPC.Core frontend/src/SPC.Core
COPY backend/global.json backend/Directory.Build.props backend/
COPY backend/src/SPC.Api backend/src/SPC.Api
WORKDIR /src/backend
RUN dotnet publish src/SPC.Api/SPC.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SPC.Api.dll"]
