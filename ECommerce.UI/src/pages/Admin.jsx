import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api } from "../api";
import { getProductImage, imageFallback } from "../utils/productImage";

function readJson(key, fallback) {
    try {
        const value = JSON.parse(localStorage.getItem(key) || "null");
        return value ?? fallback;
    } catch {
        return fallback;
    }
}

function getSession() {
    const session = readJson("user", null);
    if (!session?.token || session.role) return session;

    try {
        const payload = JSON.parse(atob(session.token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")));
        return {
            ...session,
            role: payload.role || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
        };
    } catch {
        return session;
    }
}

function isAdmin(session) {
    return [session?.role, session?.Role, session?.userRole, session?.user?.role]
        .some((role) => String(role || "").toLowerCase() === "admin");
}

function getOrderItems(order) {
    return order.items ?? order.orderItems ?? order.products ?? [];
}

function getOrderTotal(order) {
    return Number(order.total ?? order.totalAmount ?? order.amount ?? 0);
}

function getOrderDate(order) {
    return order.date ?? order.orderDate ?? order.createdAt;
}

function Admin() {
    const navigate = useNavigate();
    const [session] = useState(getSession);
    const [activeView, setActiveView] = useState("overview");
    const [products, setProducts] = useState([]);
    const [orders, setOrders] = useState(() => readJson("orders", []));
    const [customers, setCustomers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [lastUpdated, setLastUpdated] = useState(null);
    const [actionError, setActionError] = useState("");
    const [editingProduct, setEditingProduct] = useState(null);
    const [productForm, setProductForm] = useState({ name: "", description: "", price: "", stock: "" });
    const authConfig = session?.token ? { headers: { Authorization: `Bearer ${session.token}` } } : {};

    useEffect(() => {
        if (!isAdmin(session)) return;

        const headers = session?.token
            ? { Authorization: `Bearer ${session.token}` }
            : {};

        const loadDashboard = async () => {
            setLoading(true);
            setError("");
            const results = await Promise.allSettled([
                api.get("/Product"),
                api.get("/Order/admin/all", { headers }),
                api.get("/User", { headers }),
            ]);

            const [productResult, orderResult, customerResult] = results;
            const failures = results.filter((result) => result.status === "rejected");

            if (productResult.status === "fulfilled") {
                setProducts(Array.isArray(productResult.value.data) ? productResult.value.data : []);
            }
            if (orderResult.status === "fulfilled") {
                const nextOrders = Array.isArray(orderResult.value.data) ? orderResult.value.data : [];
                setOrders(nextOrders);
                localStorage.setItem("orders", JSON.stringify(nextOrders));
            }
            if (customerResult.status === "fulfilled") {
                setCustomers(Array.isArray(customerResult.value.data) ? customerResult.value.data : []);
            }
            if (failures.length === results.length) {
                setError("The admin data could not be loaded. Check that the API is running and your session is valid.");
            } else if (failures.length > 0) {
                setError("Some admin data is unavailable. The sections that loaded are still shown.");
            }
            setLastUpdated(new Date());
            setLoading(false);
        };

        loadDashboard();
    }, [session]);

    const metrics = useMemo(() => ({
        revenue: orders.reduce((total, order) => total + getOrderTotal(order), 0),
        pending: orders.filter((order) => String(order.status || "").toLowerCase() === "pending").length,
        lowStock: products.filter((product) => Number(product.stock) <= 5).length,
        customers: customers.length || new Set(orders.map((order) => order.userId ?? order.customerId ?? order.email).filter(Boolean)).size,
    }), [customers, orders, products]);

    const customerRows = useMemo(() => {
        if (customers.length) return customers;
        const uniqueCustomers = new Map();
        orders.forEach((order) => {
            const email = order.email ?? order.customerEmail ?? order.customer?.email;
            const key = order.userId ?? order.customerId ?? email;
            if (key && !uniqueCustomers.has(key)) {
                uniqueCustomers.set(key, {
                    id: key,
                    name: order.customerName ?? order.userName ?? order.customer?.username ?? "Customer",
                    email,
                    phone: order.phone ?? order.customerPhone,
                    role: "Customer",
                });
            }
        });
        return [...uniqueCustomers.values()];
    }, [customers, orders]);

    const updateOrderStatus = async (orderId, status) => {
        setActionError("");
        try {
            const response = await api.patch(`/Order/admin/${orderId}/status`, { status }, authConfig);
            setOrders((currentOrders) => currentOrders.map((order) =>
                (order.id ?? order.Id) === orderId ? { ...order, ...response.data, status } : order
            ));
        } catch (requestError) {
            setActionError(requestError.response?.data || "Unable to update order status.");
        }
    };

    const startProductEdit = (product) => {
        setEditingProduct(product.id);
        setProductForm({
            name: product.name || "",
            description: product.description || "",
            price: product.price ?? "",
            stock: product.stock ?? "",
        });
        setActionError("");
    };

    const saveProduct = async (event) => {
        event.preventDefault();
        setActionError("");
        try {
            const response = await api.put(`/Product/${editingProduct}`, {
                name: productForm.name,
                description: productForm.description,
                price: Number(productForm.price),
                stock: Number(productForm.stock),
            }, authConfig);
            setProducts((currentProducts) => currentProducts.map((product) =>
                product.id === editingProduct ? { ...product, ...response.data } : product
            ));
            setEditingProduct(null);
        } catch (requestError) {
            setActionError(requestError.response?.data || "Unable to update product.");
        }
    };

    const deleteProduct = async (productId) => {
        if (!window.confirm("Delete this product? This cannot be undone.")) return;
        setActionError("");
        try {
            await api.delete(`/Product/${productId}`, authConfig);
            setProducts((currentProducts) => currentProducts.filter((product) => product.id !== productId));
        } catch (requestError) {
            setActionError(requestError.response?.data || "Unable to delete product.");
        }
    };

    if (!isAdmin(session)) {
        return (
            <main className="admin-page admin-denied">
                <p className="eyebrow">ADMIN CONSOLE</p>
                <h1>Access restricted</h1>
                <p>You need an administrator account to view store operations.</p>
                <button type="button" onClick={() => navigate(session ? "/" : "/login")}>{session ? "Return to store" : "Go to login"}</button>
            </main>
        );
    }

    const visibleProducts = products.filter((product) => Number(product.stock) <= 5);

    return (
        <main className="admin-page">
            <header className="admin-header">
                <div>
                    <p className="eyebrow">OPERATIONS / ADMIN</p>
                    <h1>Store command center</h1>
                    <p className="admin-subtitle">Monitor orders, customers, catalog health, and revenue in one place.</p>
                </div>
                <div className="admin-header-meta">
                    <span>{lastUpdated ? `Updated ${lastUpdated.toLocaleTimeString()}` : "Syncing data..."}</span>
                    <button type="button" className="secondary-button" onClick={() => window.location.reload()}>Refresh</button>
                </div>
            </header>

            {error && <p className="admin-alert" role="status">{error}</p>}
            {actionError && <p className="admin-alert" role="alert">{actionError}</p>}

            <nav className="admin-tabs" aria-label="Admin sections">
                {[["overview", "Overview"], ["orders", "Orders"], ["customers", "Customers"], ["products", "Products"]].map(([view, label]) => (
                    <button key={view} type="button" className={activeView === view ? "active" : ""} onClick={() => setActiveView(view)}>{label}</button>
                ))}
            </nav>

            {loading ? <p className="admin-loading">Loading store data...</p> : (
                <>
                    <section className="admin-metrics" aria-label="Store summary">
                        <article><span>Total revenue</span><strong>₹{metrics.revenue.toLocaleString("en-IN")}</strong><small>Across {orders.length} orders</small></article>
                        <article><span>Customers</span><strong>{metrics.customers}</strong><small>Known customer accounts</small></article>
                        <article><span>Pending orders</span><strong>{metrics.pending}</strong><small>Need fulfilment attention</small></article>
                        <article><span>Low stock</span><strong>{metrics.lowStock}</strong><small>Products at 5 units or fewer</small></article>
                    </section>

                    {activeView === "overview" && <section className="admin-overview-grid">
                            <article className="admin-panel">
                            <div className="admin-panel-heading"><h2>Recent orders</h2><button type="button" onClick={() => setActiveView("orders")}>View all</button></div>
                            {orders.slice(0, 5).map((order) => <div className="admin-row" key={order.id ?? order.Id}><span><strong>#{order.id ?? order.Id ?? "—"}</strong><small>{getOrderDate(order) ? new Date(getOrderDate(order)).toLocaleDateString() : "Date unavailable"}</small></span><span>{getOrderItems(order).length} line(s)</span><strong>₹{getOrderTotal(order).toLocaleString("en-IN")}</strong></div>)}
                            {!orders.length && <p className="admin-empty">No orders found.</p>}
                        </article>
                        <article className="admin-panel">
                            <div className="admin-panel-heading"><h2>Stock watch</h2><button type="button" onClick={() => setActiveView("products")}>View catalog</button></div>
                            {visibleProducts.slice(0, 5).map((product) => <div className="admin-row" key={product.id}><span><strong>{product.name}</strong><small>₹{Number(product.price || 0).toLocaleString("en-IN")}</small></span><strong className={Number(product.stock) <= 0 ? "danger-text" : "warning-text"}>{Number(product.stock) <= 0 ? "Out of stock" : `${product.stock} left`}</strong></div>)}
                            {!visibleProducts.length && <p className="admin-empty">All products have healthy stock.</p>}
                        </article>
                    </section>}

                    {activeView === "orders" && <section className="admin-panel admin-table-panel"><h2>All orders</h2><div className="admin-table-wrap"><table><thead><tr><th>Order</th><th>Date</th><th>Customer</th><th>Items</th><th>Status</th><th>Total</th></tr></thead><tbody>{orders.map((order) => <tr key={order.id ?? order.Id}><td>#{order.id ?? order.Id ?? "—"}</td><td>{getOrderDate(order) ? new Date(getOrderDate(order)).toLocaleDateString() : "—"}</td><td>{order.email ?? order.customerEmail ?? order.customer?.email ?? order.userName ?? "—"}</td><td>{getOrderItems(order).length}</td><td><select className="admin-status-select" value={order.status || "Pending"} onChange={(event) => updateOrderStatus(order.id ?? order.Id, event.target.value)}><option>Pending</option><option>Processing</option><option>Shipped</option><option>Delivered</option><option>Cancelled</option><option>Refunded</option></select></td><td>₹{getOrderTotal(order).toLocaleString("en-IN")}</td></tr>)}</tbody></table></div>{!orders.length && <p className="admin-empty">No orders found.</p>}</section>}

                    {activeView === "customers" && <section className="admin-panel admin-table-panel"><h2>Customer directory</h2><div className="admin-table-wrap"><table><thead><tr><th>Name</th><th>Email</th><th>Phone</th><th>Role</th></tr></thead><tbody>{customerRows.map((customer) => <tr key={customer.id ?? customer.email}><td>{customer.name ?? (`${customer.firstName ?? ""} ${customer.lastName ?? ""}`.trim() || "—")}</td><td>{customer.email ?? "—"}</td><td>{customer.phone ?? "—"}</td><td>{customer.role ?? "Customer"}</td></tr>)}</tbody></table></div>{!customerRows.length && <p className="admin-empty">No customer records are available yet.</p>}</section>}

                    {activeView === "products" && <>
                        {editingProduct !== null && <form className="admin-product-form" onSubmit={saveProduct}><h2>Edit product</h2><input value={productForm.name} required placeholder="Product name" onChange={(event) => setProductForm({ ...productForm, name: event.target.value })} /><textarea value={productForm.description} placeholder="Description" onChange={(event) => setProductForm({ ...productForm, description: event.target.value })} /><div className="admin-form-row"><input type="number" min="0" step="0.01" value={productForm.price} required placeholder="Price" onChange={(event) => setProductForm({ ...productForm, price: event.target.value })} /><input type="number" min="0" value={productForm.stock} required placeholder="Stock" onChange={(event) => setProductForm({ ...productForm, stock: event.target.value })} /></div><button type="submit">Save changes</button><button type="button" className="secondary-button" onClick={() => setEditingProduct(null)}>Cancel</button></form>}
                        <section className="admin-product-grid">{products.map((product) => <article className="admin-product-card" key={product.id}><img src={getProductImage(product)} alt="" onError={(event) => { event.currentTarget.src = imageFallback; }} /><div><h2>{product.name}</h2><p>₹{Number(product.price || 0).toLocaleString("en-IN")} · {product.category || "Uncategorized"}</p><strong className={Number(product.stock) <= 5 ? "warning-text" : ""}>{product.stock ?? "—"} in stock</strong></div><div className="admin-product-actions"><Link to={`/product/${product.id}`}>Open product</Link><button type="button" onClick={() => startProductEdit(product)}>Edit</button><button type="button" className="danger-button" onClick={() => deleteProduct(product.id)}>Delete</button></div></article>)}{!products.length && <p className="admin-empty">No products found.</p>}</section>
                    </>}
                </>
            )}
        </main>
    );
}

export default Admin;