FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG TARGETARCH
WORKDIR /app

# Copy the project files and restore dependencies
COPY src/Bw.VaultDigest.Common/ ./src/Bw.VaultDigest.Common/
COPY src/Bw.VaultDigest.Telemetry/ ./src/Bw.VaultDigest.Telemetry
COPY src/Bw.VaultDigest.Infrastructure/ ./src/Bw.VaultDigest.Infrastructure/
COPY src/Bw.VaultDigest.Model/ ./src/Bw.VaultDigest.Model/
COPY src/Bw.VaultDigest.Web/ ./src/Bw.VaultDigest.Web/

RUN dotnet restore -a $TARGETARCH src/Bw.VaultDigest.Web/Bw.VaultDigest.Web.csproj

# Build the application
RUN dotnet publish src/Bw.VaultDigest.Web/Bw.VaultDigest.Web.csproj -a $TARGETARCH -c Release -o /app/out

# Use the official .NET runtime image for .NET 8 to run the app
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
ARG TARGETARCH
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
ENTRYPOINT ["dotnet", "Bw.VaultDigest.Web.dll"]
