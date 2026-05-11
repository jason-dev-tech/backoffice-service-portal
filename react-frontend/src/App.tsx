import { useState, type FormEvent } from 'react';
import {
  Link,
  NavLink,
  Outlet,
  Route,
  Routes,
  useNavigate,
} from 'react-router-dom';
import { authService } from './api/authService';
import { useAuth } from './auth/AuthContext';
import { ProtectedRoute } from './auth/ProtectedRoute';

function AppLayout() {
  const navigate = useNavigate();
  const { isAuthenticated, logout } = useAuth();

  function handleLogout() {
    logout();
    navigate('/login');
  }

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
          {isAuthenticated ? (
            <button className="logout-button" onClick={handleLogout} type="button">
              Logout
            </button>
          ) : null}
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
  const navigate = useNavigate();
  const { setAuthState } = useAuth();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setErrorMessage(null);

    try {
      const authState = await authService.login({ username, password });
      setAuthState(authState);
      navigate('/');
    } catch {
      setErrorMessage('Unable to sign in. Check your username and password.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="placeholder">
      <h1>Login</h1>
      <form className="login-form" onSubmit={handleSubmit}>
        {errorMessage ? <p className="form-error">{errorMessage}</p> : null}
        <label className="form-field">
          <span>Username</span>
          <input
            autoComplete="username"
            disabled={isSubmitting}
            name="username"
            onChange={(event) => setUsername(event.target.value)}
            type="text"
            value={username}
          />
        </label>
        <label className="form-field">
          <span>Password</span>
          <input
            autoComplete="current-password"
            disabled={isSubmitting}
            name="password"
            onChange={(event) => setPassword(event.target.value)}
            type="password"
            value={password}
          />
        </label>
        <button disabled={isSubmitting} type="submit">
          {isSubmitting ? 'Signing in...' : 'Sign in'}
        </button>
      </form>
      <p className="integration-note">
        Enter your username and password to sign in.
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
