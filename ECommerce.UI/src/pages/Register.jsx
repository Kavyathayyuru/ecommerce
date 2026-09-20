import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";

function Register() {
    const navigate = useNavigate();
    const [form, setForm] = useState({
        firstName: "",
        lastName: "",
        username: "",
        email: "",
        phone: "",
        otp: "",
        password: "",
    });
    const [error, setError] = useState("");
    const [otpCode, setOtpCode] = useState("");
    const [otpSent, setOtpSent] = useState(false);
    const [otpVerified, setOtpVerified] = useState(false);
    const [otpMessage, setOtpMessage] = useState("");
    const [loading, setLoading] = useState(false);

    const updateField = (event) => {
        setForm({ ...form, [event.target.name]: event.target.value });
        if (event.target.name === "phone") {
            setOtpSent(false);
            setOtpVerified(false);
            setOtpMessage("");
        }
    };

    const sendOtp = () => {
        const phone = form.phone.replace(/\D/g, "");
        if (phone.length !== 10) {
            setError("Enter a valid 10-digit phone number first.");
            return;
        }

        const generatedOtp = String(Math.floor(100000 + Math.random() * 900000));
        setOtpCode(generatedOtp);
        setOtpSent(true);
        setOtpVerified(false);
        setError("");
        setOtpMessage(`Development OTP: ${generatedOtp}`);
    };

    const verifyOtp = () => {
        if (form.otp !== otpCode) {
            setError("The OTP is incorrect. Please try again.");
            return;
        }
        setOtpVerified(true);
        setError("");
        setOtpMessage("Phone number verified successfully.");
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        setLoading(true);
        setError("");
        try {
            if (!otpVerified) {
                setError("Verify your phone number with the OTP before registering.");
                setLoading(false);
                return;
            }

            await axios.post("https://localhost:7070/api/auth/register", {
                username: form.username || `${form.firstName} ${form.lastName}`.trim(),
                email: form.email,
                password: form.password,
                firstName: form.firstName,
                lastName: form.lastName,
                phone: form.phone,
            });
            navigate("/login");
        } catch (requestError) {
            const responseMessage = requestError.response?.data;
            setError(
                typeof responseMessage === "string"
                    ? responseMessage
                    : responseMessage?.message || "Registration failed."
            );
        } finally {
            setLoading(false);
        }
    };

    return (
        <main className="auth-page">
            <h1>Create Account</h1>
            {error && <p className="error-message">{error}</p>}
            <form onSubmit={handleSubmit} className="auth-form">
                <input name="firstName" placeholder="First name" value={form.firstName} required onChange={updateField} />
                <input name="lastName" placeholder="Last name" value={form.lastName} required onChange={updateField} />
                <input name="username" placeholder="Username (optional)" value={form.username} onChange={updateField} />
                <input name="email" type="email" placeholder="Email" value={form.email} required onChange={updateField} />
                <div className="otp-row">
                    <input name="phone" type="tel" placeholder="Phone number" value={form.phone} required onChange={updateField} />
                    <button type="button" className="secondary-button" onClick={sendOtp}>Send OTP</button>
                </div>
                {otpSent && (
                    <div className="otp-row">
                        <input name="otp" inputMode="numeric" maxLength={6} placeholder="Enter OTP" value={form.otp} required onChange={updateField} />
                        <button type="button" className="secondary-button" onClick={verifyOtp} disabled={otpVerified}>
                            {otpVerified ? "Verified" : "Verify OTP"}
                        </button>
                    </div>
                )}
                {otpMessage && <p className={otpVerified ? "success-message" : "otp-hint"}>{otpMessage}</p>}
                <input name="password" type="password" placeholder="Password" value={form.password} minLength={6} required onChange={updateField} />
                <button type="submit" disabled={loading}>{loading ? "Creating..." : "Register"}</button>
            </form>
            <p>Already registered? <Link to="/login">Login</Link></p>
        </main>
    );
}

export default Register;