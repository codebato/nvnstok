import axiosClient from './axiosClient';

export interface Product {
  id: string;
  sku: string;
  name: string;
  currentStock: number;
  lowStockThreshold: number;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  initialStock: number;
  lowStockThreshold: number;
}

export const getAllProducts = async (): Promise<Product[]> => {
  const response = await axiosClient.get<Product[]>('/products');
  return response.data;
};

export const createProduct = async (data: CreateProductRequest): Promise<void> => {
  await axiosClient.post('/products', data);
};