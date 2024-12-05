# Use the official .NET SDK image for .NET 8 to build the app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project files and restore dependencies
COPY src/Bw.VaultDigest.Common/ ./src/Bw.VaultDigest.Common/
COPY src/Bw.VaultDigest.Telemetry/ ./src/Bw.VaultDigest.Telemetry
COPY src/Bw.VaultDigest.Infrastructure/ ./src/Bw.VaultDigest.Infrastructure/
COPY src/Bw.VaultDigest.Model/ ./src/Bw.VaultDigest.Model/
COPY src/Bw.VaultDigest.Data/ ./src/Bw.VaultDigest.Data/
COPY src/Bw.VaultDigest.Bot/ ./src/Bw.VaultDigest.Bot/

RUN dotnet restore src/Bw.VaultDigest.Bot/Bw.VaultDigest.Bot.csproj

# Build the application
RUN dotnet publish src/Bw.VaultDigest.Bot/Bw.VaultDigest.Bot.csproj -c Release -o /app/out

# Use the official .NET runtime image for .NET 8 to run the app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Install Bitwarden CLI
RUN apt-get update && \
    apt-get install -y wget unzip curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_20.x -o nodesource_setup.sh && \
    bash nodesource_setup.sh && \
    apt-get install -y nodejs && \
    apt-get clean && \
    rm nodesource_setup.sh && \
    rm -rf /var/lib/apt/lists/*

RUN npm install -g @bitwarden/cli

# Expose the port on which the app will run
EXPOSE 8080

# Set the entry point for the application
ENTRYPOINT ["dotnet", "Bw.VaultDigest.Bot.dll"]