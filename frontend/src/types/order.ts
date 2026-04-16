export interface OrderRequest {
  symbol: string;
  side: string;
  quantity: number;
  price: number;
}

export interface OrderResponse {
  orderId: string;
  execId: string;
  clOrdId: string;
  symbol: string;
  side: string;
  quantity: number;
  price: number;
  status: string;
  message?: string;
}

export interface ApiErrorResponse {
  error: string;
  details?: string[];
}
