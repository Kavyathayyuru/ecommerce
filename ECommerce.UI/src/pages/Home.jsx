import { Link } from "react-router-dom";
function Home() {
    return (
        <div className="home-page">

            <section className="hero">

                <div className="hero-content">

                    <p className="hero-tag">
                        Welcome to E-Commerce
                    </p>

                    <h1>
                        Everything You Need,
                        <br />
                        All in One Place
                    </h1>

                    <p>
                        Discover quality products at amazing prices.
                    </p>

                    <Link to="/products" className="shop-now">
                        Shop Now 🛍️
                    </Link>

                </div>

            </section>

            <section className="categories">

                <h2>Shop by Category</h2>

                <div className="category-grid">

                    <Link to="/products/laptops" className="category-card">
                        💻
                        <h3>Laptops</h3>
                    </Link>

                    <Link to="/products/mobiles" className="category-card">
                        📱
                        <h3>Mobiles</h3>
                    </Link>

                    <Link to="/products/audio" className="category-card">
                        🎧
                        <h3>Audio</h3>
                    </Link>

                    <Link to="/products/accessories" className="category-card">
                        🖱️
                        <h3>Accessories</h3>
                    </Link>

                </div>

            </section>

            <section className="benefits" aria-label="Shopping benefits">
                <div><span>🚚</span><strong>Fast delivery</strong><small>Reliable delivery to your door</small></div>
                <div><span>🔒</span><strong>Secure checkout</strong><small>Your details stay protected</small></div>
                <div><span>↩️</span><strong>Easy returns</strong><small>Shop confidently and hassle-free</small></div>
                <div><span>💬</span><strong>Customer support</strong><small>We are here when you need us</small></div>
            </section>

        </div>
    );
}

export default Home;