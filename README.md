# TravelMate

TravelMate is the first runnable slice of the travel storytelling app described in `..\docs\travel-app-hld.md`.

The MVP backend is a C#/.NET API that can:

- Return nearby travel stories from GPS coordinates.
- Rank stories using distance, content quality, and user interests.
- Save user preferences.
- Store explicit location, voice, and personalization consent.
- Capture playback feedback such as played, skipped, and interested.
- Answer a simple "tell me about this place" conversation request.
- Store data through EF Core using SQL Server in Azure or an in-memory database locally.
- Call Azure OpenAI and Azure Speech through gateway abstractions, with local stubs when cloud settings are absent.
- Save generated story audio to local disk or Azure Blob Storage.
- Index and search curated stories through local in-memory search or Azure AI Search.
- Accept user story contributions and create moderation review records.
- Enforce free vs premium story playback entitlements.
- Provide a C# Razor Pages admin portal for human moderation decisions.
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
    |-- TravelMate.Admin
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

Run the admin moderation portal after starting the API:

```powershell
dotnet run --project .\src\TravelMate.Admin\TravelMate.Admin.csproj --urls http://localhost:5075
```

Open `http://localhost:5075/` to review submitted contributions and approve or reject them.

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
    "ContainerName": "story-audio",
    "SasHours": 12
  },
  "AzureSearch": {
    "Endpoint": "https://<search-service>.search.windows.net",
    "ApiKey": "<admin-or-query-key>",
    "IndexName": "travelmate-stories"
  },
  "Auth": {
    "RequireApiKey": true,
    "ApiKey": "<temporary-api-key>"
  },
  "AzureAdB2C": {
    "Enabled": true,
    "Authority": "https://<tenant>.b2clogin.com/<tenant>.onmicrosoft.com/<policy>/v2.0",
    "Audience": "<api-client-id>"
  }
}
```

Use `appsettings.Local.json`, user secrets, Key Vault, or pipeline secrets for real keys.

## Next Build Steps

1. Add EF Core migrations for Azure SQL rollout.
2. Add richer MAUI audio playback and voice command UX.
3. Add AI call audit logging and cost tracking.
4. Add synthetic route tests for the pilot geography.
5. Add production deployment pipeline stages for API and admin portal.

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
