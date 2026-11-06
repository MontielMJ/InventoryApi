import { createContext } from 'react';

export interface AuthContextValue {
  authenticated: boolean;
  loading: boolean;
  username?: string;
  login: () => void;
  logout: () => void;
  getToken: () => Promise<string | undefined>;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);
