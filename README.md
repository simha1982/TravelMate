# TravelMate

TravelMate is the first runnable slice of the travel storytelling app described in `..\docs\travel-app-hld.md`.

The MVP backend is a C#/.NET API that can:

- Return nearby travel stories from GPS coordinates.
- Rank stories using distance, content quality, and user interests.
- Save user preferences.
- Capture playback feedback such as played, skipped, and interested.
- Answer a simple "tell me about this place" conversation request.
- Store data through EF Core using SQL Server in Azure or an in-memory database locally.
- Call Azure OpenAI and Azure Speech through gateway abstractions, with local stubs when cloud settings are absent.
- Save generated story audio to local disk or Azure Blob Storage.
- Index and search curated stories through local in-memory search or Azure AI Search.
- Accept user story contributions and create moderation review records.
- Run a .NET MAUI Android-first mobile app for location, nearby story prompts, and feedback.

## Solution Structure

```text
TravelMate/
|-- TravelMate.sln
`-- src/
    |-- TravelMate.Api
    |-- TravelMate.Application
    |-- TravelMate.Domain
    |-- TravelMate.Infrastructure
    |-- TravelMate.AI
    |-- TravelMate.Workers
    `-- TravelMate.Mobile
tests/
`-- TravelMate.Tests
infra/
`-- main.bicep
```

## Run Locally

```powershell
dotnet run --project .\src\TravelMate.Api\TravelMate.Api.csproj
```

Open:

```text
http://localhost:5068/
```

Example nearby story request for Nandi Hills:

```http
GET /api/stories/nearby?userId=demo-user&lat=13.3702&lon=77.6835&radiusMeters=5000
```

Run tests:

```powershell
dotnet test .\TravelMate.sln
```

## Configuration

Local development uses EF Core InMemory when `ConnectionStrings:TravelMateSql` is empty.

Set these values for Azure-backed development:

```json
{
  "ConnectionStrings": {
    "TravelMateSql": "<azure-sql-connection-string>"
  },
  "AzureOpenAI": {
    "Endpoint": "https://<account>.openai.azure.com",
    "ApiKey": "<key>",
    "ChatDeployment": "<chat-deployment>",
    "EmbeddingDeployment": "<embedding-deployment>"
  },
  "AzureSpeech": {
    "Region": "<region>",
    "ApiKey": "<key>",
    "DefaultVoiceName": "en-US-JennyNeural"
  },
  "AudioStorage": {
    "ConnectionString": "<storage-connection-string>",
    "ContainerName": "story-audio"
  },
  "AzureSearch": {
    "Endpoint": "https://<search-service>.search.windows.net",
    "ApiKey": "<admin-or-query-key>",
    "IndexName": "travelmate-stories"
  },
  "Auth": {
    "RequireApiKey": true,
    "ApiKey": "<temporary-api-key>"
  }
}
```

Use `appsettings.Local.json`, user secrets, Key Vault, or pipeline secrets for real keys.

## Next Build Steps

1. Replace optional API-key gate with Entra ID B2C JWT validation.
2. Create the Azure AI Search index schema during deployment.
3. Add real Blob SAS playback URLs for mobile audio.
4. Add payment/subscription entitlements.
5. Add human moderator portal.
6. Add richer MAUI audio playback and voice command UX.

## Infrastructure

Bicep templates live in `infra/`.

```powershell
az deployment group create `
  --resource-group rg-travelmate-dev `
  --template-file .\infra\main.bicep `
  --parameters environmentName=dev `
  --parameters sqlAdministratorLogin=travelmateadmin `
  --parameters sqlAdministratorPassword='<secure-password>'
```
