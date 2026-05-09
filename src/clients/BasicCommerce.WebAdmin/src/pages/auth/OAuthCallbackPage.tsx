import { useEffect, useRef } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Loader2 } from 'lucide-react';
import { useAuthStore } from '../../store/authStore';
import type { UserDto } from '../../types/auth';

export default function OAuthCallbackPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  const processed = useRef(false);

  useEffect(() => {
    if (processed.current) return;
    processed.current = true;

    const accessToken  = searchParams.get('accessToken');
    const refreshToken = searchParams.get('refreshToken');
    const expiresAt    = searchParams.get('expiresAt');
    const userRaw      = searchParams.get('user');

    if (!accessToken || !refreshToken || !expiresAt || !userRaw) {
      navigate('/login?error=OAuth+callback+missing+parameters', { replace: true });
      return;
    }

    try {
      const user: UserDto = JSON.parse(decodeURIComponent(userRaw));
      setAuth(accessToken, refreshToken, expiresAt, user);
      navigate('/', { replace: true });
    } catch {
      navigate('/login?error=Failed+to+process+OAuth+response', { replace: true });
    }
  }, []);

  return (
    <div className="min-h-screen bg-gradient-to-br from-primary-900 via-primary-800 to-primary-700 flex items-center justify-center">
      <div className="flex flex-col items-center gap-4 text-white">
        <Loader2 className="w-10 h-10 animate-spin" />
        <p className="text-lg font-medium">Completing sign in…</p>
        <p className="text-sm text-primary-200">Please wait</p>
      </div>
    </div>
  );
}
