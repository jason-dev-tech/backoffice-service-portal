# React Frontend

Minimal React + TypeScript frontend foundation for the Backoffice Service Portal.

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

Frontend environment variables are bundled into browser-delivered code when they are referenced by the app. Use them only for public configuration such as API base URLs, never passwords, API keys, tokens, or private credentials.

## Local Full-Stack Verification

For local React and backend integration, create a local-only `.env` from `.env.example` and set:

```bash
VITE_API_BASE_URL=http://localhost:8080
```

`.env.example` is only a template. Do not commit `.env`.

The ASP.NET Core backend may require local User Secrets for values such as:

```bash
ConnectionStrings:DefaultConnection=<LOCAL_CONNECTION_STRING>
Jwt:Key=<LOCAL_JWT_SIGNING_KEY>
BootstrapAdmin:Password=<LOCAL_ADMIN_PASSWORD>
AllowedOrigins:0=http://localhost:5173
```

Use local placeholder values only in notes and scripts. The Vite dev origin, usually `http://localhost:5173`, must be allowed by backend CORS for browser-based integration.

Verification checklist:

- Backend starts successfully.
- Frontend starts successfully.
- Login succeeds.
- Protected route loads.
- Service requests list loads.
- Create service request works.

## Authentication State

This demo phase stores only `accessToken` and `expiresAtUtc` in `localStorage`. Production systems may prefer stronger token handling, such as HttpOnly Secure SameSite cookies, with secrets kept in the backend or deployment environment.

## Frontend Security & Deployment Notes

Frontend JavaScript, HTML, CSS, and embedded configuration are visible to users in the browser. Client-side checks can improve the user experience, but real authorization and sensitive decisions must remain enforced by the backend.

Production source maps are explicitly disabled in `vite.config.ts` to avoid publishing expanded source context with production assets. Keep source maps internal if they are needed for production diagnostics.

## Azure Static Web Apps Preparation

The React frontend can be deployed independently from the ASP.NET Core backend, for example to Azure Static Web Apps at `<AZURE_STATIC_WEB_APP_URL>`.

Set the frontend build configuration so `VITE_API_BASE_URL=<BACKEND_API_BASE_URL>` points to the deployed backend API. Frontend environment variables are public build-time configuration and must not contain secrets.

The backend CORS configuration must allow the Azure frontend origin, such as `<AZURE_STATIC_WEB_APP_URL>`. Production source maps are disabled in `vite.config.ts`.

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
