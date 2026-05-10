# React Frontend

Minimal React + TypeScript frontend skeleton for the Backoffice Service Portal.

This project is intentionally small and currently renders placeholder pages only. It does not include authentication, API calls, JWT handling, deployment logic, or security logic.

## Local Setup

```bash
cd react-frontend
npm install
npm run dev
```

## API Base URL

The frontend reads the backend API base URL from `VITE_API_BASE_URL`.

Create a local `.env` file when you need to override the default:

```bash
VITE_API_BASE_URL=http://localhost:8080
```

If `VITE_API_BASE_URL` is not set, the app falls back to `http://localhost:8080`.

`VITE_API_BASE_URL` is public frontend configuration, not a secret. Secrets must remain in the backend or deployment environment.

Build for a local production check:

```bash
npm run build
```

Preview a production build locally:

```bash
npm run preview
```

## Dependencies & Technologies Used

- Vite
- React
- React Router
- TypeScript
- ASP.NET Core backend integration planned for a later phase
