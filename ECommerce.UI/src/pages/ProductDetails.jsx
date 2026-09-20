import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import { useCart } from "../useCart";
import { getProductImage, imageFallback } from "../utils/productImage";


function ProductDetails() {

    const { id } = useParams();
    const navigate = useNavigate();
    const { addToCart } = useCart();

    const [product, setProduct] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {

        const fetchProduct = async () => {

            try {

                const response = await axios.get(
                    `https://localhost:7070/api/Product/${id}`
                );

                setProduct(response.data);

            } catch (err) {

                console.error("Failed to fetch product:", err);
                setError("Unable to load product details.");

            } finally {

                setLoading(false);

            }
        };

        fetchProduct();

    }, [id]);

    if (loading) {
        return <h2 className="details-message">Loading product...</h2>;
    }

    if (error) {
        return <h2 className="details-message">{error}</h2>;
    }

    if (!product) {
        return <h2 className="details-message">Product not found.</h2>;
    }

    const imageUrl = getProductImage(product);
    const outOfStock = Number(product.stock) <= 0;

    return (
        <div className="product-details-page">

            <button
                className="back-button"
                onClick={() => navigate(-1)}
            >
                ← Back
            </button>

            <div className="product-details-card">
                <img
                    className="product-details-image"
                    src={imageUrl}
                    alt={product.name}
                    onError={(event) => { event.currentTarget.src = imageFallback; }}
                />

                <div className="product-details-info">

                    <h1>{product.name}</h1>

                    <p className="details-description">
                        {product.description}
                    </p>

                    <p className="details-price">
                        ₹{product.price}
                    </p>

                    <p className="details-stock">
                        Stock Available: {product.stock}
                    </p>

                    <button className="details-cart-button"
                        onClick={() => addToCart(product)}
                        disabled={outOfStock}>
                        🛒 Add to Cart
                    </button>

                    <button className="buy-now-button" disabled={outOfStock}
                        onClick={() => { addToCart(product); navigate("/cart"); }}>
                        Buy Now
                    </button>

                </div>

            </div>

        </div>
    );
}

export default ProductDetails;