# Bw Vault Digest

A tool designed to help you monitor the security and health of your Bitwarden vault. It compiles a digest of key metrics, including the number of passwords, their strength, and their age, and sends a summary email to the specified recipients.

## Getting Started

### Installation

1. Pull the Docker Image: Pull the Docker image from Docker Hub:

    ```shell
    docker pull danilbrenner/bw-vault-digests
    ```

2. Configure Azure Key Vault: Store your Bitwarden credentials (password and API key) in Azure Key Vault under the following names:

    * `bw-password`
    * `bw-clientid`
    * `bw-clientsecret`

3. Run the Docker Container: Start the Docker container with the required environment variables:

    ```shell
    docker run -d \
      -e SecretManager__VaultUrl="<Your Azure Key Vault URL>" \
      -e EmailNotifierOptions__From="<Sender Email>" \
      -e EmailNotifierOptions__To="<Recipient Email>" \
      -e EmailNotifierOptions__SmtpServer="<SMTP Server>" \
      -e EmailNotifierOptions__SmtpPort="<SMTP Port>" \
      -e EmailNotifierOptions__Username="<SMTP Username>" \
      -e EmailNotifierOptions__Password="<SMTP Password>" \
      -e APPLICATIONINSIGHTS_CONNECTION_STRING="<Your App Insights Connection String>" \
      -e AZURE_CLIENT_SECRET="<Your Azure Client Secret>" \
      -e AZURE_CLIENT_ID="<Your Azure Client ID>" \
      -e AZURE_TENANT_ID="<Your Azure Tenant ID>" \
      danilbrenner/bw-vault-digest
    ```