import { BrowserRouter, Routes, Route, Link } from "react-router-dom";

import Navbar from "./components/Navbar";

import Home from "./pages/Home";
import Products from "./pages/Products";
import CartPage from "./pages/Cartpage";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Orders from "./pages/Orders";
import Payment from "./pages/Payment";
import CategoryPage from "./pages/CategoryPage";
import "./App.css";
import ProductDetails from "./pages/ProductDetails";
import { CartProvider } from "./CartContext";
import CartNotification from "./components/CartNotification";
import BackButton from "./components/BackButton";

function App() {
    return (
        <CartProvider>
        <BrowserRouter>

            <Navbar />
            <CartNotification />
            <BackButton />

            <Routes>

                <Route path="/" element={<Home />} />

                <Route path="/products" element={<Products />} />

                <Route path="/cart" element={<CartPage />} />

                <Route path="/login" element={<Login />} />

                <Route path="/register" element={<Register />} />

                <Route path="/orders" element={<Orders />} />

                <Route path="/payment" element={<Payment />} />

                <Route path="/checkout" element={<Payment />} />

                <Route path="/products/:category" element={<CategoryPage />} />

                <Route path="/product/:id" element={<ProductDetails />}/>

                

            </Routes>

            <footer className="site-footer">
                <div>
                    <strong>🛒 E-Commerce</strong>
                    <span>Quality products, simple shopping.</span>
                </div>
                <nav aria-label="Footer navigation">
                    <Link to="/products">Shop</Link>
                    <Link to="/orders">Orders</Link>
                    <Link to="/cart">Cart</Link>
                    <Link to="/register">Create account</Link>
                </nav>
            </footer>

        </BrowserRouter>
        </CartProvider>
    );
}

export default App;