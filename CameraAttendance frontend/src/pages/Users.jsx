
import { useEffect, useState } from "react";
import { getUsers } from "../api/usersapi";
import "./Users.css";

const Users = () => {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);

    const loadUsers = async () => {
        try {
            const response = await getUsers();

            if (response.success) {
                setUsers(response.data);
            }
        } catch (error) {
            console.error("Users loading error:", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadUsers();
    }, []);

    return (
        <div className="users-page">

            <div className="users-header">
                <div>
                    <h1>Users</h1>
                    <p>Manage registered users</p>
                </div>

                <button className="add-user-btn">
                    + Add User
                </button>
            </div>

            <div className="users-card">

                {loading ? (
                    <p className="loading">Loading users...</p>
                ) : users.length === 0 ? (
                    <p className="no-users">No users found</p>
                ) : (
                    <table>
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Name</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Status</th>
                            </tr>
                        </thead>

                        <tbody>
                            {users.map((user) => (
                                <tr key={user.id}>
                                    <td>{user.id}</td>
                                    <td>{user.name}</td>
                                    <td>{user.email}</td>
                                    <td>{user.roleId}</td>
                                    <td>
                                        <span
                                            className={
                                                user.isActive
                                                    ? "status active"
                                                    : "status inactive"
                                            }
                                        >
                                            {user.isActive
                                                ? "Active"
                                                : "Inactive"}
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

export default Users;
