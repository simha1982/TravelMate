# TravelMate Mobile Smoke Test

Use this checklist before demoing the MAUI app.

## Automated Build

Run from the repo root:

```powershell
.\scripts\test-mobile.ps1
```

For a faster mobile-only build:

```powershell
.\scripts\test-mobile.ps1 -SkipTests
```

## Manual Demo Flow

1. Start `TravelMate.Api` locally.
2. Launch `TravelMate.Mobile` on Windows or Android.
3. Tap `Start demo trip`.
4. Use `Next stop` to move through the Hyderabad route.
5. Confirm each stop updates the map and shows multiple nearby story cards.
6. Select a story, tap `Play`, and confirm audio plays inside the app.
7. Let the audio finish and confirm the app saves a completed playback event.
8. Tap `Not interested`, reload the same location, and confirm the story is not recommended again.
9. Use typed commands: `yes`, `no`, `play`, `skip`, `next`, and `previous`.
10. Submit a contribution from the mobile form.
11. Open the admin portal moderation queue and confirm the contribution is waiting for review.

## Cloud Readiness Checks

These remain placeholder-backed until credentials are supplied:

- Azure SQL connection string.
- Azure OpenAI endpoint, API key, and deployment names.
- Azure Speech region and API key.
- Azure AI Search endpoint and key.
- Azure AD B2C tenant, authority, audience, and admin role mapping.
- GitHub Actions Azure publish profile or federated identity secrets.
