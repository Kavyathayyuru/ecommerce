import { FormEvent, useEffect, useMemo, useState } from 'react';
import { api, Checkout, Order, Product, User } from './api';

type CartItem = { product: Product; quantity: number };

export function App() {
  const [products, setProducts] = useState<Product[]>([]);
  const [user, setUser] = useState<User | null>(() => JSON.parse(localStorage.getItem('ecommerce-user') ?? 'null'));
  const [orders, setOrders] = useState<Order[]>([]);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [showCart, setShowCart] = useState(false);

  useEffect(() => {
    api.getProducts().then(setProducts).catch((reason: Error) => setError(reason.message));
  }, []);

  useEffect(() => {
    if (user) {
      api.getOrders(user.id).then(setOrders).catch((reason: Error) => setError(reason.message));
      localStorage.setItem('ecommerce-user', JSON.stringify(user));
    } else {
      localStorage.removeItem('ecommerce-user');
      setOrders([]);
    }
  }, [user]);

  const cartTotal = useMemo(() => cart.reduce((total, item) => total + item.product.price * item.quantity, 0), [cart]);

  function addToCart(product: Product) {
    setCart((current) => {
      const existing = current.find((item) => item.product.id === product.id);
      if (existing) {
        return current.map((item) => item.product.id === product.id ? { ...item, quantity: Math.min(item.quantity + 1, product.stock) } : item);
      }
      return [...current, { product, quantity: 1 }];
    });
    setNotice(`${product.name} added to your cart.`);
  }

  function signOut() {
    setUser(null);
    setCart([]);
    setNotice('You have been signed out.');
  }

  return (
    <>
      <header className="site-header">
        <div className="header-inner">
          <a className="brand" href="#top">Shop<span>Sphere</span></a>
          <nav>
            <a href="#catalog">Catalog</a>
            {user && <a href="#orders">My orders</a>}
            <button className="cart-button" onClick={() => setShowCart(true)}>Cart ({cart.length})</button>
            {user ? <button className="link-button" onClick={signOut}>Sign out</button> : <a href="#account">Sign in</a>}
          </nav>
        </div>
      </header>

      <main id="top" className="app-shell">
        <section className="hero">
          <div>
            <p className="eyebrow">A simple microservices storefront</p>
            <h1>Good products.<br /><em>Simple checkout.</em></h1>
            <p className="hero-copy">Browse our catalog, add your favorites, and place a simulated order powered by the ecommerce services.</p>
            <a className="primary-button" href="#catalog">Explore products</a>
          </div>
          <div className="hero-card"><span>Featured today</span><strong>Build your<br />everyday kit.</strong><small>Reliable essentials, delivered simply.</small></div>
        </section>

        {error && <div className="alert error">{error}</div>}
        {notice && <div className="alert success">{notice}</div>}

        <section id="catalog" className="section">
          <div className="section-heading"><div><p className="eyebrow">The collection</p><h2>Popular products</h2></div><span>{products.length} products</span></div>
          <div className="product-grid">
            {products.map((product) => <article className="product-card" key={product.id}>
              <div className="product-art">{product.name.slice(0, 1)}</div>
              <div className="product-info"><span className="stock">{product.stock > 0 ? `${product.stock} in stock` : 'Out of stock'}</span><h3>{product.name}</h3><p>{product.description}</p><div className="product-footer"><strong>${product.price.toFixed(2)}</strong><button className="small-button" disabled={!product.stock} onClick={() => addToCart(product)}>Add to cart</button></div></div>
            </article>)}
          </div>
        </section>

        {!user && <AuthPanel mode={authMode} setMode={setAuthMode} onAuthenticated={setUser} />}
        {user && <OrdersPanel orders={orders} />}
      </main>

      {showCart && <CartPanel cart={cart} total={cartTotal} user={user} onClose={() => setShowCart(false)} onRemove={(id) => setCart((current) => current.filter((item) => item.product.id !== id))} onCheckout={async (paymentMethod) => {
        if (!user) { setShowCart(false); document.getElementById('account')?.scrollIntoView(); setNotice('Sign in before checking out.'); return; }
        try {
          const results: Checkout[] = [];
          for (const item of cart) results.push(await api.checkout(user.id, item.product.id, item.quantity, paymentMethod));
          setOrders((current) => [...results.map((result) => result.order), ...current]);
          setCart([]); setShowCart(false); setNotice(`Payment completed for ${results.length} order${results.length === 1 ? '' : 's'}.`);
        } catch (reason) { setError((reason as Error).message); }
      }} />}
    </>
  );
}

