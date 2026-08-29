import "./Dashboard.css";
import { Link } from "react-router-dom";

const Dashboard = () => {
    return (
        <div className="dashboard-page">
            <h1>Dashboard</h1>

            <div className="dashboard-cards">
                <div className="dashboard-card">
                    <h3>Total Users</h3>
                    <p>0</p>
                </div>

                <div className="dashboard-card">
                    <h3>Active Cameras</h3>
                    <p>0</p>
                </div>

                <div className="dashboard-card">
                    <h3>Today's Attendance</h3>
                    <p>0</p>
                </div>

                <div className="dashboard-card">
                    <h3>Strangers Detected</h3>
                    <p>0</p>
                    <Link to="/users">
    Users
</Link>
                </div>
            </div>
        </div>
    );
};

export default Dashboard;