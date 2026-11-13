import { useEffect, useRef } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Order } from './api';

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5005').replace(/\/$/, '');
export function useOrderUpdates(onOrderChanged: (orderId: number, status: string) => void, enabled: boolean) {
  const onOrderChangedRef = useRef(onOrderChanged);
  onOrderChangedRef.current = onOrderChanged;

  useEffect(() => {
    if (!enabled || !localStorage.getItem('ecommerce-token')) return;

    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/orders`, { accessTokenFactory: () => localStorage.getItem('ecommerce-token') ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();
    const handler = (payload: string | Pick<Order, 'id' | 'status'>) => {
      try {
        const update = typeof payload === 'string' ? JSON.parse(payload) as { orderId: number; status: string } : payload;
        onOrderChangedRef.current('orderId' in update ? update.orderId : update.id, update.status);
      } catch {
        return;
      }
    };
    connection.on('OrderStatusChanged', handler);
    void connection.start().catch(() => undefined);
    return () => {
      connection.off('OrderStatusChanged', handler);
      void connection.stop();
    };
  }, [enabled]);
}
