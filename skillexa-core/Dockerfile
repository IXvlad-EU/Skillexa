# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY .editorconfig Directory.Build.props ./
COPY skillexa-core/Skillexa.Core.csproj skillexa-core/
RUN dotnet restore skillexa-core/Skillexa.Core.csproj

COPY skillexa-core/ skillexa-core/
RUN dotnet publish skillexa-core/Skillexa.Core.csproj -c $BUILD_CONFIGURATION -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

USER app

EXPOSE 8080

ENTRYPOINT ["dotnet", "Skillexa.Core.dll"]