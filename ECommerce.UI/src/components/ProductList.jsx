import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useSearchParams } from "react-router-dom";
import axios from "axios";
import { useCart } from "../useCart";
import { getProductImage, imageFallback } from "../utils/productImage";

function ProductList() {
  const navigate = useNavigate();
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const { cart, addToCart } = useCart();
  const [searchParams] = useSearchParams();
  const searchTerm = (searchParams.get("search") || "").toLowerCase();

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const response = await axios.get("https://localhost:7070/api/Product");
        setProducts(Array.isArray(response.data) ? response.data : []);
      } catch (err) {
        console.error("Failed to fetch products:", err);
        setError("Unable to load products right now.");
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, []);
 
  if (loading) {
    return <div><h1>Products</h1><p>Loading products...</p></div>;
  }

  if (error) {
    return <div><h1>Products</h1><p>{error}</p></div>;
  }

  return (
    <div className="products-page">

        <div className="section-heading">
          <div>
            <p className="eyebrow">CURATED FOR YOU</p>
            <h1>{searchTerm ? `Results for “${searchParams.get("search")}”` : "All Products"}</h1>
          </div>
          <span className="cart-summary-chip">🛒 {cart.reduce((total, item) => total + item.quantity, 0)} items in cart</span>
        </div>
        {products.filter((product) => !searchTerm || `${product.name} ${product.description}`.toLowerCase().includes(searchTerm)).length === 0 ? (
            <p>Our products will appear here.</p>
        ) : (
            <div className="product-grid">

                {products.filter((product) => !searchTerm || `${product.name} ${product.description}`.toLowerCase().includes(searchTerm)).map((product) => (
                    <div className="product-card" key={product.id}>

                      <img
                        className="product-image"
                        src={getProductImage(product)}
                        alt={product.name}
                        onError={(event) => {
                          event.currentTarget.src = imageFallback;
                        }}
                      />

                        <h2>{product.name}</h2>

                        <p className="description">
                            {product.description}
                        </p>

                        <p className="price">
                            ₹{product.price}
                        </p>

                        <p className="stock">
                            Stock: {product.stock}
                        </p>

                       <button onClick={() => navigate(`/product/${product.id}`)}>
                            View Details
                       </button>

                       <button onClick={() => addToCart(product)}>
                            Add to Cart
                       </button>

                    </div>
                ))}

            </div>
        )}

    </div>
);
}

export default ProductList;