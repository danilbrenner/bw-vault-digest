# Bw Sync Bot

Sync Bitwarden passwords to sqlite DB...

2. Configure Azure Key Vault: Store your Bitwarden credentials (password and API key) in Azure Key Vault under the following names:

    * `bw-password`
    * `bw-clientid`
    * `bw-clientsecret`

3. Run the Docker Container: Required environment variables:

    ```shell
      -e SecretManager__VaultUrl="<Your Azure Key Vault URL>" \
      -e APPLICATIONINSIGHTS_CONNECTION_STRING="<Your App Insights Connection String>" \
      -e AZURE_CLIENT_SECRET="<Your Azure Client Secret>" \
      -e AZURE_CLIENT_ID="<Your Azure Client ID>" \
      -e AZURE_TENANT_ID="<Your Azure Tenant ID>" \
    ```