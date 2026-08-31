import { Link, useLocation } from "react-router-dom";
import "./Sidebar.css";

const Sidebar = () => {

    const location = useLocation();

    return (
        <div className="sidebar">

            <div className="sidebar-logo">
                📷 Camera Attendance
            </div>

            <nav>

                <Link
                    to="/dashboard"
                    className={location.pathname === "/dashboard" ? "active" : ""}
                >
                    🏠 Dashboard
                </Link>

                <Link
                    to="/users"
                    className={location.pathname === "/users" ? "active" : ""}
                >
                    👥 Users
                </Link>

                <Link
                    to="/attendance"
                    className={location.pathname === "/attendance" ? "active" : ""}
                >
                    📋 Attendance
                </Link>

                <Link
                    to="/cameras"
                    className={location.pathname === "/cameras" ? "active" : ""}
                >
                    📹 Cameras
                </Link>

                <Link
                    to="/captured-images"
                    className={location.pathname === "/captured-images" ? "active" : ""}
                >
                    🖼️ Captured Images
                </Link>

                <Link
                    to="/face-recognition"
                    className={location.pathname === "/face-recognition" ? "active" : ""}
                >
                    👤 Face Recognition
                </Link>

                <Link
                    to="/strangers"
                    className={location.pathname === "/strangers" ? "active" : ""}
                >
                    👤 Strangers
                </Link>

                <Link
                    to="/reports"
                    className={location.pathname === "/reports" ? "active" : ""}
                >
                    📊 Reports
                </Link>

                <Link
                    to="/settings"
                    className={location.pathname === "/settings" ? "active" : ""}
                >
                    ⚙️ Settings
                </Link>

            </nav>

            <div className="sidebar-bottom">

                <Link to="/login">
                    🚪 Logout
                </Link>

            </div>

        </div>
    );
};

export default Sidebar;