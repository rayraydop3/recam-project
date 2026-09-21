# RECAM

Real-estate media delivery platform connecting photography companies (Admin) with real-estate agents (Agent).

| Folder | Description | Stack |
|---|---|---|
| [Management-tool-backend](Management-tool-backend) | REST API, auth, media storage | ASP.NET Core (.NET 9) |
| [recam-frontend](recam-frontend) | Admin / Agent web app | React + TypeScript + Vite |
| [recam-ai-agent](recam-ai-agent) | AI agent that calls the backend API | Python |

See [RECAM_Project_Outline.md](RECAM_Project_Outline.md) for the full project outline.

## Configuration

Secrets are not committed.

- Backend: put real values for `ConnectionStrings`, `Email`, `Jwt` and Azure Blob Storage in `Management-tool-backend/appsettings.Development.json` (git-ignored). `appsettings.json` only holds placeholders.
- Frontend: copy the variables from your own `.env` (`VITE_API_BASE_URL`).
- AI agent: copy `recam-ai-agent/.env.example` to `.env` and fill it in.
