import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { useCart } from "../useCart";

function Navbar() {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const [search, setSearch] = useState(() => searchParams.get("search") || "");
    const [user, setUser] = useState(() => localStorage.getItem("user"));
    const session = user ? JSON.parse(user) : null;
    const isAdmin = [session?.role, session?.Role, session?.userRole, session?.user?.role]
        .some((role) => String(role || "").toLowerCase() === "admin");

    useEffect(() => {
        const refreshUser = () => setUser(localStorage.getItem("user"));
        window.addEventListener("auth-change", refreshUser);
        return () => window.removeEventListener("auth-change", refreshUser);
    }, []);
    const { cart = [] } = useCart();
    const cartCount = cart.reduce((total, item) => total + item.quantity, 0);

    const handleSearch = (event) => {
        event.preventDefault();
        const value = search.trim();
        navigate(value ? `/products?search=${encodeURIComponent(value)}` : "/products");
    };

    return (
        <header className="navbar">

            <div className="navbar-container">

                <Link to="/" className="logo">
                    🛒 E-Commerce
                </Link>

                <form className="search-box" onSubmit={handleSearch}>
                    <input
                        type="text"
                        value={search}
                        onChange={(event) => setSearch(event.target.value)}
                        placeholder="Search products, brands and more"
                        aria-label="Search products"
                    />
                    <button type="submit" aria-label="Search">🔍</button>
                </form>

                <nav className="nav-links">

                    <Link to="/">Home</Link>

                    <Link to="/products">Products</Link>
                    <Link to="/products/laptops">Categories</Link>

                    <Link to="/orders">📦 Orders</Link>

                    <Link to="/cart">🛒 Cart ({cartCount})</Link>

                    {isAdmin && <Link to="/admin">Admin</Link>}

                    {user ? (
                        <button type="button" onClick={() => { localStorage.removeItem("user"); setUser(null); navigate("/"); }}>
                            Logout
                        </button>
                    ) : (
                        <>
                            <Link to="/login">Login</Link>
                            <Link className="register-link" to="/register">Register</Link>
                        </>
                    )}

                </nav>

            </div>

        </header>
    );
}

export default Navbar;