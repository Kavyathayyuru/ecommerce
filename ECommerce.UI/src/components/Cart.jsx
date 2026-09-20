import { useNavigate } from "react-router-dom";
import { getProductImage, imageFallback } from "../utils/productImage";

function Cart({
    cart,
    increaseQuantity,
    decreaseQuantity,
    cartTotal,
}) {
    const navigate = useNavigate();

    if (cart.length === 0) {
        return (
            <section className="cart-container empty-cart-state">
                <div className="empty-cart-icon" aria-hidden="true">🛒</div>
                <h1>Your cart is empty</h1>
                <p>Add something you love and it will appear here.</p>

                <button className="continue-shopping-button" onClick={() => navigate("/products")}>
                    Continue Shopping
                </button>
            </section>
        );
    }

    return (
        <section className="cart-container">
            <h2>🛒 Your Cart</h2>

            <div className="cart-items">
                {cart.map((item) => (
                    <article
                        className="cart-item"
                        key={item.product.id}
                    >
                        <img
                            className="cart-item-image"
                            src={getProductImage(item.product)}
                            alt={item.product.name}
                            onError={(event) => {
                                event.currentTarget.src =
                                    imageFallback;
                            }}
                        />

                        <div className="cart-item-details">
                            <h3>{item.product.name}</h3>
                            <p>
                                ₹{Number(item.product.price).toLocaleString("en-IN")}
                            </p>

                            <div className="quantity-controls">
                                <button
                                    onClick={() =>
                                        decreaseQuantity(item.product.id)
                                    }
                                >
                                    −
                                </button>

                                <span>{item.quantity}</span>

                                <button
                                    onClick={() =>
                                        increaseQuantity(item.product.id)
                                    }
                                >
                                    +
                                </button>
                            </div>
                        </div>

                        <strong>
                            ₹
                            {(
                                Number(item.product.price) * item.quantity
                            ).toLocaleString("en-IN")}
                        </strong>
                    </article>
                ))}
            </div>

            <div className="cart-summary">
                <h2>
                    Total: ₹{Number(cartTotal).toLocaleString("en-IN")}
                </h2>

                <button
                    className="checkout-button"
                    onClick={() => navigate("/checkout")}
                >
                    Proceed to Checkout
                </button>
            </div>
        </section>
    );
}

export default Cart;