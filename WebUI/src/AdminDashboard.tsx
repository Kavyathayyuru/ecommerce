import { FormEvent, useCallback, useEffect, useState } from 'react';
import { api, Order, Payment, Product, ServiceHealth, User } from './api';
import { useOrderUpdates } from './useOrderUpdates';

const emptyProduct = { name: '', description: '', price: '', stock: '' };

export function AdminDashboard({ user, onSignOut }: { user: User; onSignOut: () => void }) {
  const [products, setProducts] = useState<Product[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [payments, setPayments] = useState<Payment[]>([]);
  const [services, setServices] = useState<ServiceHealth[]>([]);
  const [newProduct, setNewProduct] = useState(emptyProduct);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [busy, setBusy] = useState(false);

  useOrderUpdates((orderId, status) => {
    setOrders((current) => current.map((order) => order.id === orderId ? { ...order, status } : order));
    setNotice(`Order #${orderId} status changed to ${status}.`);
  }, true);

  const refresh = useCallback(async () => {
    setError('');
    const results = await Promise.allSettled([
      api.getProducts(), api.getOrders(), api.getPayments(), api.getServiceHealth(),
    ]);
    const [productResult, orderResult, paymentResult, serviceResult] = results;
    if (productResult.status === 'fulfilled') setProducts(productResult.value);
    if (orderResult.status === 'fulfilled') setOrders(orderResult.value);
    if (paymentResult.status === 'fulfilled') setPayments(paymentResult.value);
    if (serviceResult.status === 'fulfilled') setServices(serviceResult.value);
    const failures = results.filter((result) => result.status === 'rejected') as PromiseRejectedResult[];
    if (failures.length) setError(failures.map((failure) => (failure.reason as Error).message).join(' '));
  }, []);

  useEffect(() => {
    void refresh();
    const interval = window.setInterval(() => void refresh(), 30000);
    return () => window.clearInterval(interval);
  }, [refresh]);

  async function createProduct(event: FormEvent) {
    event.preventDefault(); setBusy(true); setError('');
    try {
      await api.createProduct({ name: newProduct.name, description: newProduct.description, price: Number(newProduct.price), stock: Number(newProduct.stock) });
      setNewProduct(emptyProduct); setNotice('Product added.'); await refresh();
    } catch (reason) { setError((reason as Error).message); }
    finally { setBusy(false); }
  }

  function updateDraft(id: number, patch: Partial<Product>) {
    setProducts((current) => current.map((product) => product.id === id ? { ...product, ...patch } : product));
  }

  async function saveProduct(product: Product) {
    setBusy(true); setError('');
    try { await api.updateProduct(product); setNotice(`${product.name} updated.`); await refresh(); }
    catch (reason) { setError((reason as Error).message); }
    finally { setBusy(false); }
  }

  async function deleteProduct(product: Product) {
    if (!window.confirm(`Delete ${product.name}?`)) return;
    setBusy(true); setError('');
    try { await api.deleteProduct(product.id); setNotice(`${product.name} deleted.`); await refresh(); }
    catch (reason) { setError((reason as Error).message); }
    finally { setBusy(false); }
  }

  async function updateOrder(order: Order, status: string) {
    setBusy(true); setError('');
    try { await api.updateOrderStatus(order.id, status); setNotice(`Order #${order.id} updated.`); await refresh(); }
    catch (reason) { setError((reason as Error).message); }
    finally { setBusy(false); }
  }

  return <div className="admin-shell">
    <header className="admin-header"><a className="brand" href="#admin">Shop<span>Sphere</span> <small>Admin</small></a><div><span>Signed in as <strong>{user.name}</strong></span><button className="link-button" onClick={onSignOut}>Sign out</button></div></header>
    <main id="admin" className="admin-content">
      <section className="admin-welcome"><div><p className="eyebrow">Operations overview</p><h1>Store dashboard</h1><p>Monitor services, manage inventory, and review orders and simulated payments.</p></div><button className="small-button" onClick={() => void refresh()}>Refresh data</button></section>
      {error && <div className="alert error">{error}</div>}{notice && <div className="alert success">{notice}</div>}
      <section className="admin-section"><div className="section-heading"><div><p className="eyebrow">Platform</p><h2>Service health</h2></div><span>Auto-refreshes every 30 seconds</span></div><div className="health-grid">{services.map((service) => <article className="health-card" key={service.name}><span className={`health-dot ${service.status}`} /><div><strong>{service.name}</strong><small>{service.url}</small></div><b className={service.status}>{service.status}</b></article>)}</div></section>
      <section className="admin-section"><div className="section-heading"><div><p className="eyebrow">Catalog</p><h2>Products & inventory</h2></div><span>{products.length} products</span></div>
        <form className="product-create" onSubmit={createProduct}><input required placeholder="Product name" value={newProduct.name} onChange={(event) => setNewProduct({ ...newProduct, name: event.target.value })} /><input placeholder="Description" value={newProduct.description} onChange={(event) => setNewProduct({ ...newProduct, description: event.target.value })} /><input required min="0" step="0.01" type="number" placeholder="Price" value={newProduct.price} onChange={(event) => setNewProduct({ ...newProduct, price: event.target.value })} /><input required min="0" step="1" type="number" placeholder="Stock" value={newProduct.stock} onChange={(event) => setNewProduct({ ...newProduct, stock: event.target.value })} /><button className="small-button" disabled={busy}>Add product</button></form>
        <div className="table-wrap"><table><thead><tr><th>Product</th><th>Description</th><th>Price</th><th>Stock</th><th>Actions</th></tr></thead><tbody>{products.map((product) => <tr key={product.id}><td><input aria-label="Product name" value={product.name} onChange={(event) => updateDraft(product.id, { name: event.target.value })} /></td><td><input aria-label="Product description" value={product.description ?? ''} onChange={(event) => updateDraft(product.id, { description: event.target.value })} /></td><td><input aria-label="Product price" min="0" step="0.01" type="number" value={product.price} onChange={(event) => updateDraft(product.id, { price: Number(event.target.value) })} /></td><td><input aria-label="Product stock" min="0" step="1" type="number" value={product.stock} onChange={(event) => updateDraft(product.id, { stock: Number(event.target.value) })} /></td><td className="action-cell"><button className="small-button" disabled={busy} onClick={() => void saveProduct(product)}>Save</button><button className="link-button danger-link" disabled={busy} onClick={() => void deleteProduct(product)}>Delete</button></td></tr>)}</tbody></table></div>
      </section>
      <section className="admin-section"><div className="section-heading"><div><p className="eyebrow">Fulfillment</p><h2>Orders</h2></div><span>{orders.length} orders</span></div><div className="table-wrap"><table><thead><tr><th>Order</th><th>Customer</th><th>Product</th><th>Qty</th><th>Total</th><th>Status</th><th>Control</th></tr></thead><tbody>{orders.map((order) => <tr key={order.id}><td>#{order.id}</td><td>{order.userId}</td><td>{order.productId}</td><td>{order.quantity}</td><td>${order.totalPrice.toFixed(2)}</td><td><span className="status-pill">{order.status}</span></td><td><select value={order.status} disabled={busy} onChange={(event) => void updateOrder(order, event.target.value)}><option>Pending</option><option>Processing</option><option>Completed</option><option>Cancelled</option></select></td></tr>)}</tbody></table></div></section>
      <section className="admin-section"><div className="section-heading"><div><p className="eyebrow">Payments</p><h2>Simulated payment records</h2></div><span>{payments.length} payments</span></div><div className="table-wrap"><table><thead><tr><th>Payment</th><th>Order</th><th>Method</th><th>Amount</th><th>Status</th></tr></thead><tbody>{payments.map((payment) => <tr key={payment.id}><td>#{payment.id}</td><td>#{payment.orderId}</td><td>{payment.paymentMethod}</td><td>${payment.amount.toFixed(2)}</td><td><span className="status-pill">{payment.status}</span></td></tr>)}</tbody></table></div></section>
    </main>
  </div>;
}
