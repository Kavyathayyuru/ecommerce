import { useEffect, useState } from "react";
import { CartContext } from "./CartContextDefinition";
export function CartProvider({ children }) {
    const [cart, setCart] = useState(() => {
        try {
            return JSON.parse(localStorage.getItem("cart") || "[]");
        } catch {
            return [];
        }
    });

    useEffect(() => {
        localStorage.setItem("cart", JSON.stringify(cart));
    }, [cart]);

    const addToCart = (product) => {
        setCart((currentCart) => {
            const existingItem = currentCart.find(
                (item) => item.product.id === product.id
            );

            if (existingItem) {
                return currentCart.map((item) =>
                    item.product.id === product.id
                        ? { ...item, quantity: item.quantity + 1 }
                        : item
                );
            }

            return [...currentCart, { product, quantity: 1 }];
        });
    };

    const increaseQuantity = (productId) => {
        setCart((currentCart) => currentCart.map((item) =>
                item.product.id === productId
                    ? { ...item, quantity: item.quantity + 1 }
                    : item
            ));
    };

    const decreaseQuantity = (productId) => {
        setCart((currentCart) => currentCart
                .map((item) =>
                    item.product.id === productId
                        ? { ...item, quantity: item.quantity - 1 }
                        : item
                )
                .filter((item) => item.quantity > 0)
        );
    };

    const clearCart = () => setCart([]);

    const cartTotal = cart.reduce(
        (total, item) => total + item.product.price * item.quantity,
        0
    );

    return (
        <CartContext.Provider
            value={{
                cart,
                addToCart,
                increaseQuantity,
                decreaseQuantity,
                clearCart,
                cartTotal
            }}
        >
            {children}
        </CartContext.Provider>
    );
}