function AuthPanel({ mode, setMode, onAuthenticated }: { mode: 'login' | 'register'; setMode: (mode: 'login' | 'register') => void; onAuthenticated: (user: User) => void }) {
  const [name, setName] = useState(''); const [email, setEmail] = useState(''); const [password, setPassword] = useState(''); const [error, setError] = useState('');
  async function submit(event: FormEvent) { event.preventDefault(); setError(''); try { const result = mode === 'login' ? await api.login(email, password) : await api.register(name, email, password); onAuthenticated(result); } catch (reason) { setError((reason as Error).message); } }
  return <section id="account" className="account section"><div><p className="eyebrow">Your account</p><h2>{mode === 'login' ? 'Welcome back.' : 'Join the collection.'}</h2><p>Sign in to checkout and view your order history.</p></div><form onSubmit={submit} className="auth-form">{mode === 'register' && <input required placeholder="Name" value={name} onChange={(event) => setName(event.target.value)} />}<input required type="email" placeholder="Email" value={email} onChange={(event) => setEmail(event.target.value)} /><input required minLength={8} type="password" placeholder="Password (8+ characters)" value={password} onChange={(event) => setPassword(event.target.value)} />{error && <small className="form-error">{error}</small>}<button className="primary-button" type="submit">{mode === 'login' ? 'Sign in' : 'Create account'}</button><button type="button" className="link-button" onClick={() => setMode(mode === 'login' ? 'register' : 'login')}>{mode === 'login' ? 'Need an account?' : 'Already have an account?'}</button></form></section>;
}

function OrdersPanel({ orders }: { orders: Order[] }) { return <section id="orders" className="section"><div className="section-heading"><div><p className="eyebrow">Your account</p><h2>Recent orders</h2></div></div>{orders.length === 0 ? <p className="muted">Your completed orders will appear here.</p> : <div className="orders-list">{orders.map((order) => <div className="order-row" key={order.id}><span>Order #{order.id}</span><span>{order.quantity} item{order.quantity === 1 ? '' : 's'}</span><strong>${order.totalPrice.toFixed(2)}</strong><b>{order.status}</b></div>)}</div>}</section>; }

function CartPanel({ cart, total, user, onClose, onRemove, onCheckout }: { cart: CartItem[]; total: number; user: User | null; onClose: () => void; onRemove: (id: number) => void; onCheckout: (paymentMethod: string) => void }) {
  const [paymentMethod, setPaymentMethod] = useState('Demo card');
  return <div className="modal-backdrop" onClick={onClose}><aside className="cart-panel" onClick={(event) => event.stopPropagation()}><div className="panel-heading"><h2>Your cart</h2><button className="close-button" onClick={onClose}>×</button></div>{cart.length === 0 ? <p className="muted">Your cart is empty.</p> : <>{cart.map((item) => <div className="cart-row" key={item.product.id}><div><strong>{item.product.name}</strong><span>{item.quantity} × ${item.product.price.toFixed(2)}</span></div><button className="link-button" onClick={() => onRemove(item.product.id)}>Remove</button></div>)}<div className="cart-total"><span>Total</span><strong>${total.toFixed(2)}</strong></div><select value={paymentMethod} onChange={(event) => setPaymentMethod(event.target.value)}><option>Demo card</option><option>Demo bank transfer</option><option>Demo cash on delivery</option></select><button className="primary-button full-width" disabled={!user} onClick={() => onCheckout(paymentMethod)}>{user ? 'Complete simulated payment' : 'Sign in to checkout'}</button></>}</aside></div>;
}
