import { describe, expect, it } from 'vitest';
import { config } from './config';

describe('frontend configuration', () => {
  it('uses the local API and Keycloak defaults', () => {
    expect(config.apiUrl).toBe('http://localhost:8081');
    expect(config.keycloakRealm).toBe('inventory');
    expect(config.keycloakClientId).toBe('inventory-frontend');
  });
});
