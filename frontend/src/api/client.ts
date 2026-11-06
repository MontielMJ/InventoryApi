import axios from 'axios';
import { config } from '../config';
import type { AuthContextValue } from '../auth/context';

export function createApiClient(getToken: AuthContextValue['getToken']) {
  const client = axios.create({ baseURL: config.apiUrl, headers: { 'Content-Type': 'application/json' } });
  client.interceptors.request.use(async request => {
    const token = await getToken();
    if (token) {
      request.headers.Authorization = `Bearer ${token}`;
    }
    return request;
  });
  return client;
}
