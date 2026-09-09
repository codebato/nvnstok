import axiosClient from './axiosClient';

export interface AskResponse {
  question: string;
  answer: string;
}

export const askAssistant = async (question: string): Promise<AskResponse> => {
  const response = await axiosClient.post<AskResponse>('/assistant/ask', { question });
  return response.data;
};