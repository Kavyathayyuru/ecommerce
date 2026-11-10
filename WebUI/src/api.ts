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
  role: 'Customer' | 'Admin';
};

export type LoginResponse = {
  user: User;
  token: string;
  expiresAtUtc: string;
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

export type ServiceHealth = { name: string; status: string; url: string };

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const token = localStorage.getItem('ecommerce-token');
  const headers = new Headers(options?.headers);
  headers.set('Content-Type', 'application/json');
  if (token) headers.set('Authorization', `Bearer ${token}`);
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed with status ${response.status}`);
  }

  if (response.status === 204) return undefined as T;

  return response.json() as Promise<T>;
}

export const api = {
  getProducts: () => request<Product[]>('/api/product/api/Products'),
  getOrders: () => request<Order[]>('/api/order/api/Orders'),
  getPayments: () => request<Payment[]>('/api/payment/api/Payments'),
  getServiceHealth: () => request<ServiceHealth[]>('/api/admin/services'),
  login: (email: string, password: string) => request<LoginResponse>('/api/user/api/Users/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  }),
  register: (name: string, email: string, password: string) => request<User>('/api/user/api/Users', {
    method: 'POST',
    body: JSON.stringify({ name, email, password }),
  }),
  createProduct: (product: Omit<Product, 'id'>) => request<Product>('/api/product/api/Products', { method: 'POST', body: JSON.stringify(product) }),
  updateProduct: (product: Product) => request<Product>(`/api/product/api/Products/${product.id}`, { method: 'PUT', body: JSON.stringify(product) }),
  deleteProduct: (id: number) => request<void>(`/api/product/api/Products/${id}`, { method: 'DELETE' }),
  updateOrderStatus: (id: number, status: string) => request<Order>(`/api/order/api/Orders/${id}/status`, { method: 'PATCH', body: JSON.stringify({ status }) }),
  checkout: (userId: number, productId: number, quantity: number, paymentMethod: string) => request<Checkout>('/api/order/api/Orders/checkout', {
    method: 'POST',
    body: JSON.stringify({ userId, productId, quantity, paymentMethod }),
  }),
};
