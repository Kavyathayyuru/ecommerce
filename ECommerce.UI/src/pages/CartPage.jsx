import Cart from "../components/Cart";
import { useCart } from "../useCart";

function CartPage() {
    const {
        cart = [],
        increaseQuantity,
        decreaseQuantity,
        cartTotal = 0,
    } = useCart();

    return (
        <main className="cart-page">
            <Cart
                cart={cart}
                increaseQuantity={increaseQuantity}
                decreaseQuantity={decreaseQuantity}
                cartTotal={cartTotal}
            />
        </main>
    );
}

export default CartPage;