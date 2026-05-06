# WeatherApp — Dallas Historical Weather

A .NET 8 Web API + React (Vite) application that reads a list of dates from `dates.txt`, fetches historical weather data for Dallas, TX from the [Open-Meteo Historical Weather API](https://open-meteo.com/en/docs/historical-weather-api), caches the results locally as JSON, and displays them in a sortable, filterable UI.

---

## Project Structure

```
WeatherApp/
├── WeatherApp.sln
├── README.md
├── AI_NOTES.md
├── WeatherApp.Api/              # ASP.NET Core .NET 8 backend
│   ├── Controllers/
│   │   └── WeatherController.cs
│   ├── Models/
│   │   ├── WeatherEntry.cs      # API response model
│   │   ├── OpenMeteoResponse.cs # Deserialization model
│   │   └── WeatherSettings.cs   # Typed config
│   ├── Services/
│   │   ├── IDateParserService.cs
│   │   ├── DateParserService.cs # Multi-format date parsing
│   │   ├── IWeatherService.cs
│   │   └── WeatherService.cs   # Fetch + cache orchestration
│   ├── dates.txt               # Input dates (various formats)
│   ├── weather-data/           # Created at runtime; JSON cache per date
│   ├── Program.cs
│   └── appsettings.json
└── WeatherApp.Client/          # React + Vite frontend
    ├── src/
    │   ├── App.jsx
    │   ├── components/
    │   │   ├── WeatherTable.jsx
    │   │   └── DetailPanel.jsx
    │   └── index.css / App.css
    ├── vite.config.js
    └── package.json
```

---

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0 or later |
| [Node.js](https://nodejs.org/) | 18 or later |
| npm | 9 or later (bundled with Node) |

---

## Running the Backend

```bash
cd WeatherApp.Api
dotnet run
```

The API starts on **http://localhost:5000** (HTTP) by default.

- Swagger UI: http://localhost:5000/swagger
- Weather endpoint: http://localhost:5000/api/weather

On first run the service reads `dates.txt`, calls Open-Meteo for each valid date, and writes results to `weather-data/`. Subsequent runs use the cached JSON files and skip the API calls.

### Configuration

All settings live in `appsettings.json` under the `WeatherSettings` key — no secrets are required and none are hardcoded.

| Key | Default | Description |
|-----|---------|-------------|
| `Latitude` | 32.78 | Dallas, TX latitude |
| `Longitude` | -96.8 | Dallas, TX longitude |
| `Timezone` | America/Chicago | IANA timezone for Open-Meteo |
| `OpenMeteoBaseUrl` | https://archive-api.open-meteo.com/v1/archive | API base URL |
| `WeatherDataFolder` | weather-data | Folder for cached JSON files |
| `DatesFile` | dates.txt | Input dates file |

---

## Running the Frontend

In a second terminal:

```bash
cd WeatherApp.Client
npm install
npm run dev
```

The app opens at **http://localhost:5173**.

Vite proxies all `/api` requests to `http://localhost:5000`, so no extra CORS configuration is needed during development.

---

## Features

- **Date parsing** — handles `MM/dd/yyyy`, `MMMM d, yyyy`, and `MMM-dd-yyyy` formats; logically invalid dates (e.g. April 31) are rejected gracefully.
- **Caching** — each fetched date is saved as `weather-data/{date}.json`; no repeat API calls on reload.
- **Error handling** — invalid dates and API failures are surfaced as entries with `isSuccess: false`; the app never crashes.
- **Sortable table** — click any column header to sort ascending/descending.
- **Temperature filter** — filter rows to only show dates where the minimum temperature is at or above a threshold.
- **Detail panel** — click any row to open a slide-in panel with full detail; close with Escape or the ✕ button.
- **Loading & error states** — UI shows a spinner while fetching and a clear error banner if the API call fails.

---

## Assumptions

1. All dates in `dates.txt` are for **Dallas, TX** only — location is not per-date.
2. Open-Meteo temperatures are returned in **°C** (the API default). No Fahrenheit conversion is applied in the backend — the UI displays °C directly.
3. "Invalid" means either unparseable format *or* a logically impossible calendar date (e.g. April 31). Both cases are recorded with a descriptive error and excluded from API calls.
4. The `weather-data/` folder is created automatically next to the compiled binary on first run.
5. Caching is file-based (existence check only) — suitable for this exercise. A production system would use a database or distributed cache.
