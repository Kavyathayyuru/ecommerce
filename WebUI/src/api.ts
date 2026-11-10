const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5005').replace(/\/$/, '');

export type Product = {
  id: number;
  name: string;
  description: string;
  price: number;
  stock: number;
};

export type User = {
  id: number;
  name: string;
  email: string;
};

export type Order = {
  id: number;
  userId: number;
  productId: number;
  quantity: number;
  totalPrice: number;
  status: string;
};

export type Payment = {
  id: number;
  orderId: number;
  amount: number;
  paymentMethod: string;
  status: string;
};

export type Checkout = {
  order: Order;
  payment: Payment;
};

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json', ...(options?.headers ?? {}) },
    ...options,
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed with status ${response.status}`);
  }

  return response.json() as Promise<T>;
}

export const api = {
  getProducts: () => request<Product[]>('/api/product/api/Products'),
  getOrders: (userId: number) => request<Order[]>('/api/order/api/Orders').then((orders) => orders.filter((order) => order.userId === userId)),
  login: (email: string, password: string) => request<User>('/api/user/api/Users/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  }),
  register: (name: string, email: string, password: string) => request<User>('/api/user/api/Users', {
    method: 'POST',
    body: JSON.stringify({ name, email, password }),
  }),
  checkout: (userId: number, productId: number, quantity: number, paymentMethod: string) => request<Checkout>('/api/order/api/Orders/checkout', {
    method: 'POST',
    body: JSON.stringify({ userId, productId, quantity, paymentMethod }),
  }),
};
