import { useLocation, useNavigate } from "react-router-dom";

function BackButton() {
    const navigate = useNavigate();
    const location = useLocation();

    if (location.pathname === "/") return null;

    const goBack = () => {
        if (window.history.length > 1) {
            navigate(-1);
        } else {
            navigate("/");
        }
    };

    return (
        <div className="back-button-bar">
            <button type="button" className="site-back-button" onClick={goBack}>
                ← Back
            </button>
        </div>
    );
}

export default BackButton;
