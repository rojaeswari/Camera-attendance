import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getDashboard } from "../api/dashboardapi";
import "./Dashboard.css";

const Dashboard = () => {

    const [dashboard, setDashboard] = useState({
        totalUsers: 0,
        activeCameras: 0,
        todayAttendance: 0,
        present: 0,
        recentAttendance: []
    });

    const [loading, setLoading] = useState(true);

    const loadDashboard = async () => {
        try {

            const response = await getDashboard();

            if (response.success) {
                setDashboard(response.data);
            }

        } catch (error) {

            console.error("Dashboard loading error:", error);

        } finally {

            setLoading(false);

        }
    };

    useEffect(() => {
        loadDashboard();
    }, []);

    return (
        <div className="dashboard-page">

            <div className="dashboard-header">
                <div>
                    <h1>Dashboard</h1>
                    <p>Overview of your camera attendance system</p>
                </div>
            </div>


            {/* ================= CARDS ================= */}

            <div className="dashboard-cards">

                <div className="dashboard-card">
                    <h3>Total Users</h3>
                    <p>
                        {loading ? "..." : dashboard.totalUsers}
                    </p>
                </div>


                <div className="dashboard-card">
                    <h3>Active Cameras</h3>
                    <p>
                        {loading ? "..." : dashboard.activeCameras}
                    </p>
                </div>


                <div className="dashboard-card">
                    <h3>Today's Attendance</h3>
                    <p>
                        {loading ? "..." : dashboard.todayAttendance}
                    </p>
                </div>


                <div className="dashboard-card">
                    <h3>Present</h3>
                    <p>
                        {loading ? "..." : dashboard.present}
                    </p>
                </div>

            </div>


            {/* ================= ATTENDANCE SUMMARY ================= */}

            <div className="dashboard-section">

                <h2>Today's Attendance</h2>

                <div className="attendance-summary">

                    <div>
                        <span>Present</span>
                        <strong>
                            {dashboard.present}
                        </strong>
                    </div>

                    <div>
                        <span>Total</span>
                        <strong>
                            {dashboard.totalUsers}
                        </strong>
                    </div>

                    <div>
                        <span>Absent</span>
                        <strong>
                            {Math.max(
                                dashboard.totalUsers -
                                dashboard.present,
                                0
                            )}
                        </strong>
                    </div>

                </div>

            </div>


            {/* ================= RECENT ATTENDANCE ================= */}

            <div className="dashboard-section">

                <div className="section-header">

                    <h2>Recent Attendance</h2>

                    <Link to="/attendance">
                        View All
                    </Link>

                </div>


                {dashboard.recentAttendance.length === 0 ? (

                    <p className="no-attendance">
                        No attendance records found
                    </p>

                ) : (

                    <div className="attendance-table-wrapper">

                        <table className="attendance-table">

                            <thead>
                                <tr>
                                    <th>Name</th>
                                    <th>Date</th>
                                    <th>Time</th>
                                    <th>Camera</th>
                                    <th>Status</th>
                                </tr>
                            </thead>

                            <tbody>

                                {dashboard.recentAttendance.map(
                                    (item) => (

                                        <tr key={item.id}>

                                            <td>
                                                {item.userName}
                                            </td>

                                            <td>
                                                {new Date(
                                                    item.attendanceDate
                                                ).toLocaleDateString()}
                                            </td>

                                            <td>
                                                {item.attendanceTime}
                                            </td>

                                            <td>
                                                {item.cameraName || "Unknown"}
                                            </td>

                                            <td>

                                                <span className="status active">
                                                    {item.status}
                                                </span>

                                            </td>

                                        </tr>

                                    )
                                )}

                            </tbody>

                        </table>

                    </div>

                )}

            </div>

        </div>
    );
};

export default Dashboard;