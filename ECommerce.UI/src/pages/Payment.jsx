import { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { useCart } from "../useCart";

function Payment() {
    const navigate = useNavigate();
    const { cart, cartTotal, clearCart } = useCart();

    const [form, setForm] = useState({
        name: "",
        email: "",
        phone: "",
        address: "",
        city: "",
        postalCode: "",
        paymentMethod: "Card",
    });

    const [error, setError] = useState("");
    const [processing, setProcessing] = useState(false);
    const [paymentStatus, setPaymentStatus] = useState("");

    const handleChange = (event) => {
        setForm({
            ...form,
            [event.target.name]: event.target.value,
        });
    };

    const handleSubmit = (event) => {
        event.preventDefault();

        if (cart.length === 0) {
            setError("Your cart is empty.");
            return;
        }

        const session = JSON.parse(localStorage.getItem("user") || "null");
        if (!session?.token) {
            setError("Please log in before placing an order.");
            navigate("/login");
            return;
        }

        setProcessing(true);
        setError("");
        setPaymentStatus("Payment is being processed...");

        setTimeout(async () => {
            setPaymentStatus("Your payment is successful!");

            try {
                const response = await axios.post(
                    "https://localhost:7070/api/Order",
                    {
                        items: cart.map((item) => ({
                            productId: item.product.id,
                            quantity: item.quantity,
                        })),
                    },
                    { headers: { Authorization: `Bearer ${session.token}` } }
                );

                const order = response.data;
                const previousOrders = JSON.parse(localStorage.getItem("orders") || "[]");
                localStorage.setItem("orders", JSON.stringify([order, ...previousOrders]));

                clearCart();
                navigate("/orders", {
                    state: { successMessage: "Order placed successfully." },
                });
            } catch (requestError) {
                console.error("Failed to create backend order:", requestError);
                if (requestError.response?.status === 401) {
                    localStorage.removeItem("user");
                    window.dispatchEvent(new Event("auth-change"));
                    setError("Your login session expired. Please log in again before placing the order.");
                } else {
                    const responseMessage = requestError.response?.data;
                    const backendMessage = typeof responseMessage === "string"
                        ? responseMessage
                        : responseMessage?.message;
                    setError(
                        backendMessage ||
                        "Payment succeeded, but the order could not be saved. Please try again."
                    );
                }
                setProcessing(false);
                setPaymentStatus("");
            }
        }, 1200);
    };

    if (cart.length === 0) {
        return (
            <main className="checkout-page">
                <h1>Checkout</h1>
                <p>Your cart is empty.</p>
                <button onClick={() => navigate("/products")}>
                    Continue Shopping
                </button>
            </main>
        );
    }

    return (
        <main className="checkout-page">
            <h1>Checkout</h1>

            {error && <p className="error-message">{error}</p>}
            {paymentStatus && <p className="success-message" role="status">{paymentStatus}</p>}

            <form className="checkout-form" onSubmit={handleSubmit}>
                <input
                    name="name"
                    placeholder="Full name"
                    value={form.name}
                    onChange={handleChange}
                    required
                />

                <input
                    type="email"
                    name="email"
                    placeholder="Email address"
                    value={form.email}
                    onChange={handleChange}
                    required
                />

                <input
                    name="phone"
                    placeholder="Phone number"
                    value={form.phone}
                    onChange={handleChange}
                    required
                />

                <textarea
                    name="address"
                    placeholder="Delivery address"
                    value={form.address}
                    onChange={handleChange}
                    required
                />

                <input
                    name="city"
                    placeholder="City"
                    value={form.city}
                    onChange={handleChange}
                    required
                />

                <input
                    name="postalCode"
                    placeholder="Postal code"
                    value={form.postalCode}
                    onChange={handleChange}
                    required
                />

                <select
                    name="paymentMethod"
                    value={form.paymentMethod}
                    onChange={handleChange}
                >
                    <option value="Card">Credit/Debit Card</option>
                    <option value="UPI">UPI</option>
                    <option value="Cash on Delivery">
                        Cash on Delivery
                    </option>
                </select>

                <h2>
                    Total: ₹{Number(cartTotal).toLocaleString("en-IN")}
                </h2>

                <button type="submit" disabled={processing}>
                    {processing ? "Processing..." : "Place Order"}
                </button>
            </form>
        </main>
    );
}

export default Payment;