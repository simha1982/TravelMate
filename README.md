# TravelMate

TravelMate is the first runnable slice of the travel storytelling app described in `..\docs\travel-app-hld.md`.

The MVP backend is a C#/.NET API that can:

- Return nearby travel stories from GPS coordinates.
- Rank stories using distance, content quality, and user interests.
- Save user preferences.
- Capture playback feedback such as played, skipped, and interested.
- Answer a simple "tell me about this place" conversation request.
- Keep AI behind an `IModelGateway` abstraction so Azure OpenAI or on-prem models can be added without changing business logic.

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
    `-- TravelMate.Workers
```

## Run Locally

```powershell
dotnet run --project .\src\TravelMate.Api\TravelMate.Api.csproj
```

Open:

```text
https://localhost:7001/
```

Example nearby story request for Nandi Hills:

```http
GET /api/stories/nearby?userId=demo-user&lat=13.3702&lon=77.6835&radiusMeters=5000
```

## Next Build Steps

1. Replace in-memory repository with EF Core and Azure SQL.
2. Add Azure Blob Storage for generated audio.
3. Add Azure AI Speech for text-to-speech and speech-to-text.
4. Add Azure OpenAI implementation for `IModelGateway`.
5. Add .NET MAUI mobile app for location, story prompts, and playback.
6. Add Azure AI Search for vector and keyword story retrieval.

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
