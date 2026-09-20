import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCart } from "../useCart";

function CartNotification() {
    const { cart = [] } = useCart();
    const navigate = useNavigate();
    const [visible, setVisible] = useState(false);
    const previousCount = useRef(null);
    const itemCount = cart.reduce((total, item) => total + item.quantity, 0);

    useEffect(() => {
        if (previousCount.current === null) {
            previousCount.current = itemCount;
            return;
        }

        if (itemCount > previousCount.current) {
            setVisible(true);
            const timer = setTimeout(() => setVisible(false), 3000);
            previousCount.current = itemCount;
            return () => clearTimeout(timer);
        }

        previousCount.current = itemCount;
    }, [itemCount]);

    if (!visible) return null;

    return (
        <div className="cart-notification" role="status">
            <span>Product added to cart.</span>
            <button type="button" onClick={() => navigate("/cart")}>
                View Cart
            </button>
            <button type="button" onClick={() => setVisible(false)} aria-label="Close notification">
                ×
            </button>
        </div>
    );
}

export default CartNotification;
