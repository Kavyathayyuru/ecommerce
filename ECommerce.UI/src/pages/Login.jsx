import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api } from "../api";

function Login() {
    const navigate = useNavigate();
    const [form, setForm] = useState({ email: "", password: "" });
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (event) => {
        event.preventDefault();
        setLoading(true);
        setError("");
        try {
            const response = await api.post("/auth/login", {
                email: form.email.trim().toLowerCase(),
                password: form.password,
            });
            localStorage.setItem("user", JSON.stringify(response.data));
            window.dispatchEvent(new Event("auth-change"));
            navigate("/");
        } catch (requestError) {
            setError(requestError.response?.data?.message || "Login failed. Check your credentials.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <main className="auth-page">
            <h1>Login</h1>
            {error && <p className="error-message">{error}</p>}
            <form onSubmit={handleSubmit} className="auth-form">
                <input type="email" placeholder="Email" value={form.email} required
                    onChange={(event) => setForm({ ...form, email: event.target.value })} />
                <input type="password" placeholder="Password" value={form.password} required
                    onChange={(event) => setForm({ ...form, password: event.target.value })} />
                <button type="submit" disabled={loading}>{loading ? "Signing in..." : "Login"}</button>
            </form>
            <p>New customer? <Link to="/register">Create an account</Link></p>
            <Link className="admin-login-link" to="/admin/login">Admin login</Link>
        </main>
    );
}

export default Login;