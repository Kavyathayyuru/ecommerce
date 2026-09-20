import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import axios from "axios";

function readStoredOrders() {
    try {
        const storedOrders = JSON.parse(localStorage.getItem("orders") || "[]");
        return Array.isArray(storedOrders) ? storedOrders : [];
    } catch {
        return [];
    }
}

function Orders() {
    const location = useLocation();
    const navigate = useNavigate();
    const [orders, setOrders] = useState(readStoredOrders);

    useEffect(() => {
        const session = JSON.parse(localStorage.getItem("user") || "null");
        if (!session?.token) return;

        axios.get("https://localhost:7070/api/Order", {
            headers: { Authorization: `Bearer ${session.token}` },
        }).then((response) => {
            const backendOrders = Array.isArray(response.data) ? response.data : [];
            setOrders(backendOrders);
            localStorage.setItem("orders", JSON.stringify(backendOrders));
        }).catch((error) => {
            console.error("Failed to load backend orders:", error);
        });
    }, []);

    return (
        <main className="orders-page">
            <h1>Previous Orders</h1>
            {location.state?.successMessage && <p className="success-message">{location.state.successMessage}</p>}
            {orders.length === 0 ? (
                <section className="orders-empty">
                    <p>You have not placed any orders yet.</p>
                    <button onClick={() => navigate("/products")}>Start Shopping</button>
                </section>
            ) : (
                <div className="orders-list">
                    {orders.map((order) => (
                        <article className="order-card" key={order.id ?? order.Id}>
                            <h2>Order #{order.id ?? order.Id}</h2>
                            <p>{new Date(order.date ?? order.orderDate).toLocaleString()}</p>
                            <p>{(order.items ?? order.orderItems ?? []).length} product line(s) · Status: {order.status}</p>
                            <strong>Total: ₹{Number(order.total ?? order.totalAmount ?? 0).toLocaleString("en-IN")}</strong>
                        </article>
                    ))}
                </div>
            )}
        </main>
    );
}

export default Orders;