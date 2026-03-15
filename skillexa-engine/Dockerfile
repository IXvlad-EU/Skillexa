# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy Directory.Build.props, .editorconfig, and restore dependencies
COPY .editorconfig Directory.Build.props ./
COPY skillexa-engine/Skillexa.Engine.csproj skillexa-engine/
RUN dotnet restore skillexa-engine/Skillexa.Engine.csproj

# Copy everything else and build
COPY skillexa-engine/ skillexa-engine/
RUN dotnet publish skillexa-engine/Skillexa.Engine.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

USER app

# Run the application
ENTRYPOINT ["dotnet", "Skillexa.Engine.dll"]
