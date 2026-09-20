import { useEffect, useState } from "react";
import axios from "axios";
import { useCart } from "../useCart";
import { getProductImage, imageFallback } from "../utils/productImage";

function CategoryPage() {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const { addToCart } = useCart();

    const category = window.location.pathname.split("/").pop();

    useEffect(() => {
        const fetchProducts = async () => {
            try {
                const response = await axios.get(
                    "https://localhost:7070/api/Product"
                );

                setProducts(response.data);
            } catch (error) {
                console.error("Failed to fetch products:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchProducts();
    }, []);

    const categoryKeywords = {
        laptops: ["laptop"],
        mobiles: ["iphone", "samsung"],
        audio: ["headphone", "speaker"],
        accessories: [
            "keyboard",
            "mouse",
            "hub",
            "webcam",
            "power bank",
            "stand"
        ]
    };

    const keywords = categoryKeywords[category] || [];

    const filteredProducts = products.filter((product) => {
        const productText =
            `${product.name} ${product.description}`.toLowerCase();

        return keywords.some((keyword) =>
            productText.includes(keyword)
        );
    });

    if (loading) {
        return <h2>Loading products...</h2>;
    }

    return (
        <div className="category-page">

            <h1>
                {category.charAt(0).toUpperCase() + category.slice(1)}
            </h1>

            <div className="product-grid">

                {filteredProducts.map((product) => (
                    <div className="product-card" key={product.id}>

                        <img className="product-image"
                            src={getProductImage(product)}
                            alt={product.name}
                            onError={(event) => { event.currentTarget.src = imageFallback; }} />

                        <h2>{product.name}</h2>

                        <p>{product.description}</p>

                        <p className="price">
                            ₹{product.price}
                        </p>

                        <p>
                            Stock: {product.stock}
                        </p>

                        <button onClick={() => addToCart(product)} disabled={Number(product.stock) <= 0}>
                            Add to Cart
                        </button>

                    </div>
                ))}

            </div>

        </div>
    );
}

export default CategoryPage;