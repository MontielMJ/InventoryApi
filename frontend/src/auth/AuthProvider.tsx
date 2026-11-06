import Keycloak from 'keycloak-js';
import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { config } from '../config';
import { AuthContext } from './context';

const keycloak = new Keycloak({
  url: config.keycloakUrl,
  realm: config.keycloakRealm,
  clientId: config.keycloakClientId
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [authenticated, setAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    keycloak
      .init({ onLoad: 'check-sso', pkceMethod: 'S256', checkLoginIframe: false })
      .then(setAuthenticated)
      .catch(() => setAuthenticated(false))
      .finally(() => setLoading(false));
  }, []);

  const value = useMemo(() => ({
    authenticated,
    loading,
    username: keycloak.tokenParsed?.preferred_username,
    login: () => keycloak.login({ redirectUri: window.location.origin }),
    logout: () => keycloak.logout({ redirectUri: window.location.origin }),
    getToken: async () => {
      if (!keycloak.authenticated) {
        return undefined;
      }
      await keycloak.updateToken(30);
      return keycloak.token;
    }
  }), [authenticated, loading]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
