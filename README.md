## Smart Apiary (starter)

Telemetry + realtime updates starter project.

- **Backend**: ASP.NET Core Web API (C#) + EF Core + PostgreSQL + SignalR
- **Frontend**: React + TypeScript (Vite) + Recharts

## Repo structure

- `backend/SmartApiary.Api/`: ASP.NET Core API
- `frontend/`: React app
- `docker-compose.yml`: PostgreSQL for local dev

## Run (local dev)

Start PostgreSQL:

```bash
docker compose up -d
```

Run backend:

```bash
cd backend/SmartApiary.Api
dotnet tool run dotnet-ef database update
dotnet run
```

Run frontend:

```bash
cd frontend
npm install
npm run dev
```

Open the UI at `http://localhost:5173`.

## API

- `POST /api/telemetry`: insert telemetry (also broadcasts via SignalR)
- `GET /api/telemetry/{hiveId}`: list telemetry for a hive

You can use `backend/SmartApiary.Api/SmartApiary.Api.http` to try requests.

## Realtime (SignalR)

Hub endpoint: `/hubs/telemetry`

- Client calls `SubscribeHive(hiveId)` to receive per-hive updates
- Server broadcasts `telemetryInserted` with the inserted telemetry payload

