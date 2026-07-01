import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import './LoginPage.css';

/** Username/password login screen. */
export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setErrorMessage(null);
    setIsSubmitting(true);
    try {
      await login(username, password);
      navigate('/', { replace: true });
    } catch {
      setErrorMessage('Invalid username or password.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="login-page">
      <form className="login-card" onSubmit={handleSubmit}>
        <h1 className="login-title">
          AID <span>Dashboard</span>
        </h1>
        <p className="login-subtitle">Sign in to continue</p>

        <label className="login-label" htmlFor="username">Username</label>
        <input
          id="username"
          autoFocus
          value={username}
          onChange={(event) => setUsername(event.target.value)}
          autoComplete="username"
        />

        <label className="login-label" htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          autoComplete="current-password"
        />

        {errorMessage && <div className="login-error">{errorMessage}</div>}

        <button type="submit" className="btn-primary login-submit" disabled={isSubmitting}>
          {isSubmitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </div>
  );
}
