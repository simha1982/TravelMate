# TravelMate Infrastructure

Infrastructure is defined with Bicep.

## MVP Resources

- Azure App Service for `TravelMate.Api`
- Azure SQL Database for core app data
- Azure Storage Blob container for story audio
- Azure AI Search for future keyword/vector retrieval
- Azure AI Speech for speech-to-text and text-to-speech
- Azure OpenAI account for summarization, personalization, and RAG
- Azure Key Vault for secrets
- Application Insights and Log Analytics for observability

## Deploy

```powershell
az group create --name rg-travelmate-dev --location eastus

az deployment group create `
  --resource-group rg-travelmate-dev `
  --template-file .\main.bicep `
  --parameters environmentName=dev `
  --parameters sqlAdministratorLogin=travelmateadmin `
  --parameters sqlAdministratorPassword='<secure-password>'
```

Use Key Vault or a secure pipeline variable for the SQL password in CI/CD.
