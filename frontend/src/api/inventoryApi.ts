import type { AxiosInstance } from 'axios';
import type { Category, CategoryInput, Movement, MovementInput, PagedResult, Product, ProductInput } from '../types';

export function inventoryApi(client: AxiosInstance) {
  return {
    products: {
      list: (search = '') => client.get<PagedResult<Product>>('/api/products', { params: { search, pageSize: 100 } }).then(response => response.data),
      create: (input: ProductInput) => client.post<string>('/api/products', input).then(response => response.data),
      update: (id: string, input: ProductInput) => client.put(`/api/products/${id}`, { id, ...input }),
      remove: (id: string) => client.delete(`/api/products/${id}`)
    },
    categories: {
      list: () => client.get<Category[]>('/api/categories').then(response => response.data),
      create: (input: CategoryInput) => client.post<string>('/api/categories', input).then(response => response.data),
      update: (id: string, input: CategoryInput) => client.put(`/api/categories/${id}`, { id, ...input }),
      remove: (id: string) => client.delete(`/api/categories/${id}`)
    },
    movements: {
      list: () => client.get<PagedResult<Movement>>('/api/inventory-movements', { params: { pageSize: 100 } }).then(response => response.data),
      create: (input: MovementInput) => client.post<string>('/api/inventory-movements', input).then(response => response.data)
    }
  };
}
