# Use the official .NET SDK image for .NET 8 to build the app
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project files and restore dependencies
COPY src/Bw.VaultDigest.Common/ ./src/Bw.VaultDigest.Common/
COPY src/Bw.VaultDigest.Infrastructure/ ./src/Bw.VaultDigest.Infrastructure/
COPY src/Bw.VaultDigest.Model/ ./src/Bw.VaultDigest.Model/
COPY src/Bw.VaultDigest.Web/ ./src/Bw.VaultDigest.Web/
RUN dotnet restore src/Bw.VaultDigest.Web/Bw.VaultDigest.Web.csproj

# Build the application
RUN dotnet publish src/Bw.VaultDigest.Web/Bw.VaultDigest.Web.csproj -c Release -o /app/out

# Use the official .NET runtime image for .NET 8 to run the app
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Install Bitwarden CLI
RUN apt-get update && \
    apt-get install -y wget unzip && \
    wget -qO /tmp/bw.zip "https://vault.bitwarden.com/download/?app=cli&platform=linux" && \
    unzip /tmp/bw.zip -d /usr/local/bin && \
    chmod +x /usr/local/bin/bw && \
    rm /tmp/bw.zip && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Expose the port on which the app will run
EXPOSE 80

# Set the entry point for the application
ENTRYPOINT ["dotnet", "Bw.VaultDigest.Web.dll"]