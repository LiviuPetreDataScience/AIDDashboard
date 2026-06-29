import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

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
    <div style={styles.page}>
      <form style={styles.card} onSubmit={handleSubmit}>
        <h1 style={styles.title}>
          AID <span style={{ color: 'var(--color-accent)' }}>Dashboard</span>
        </h1>
        <p style={styles.subtitle}>Sign in to continue</p>

        <label style={styles.label} htmlFor="username">Username</label>
        <input
          id="username"
          autoFocus
          value={username}
          onChange={(event) => setUsername(event.target.value)}
          autoComplete="username"
        />

        <label style={styles.label} htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          autoComplete="current-password"
        />

        {errorMessage && <div style={styles.error}>{errorMessage}</div>}

        <button type="submit" className="btn-primary" style={styles.submit} disabled={isSubmitting}>
          {isSubmitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  page: {
    minHeight: '100vh',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    background: 'var(--color-primary)',
    padding: '1rem',
  },
  card: {
    width: '100%',
    maxWidth: 360,
    background: 'var(--color-surface)',
    borderRadius: 'var(--radius-md)',
    boxShadow: 'var(--shadow-md)',
    padding: '2rem',
    display: 'flex',
    flexDirection: 'column',
  },
  title: { margin: 0, textAlign: 'center' },
  subtitle: { marginTop: 4, marginBottom: 24, textAlign: 'center', color: 'var(--color-text-muted)' },
  label: { fontWeight: 600, marginBottom: 4, marginTop: 12 },
  error: { color: 'var(--color-danger)', marginTop: 12, fontSize: 13 },
  submit: { marginTop: 20 },
};
