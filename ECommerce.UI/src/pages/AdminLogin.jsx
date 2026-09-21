import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api } from "../api";

function isAdmin(session) {
    return [session?.role, session?.Role, session?.userRole, session?.user?.role]
        .some((role) => String(role || "").toLowerCase() === "admin");
}

function readTokenClaims(token) {
    try {
        const payload = token.split(".")[1];
        return JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
    } catch {
        return {};
    }
}

function normalizeSession(responseData) {
    const token = responseData?.token || responseData?.Token || responseData;
    const claims = typeof token === "string" ? readTokenClaims(token) : {};
    const role = responseData?.role || responseData?.Role || claims.role || claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    return { token, role, isAdmin: responseData?.isAdmin ?? responseData?.IsAdmin ?? role?.toLowerCase() === "admin" };
}

function AdminLogin() {
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
            const session = normalizeSession(response.data);

            if (!session.isAdmin || !isAdmin(session)) {
                setError("This account does not have administrator access.");
                return;
            }

            localStorage.setItem("user", JSON.stringify(session));
            window.dispatchEvent(new Event("auth-change"));
            navigate("/admin");
        } catch (requestError) {
            const responseMessage = requestError.response?.data;
            setError(
                typeof responseMessage === "string"
                    ? responseMessage
                    : responseMessage?.message || "Admin login failed. Check your credentials and confirm the API is running."
            );
        } finally {
            setLoading(false);
        }
    };

    return (
        <main className="auth-page admin-login-page">
            <p className="eyebrow">STORE OPERATIONS</p>
            <h1>Admin login</h1>
            <p className="auth-intro">Sign in with an administrator account to monitor the store.</p>
            {error && <p className="error-message">{error}</p>}
            <form onSubmit={handleSubmit} className="auth-form">
                <label htmlFor="admin-email">Admin email</label>
                <input id="admin-email" type="email" placeholder="Admin email" value={form.email} required
                    onChange={(event) => setForm({ ...form, email: event.target.value })} />
                <label htmlFor="admin-password">Password</label>
                <input id="admin-password" type="password" placeholder="Password" value={form.password} required
                    onChange={(event) => setForm({ ...form, password: event.target.value })} />
                <button type="submit" disabled={loading}>{loading ? "Opening admin console..." : "Sign in as admin"}</button>
            </form>
            <Link className="customer-login-link" to="/login">Customer login</Link>
        </main>
    );
}

export default AdminLogin;