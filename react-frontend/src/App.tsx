import { Link, NavLink, Outlet, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from './auth/ProtectedRoute';

function AppLayout() {
  return (
    <div className="app-shell">
      <header className="site-header">
        <Link className="brand" to="/">
          Backoffice Service Portal
        </Link>
        <nav className="site-nav" aria-label="Primary navigation">
          <NavLink to="/" end>
            Dashboard
          </NavLink>
          <NavLink to="/service-requests">Service Requests</NavLink>
          <NavLink to="/login">Login</NavLink>
        </nav>
      </header>
      <main className="page-content">
        <Outlet />
      </main>
    </div>
  );
}

function DashboardPage() {
  return (
    <section className="placeholder">
      <h1>Dashboard</h1>
      <p>
        This placeholder dashboard will become the main workspace for the
        Backoffice Service Portal.
      </p>
      <p className="integration-note">
        It will later integrate with the existing ASP.NET Core backend.
      </p>
    </section>
  );
}

function ServiceRequestsPage() {
  return (
    <section className="placeholder">
      <h1>Service Requests</h1>
      <p>
        This placeholder page will later show and manage service request
        activity.
      </p>
      <p className="integration-note">
        Backend API integration will be added in a later phase.
      </p>
    </section>
  );
}

function LoginPage() {
  return (
    <section className="placeholder">
      <h1>Login</h1>
      <p>
        This placeholder login page is reserved for future sign-in experience
        work.
      </p>
      <p className="integration-note">
        Authentication API integration and JWT handling are not implemented yet.
      </p>
    </section>
  );
}

export default function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route element={<ProtectedRoute />}>
          <Route index element={<DashboardPage />} />
          <Route path="service-requests" element={<ServiceRequestsPage />} />
        </Route>
        <Route path="login" element={<LoginPage />} />
      </Route>
    </Routes>
  );
}
