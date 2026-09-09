
import axiosClient from './axiosClient';


export interface AuthResponse {
  token: string;
  email: string;
}

export const registerUser = async (email: string, password: string): Promise<AuthResponse> => {
  const response = await axiosClient.post<AuthResponse>('/auth/register', { email, password });
  return response.data;
};

export const loginUser = async (email: string, password: string): Promise<AuthResponse> => {
  const response = await axiosClient.post<AuthResponse>('/auth/login', { email, password });
  return response.data;
};