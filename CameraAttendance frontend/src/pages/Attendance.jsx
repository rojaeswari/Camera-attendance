import { useEffect, useState } from "react";
import { getAttendance } from "../api/attendanceapi";
import "./Attendance.css";

const Attendance = () => {

    const [attendance, setAttendance] = useState([]);
    const [loading, setLoading] = useState(true);

    const loadAttendance = async () => {

        try {

            const response = await getAttendance();

            if (response.success) {
                setAttendance(response.data);
            }

        } catch (error) {

            console.error("Attendance loading error:", error);

        } finally {

            setLoading(false);

        }
    };

    useEffect(() => {
        loadAttendance();
    }, []);

    return (
        <div className="attendance-page">

            {/* Header */}

            <div className="attendance-header">

                <div>
                    <h1>Attendance</h1>

                    <p>
                        View user attendance records
                    </p>
                </div>

            </div>


            {/* Attendance Card */}

            <div className="attendance-card">

                {loading ? (

                    <p className="loading">
                        Loading attendance...
                    </p>

                ) : attendance.length === 0 ? (

                    <p className="no-attendance">
                        No attendance records found
                    </p>

                ) : (

                    <table>

                        <thead>

                            <tr>
                                <th>ID</th>
                                <th>User</th>
                                <th>Date</th>
                                <th>Time</th>
                                <th>Camera</th>
                                <th>Status</th>
                            </tr>

                        </thead>

                        <tbody>

                            {attendance.map((record) => (

                                <tr key={record.id}>

                                    <td>
                                        {record.id}
                                    </td>

                                    <td className="user-name">
                                        {record.userName}
                                    </td>

                                    <td>
                                        {record.attendanceDate}
                                    </td>

                                    <td>
                                        {record.attendanceTime}
                                    </td>

                                    <td>
                                        {record.cameraName || "N/A"}
                                    </td>

                                    <td>

                                        <span className="attendance-status">
                                            {record.status}
                                        </span>

                                    </td>

                                </tr>

                            ))}

                        </tbody>

                    </table>

                )}

            </div>

        </div>
    );
};

export default Attendance;