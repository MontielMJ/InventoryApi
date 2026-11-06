export type MovementType = 'In' | 'Out';

export interface Category {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Product {
  id: string;
  sku: string;
  name: string;
  description?: string;
  categoryId: string;
  categoryName: string;
  price: number;
  stock: number;
  minimumStock: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Movement {
  id: string;
  productId: string;
  productName: string;
  type: MovementType;
  quantity: number;
  unitCost: number;
  reason?: string;
  reference?: string;
  createdBy: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ProductInput {
  sku: string;
  name: string;
  description?: string;
  categoryId: string;
  price: number;
  minimumStock: number;
}

export interface CategoryInput {
  name: string;
  description?: string;
}

export interface MovementInput {
  productId: string;
  type: MovementType;
  quantity: number;
  unitCost: number;
  reason?: string;
  reference?: string;
}
