import { useEffect, useState } from "react";
import { getUsers, addUser, getFaceImageUrl, deleteUser, updateUser } from "../api/usersapi";
import "./Users.css";

const Users = () => {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const getRoleName = (roleId) => {
        if (roleId === 1) return "Admin";
        if (roleId === 2) return "User";
        return "Unknown";
    };

    // Modal open / close
    const [showModal, setShowModal] = useState(false);

    // Form data
    const [formData, setFormData] = useState({
        name: "",
        email: "",
        password: "",
        roleId: "2",
        faceImage: null
    });
    const [isEdit, setIsEdit] = useState(false);

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

    // Input change
    const handleChange = (e) => {

        if (e.target.name === "faceImage") {

            setFormData({
                ...formData,
                faceImage: e.target.files[0]
            });

        } else {

            setFormData({
                ...formData,
                [e.target.name]: e.target.value
            });

        }
    };

    // Add User button
    const handleAddUser = () => {

        setFormData({
            name: "",
            email: "",
            password: "",
            roleId: "2",
            faceImage: null
        });

        setIsEdit(false);
        setShowModal(true);
    };

    const handleEditUser = (user) => {

        setFormData({
            id: user.id,
            name: user.name,
            email: user.email,
            password: "",
            roleId: String(user.roleId),
            faceImage: null
        });

        setIsEdit(true);
        setShowModal(true);
    };

    // Close modal
    const handleCloseModal = () => {
        setShowModal(false);
    };



    const handleDeleteUser = async (id, name) => {
        const confirmDelete = window.confirm(
            `Are you sure you want to delete ${name}?`
        );

        if (!confirmDelete) {
            return;
        }

        try {
            const response = await deleteUser(id);

            if (response.success) {
                alert("User deleted successfully");

                loadUsers();
            } else {
                alert(response.message || "Failed to delete user");
            }
        } catch (error) {
            console.error("Delete user error:", error);

            if (error.response) {
                alert(
                    error.response.data?.message ||
                    "Failed to delete user"
                );
            } else {
                alert("Unable to connect to server");
            }
        }
    };

    // Save user
    const handleSubmit = async (e) => {

        e.preventDefault();

        try {

            const userData = new FormData();

            userData.append("Name", formData.name);
            userData.append("Email", formData.email);
            userData.append("RoleId", formData.roleId);

            // Password only when entered
            if (formData.password.trim() !== "") {
                userData.append("Password", formData.password);
            }

            // Photo only when selected
            if (formData.faceImage) {
                userData.append("FaceImage", formData.faceImage);
            }

            let response;

            if (isEdit) {

                response = await updateUser(
                    formData.id,
                    userData
                );

            } else {

                if (!formData.password.trim()) {
                    alert("Password is required");
                    return;
                }

                response = await addUser(userData);
            }

            if (response.success) {

                alert(
                    isEdit
                        ? "User updated successfully"
                        : "User added successfully"
                );

                setShowModal(false);
                setIsEdit(false);

                loadUsers();

            } else {

                alert(
                    response.message ||
                    "Operation failed"
                );
            }

        } catch (error) {

            console.error(
                isEdit
                    ? "Update user error:"
                    : "Add user error:",
                error
            );

            if (error.response) {

                alert(
                    error.response.data?.message ||
                    "Operation failed"
                );

            } else {

                alert("Unable to connect to server");
            }
        }
    };
    return (
        <div className="users-page">

            {/* ================= HEADER ================= */}

            <div className="users-header">

                <div>
                    <h1>Users</h1>
                    <p>Manage registered users</p>
                </div>

                <button
                    className="add-user-btn"
                    onClick={handleAddUser}
                >
                    + Add User
                </button>

            </div>


            {/* ================= USERS TABLE ================= */}

            <div className="users-card">

                {loading ? (
                    <p className="loading">
                        Loading users...
                    </p>

                ) : users.length === 0 ? (

                    <p className="no-users">
                        No users found
                    </p>

                ) : (

                    <table>

                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Name</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Face</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>

                        <tbody>

                            {users.map((user) => (

                                <tr key={user.id}>

                                    <td>{user.id}</td>

                                    <td>{user.name}</td>

                                    <td>{user.email}</td>

                                    <td>{getRoleName(user.roleId)}</td>

                                    {/* FACE */}
                                    <td>
                                        {user.faceImagePath ? (
                                            <img
                                                src={getFaceImageUrl(user.faceImagePath)}
                                                alt={user.name}
                                                className="user-face-image"
                                            />
                                        ) : (
                                            <span>No Image</span>
                                        )}
                                    </td>

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


                                    <td className="actions-cell">

                                        <button
                                            className="edit-btn"
                                            onClick={() => handleEditUser(user)}
                                        >
                                            Edit
                                        </button>

                                        <button
                                            className="delete-btn"
                                            onClick={() =>
                                                handleDeleteUser(user.id, user.name)
                                            }
                                        >
                                            Delete
                                        </button>

                                    </td>

                                </tr>

                            ))}

                        </tbody>

                    </table>

                )}

            </div>


            {/* ================= ADD USER MODAL ================= */}

            {showModal && (

                <div
                    className="modal-overlay"
                    onClick={handleCloseModal}
                >

                    <div
                        className="add-user-modal"
                        onClick={(e) => e.stopPropagation()}
                    >

                        {/* ============================
    MODAL HEADER
============================ */}

                        <div className="modal-header">

                            <div>
                                <h2>
                                    {isEdit ? "Edit User" : "Add User"}
                                </h2>

                                <p>
                                    {isEdit
                                        ? "Update user details"
                                        : "Create a new user account"}
                                </p>
                            </div>

                            <button
                                type="button"
                                className="close-btn"
                                onClick={handleCloseModal}
                            >
                                ×
                            </button>

                        </div>


                        {/* ============================
    FORM
============================ */}

                        <form
                            className="add-user-form"
                            onSubmit={handleSubmit}
                        >

                            {/* ============================
        NAME
    ============================ */}

                            <div className="form-group">

                                <label>Name</label>

                                <input
                                    type="text"
                                    name="name"
                                    placeholder="Enter user name"
                                    value={formData.name}
                                    onChange={handleChange}
                                    required
                                />

                            </div>


                            {/* ============================
        EMAIL
    ============================ */}

                            <div className="form-group">

                                <label>Email</label>

                                <input
                                    type="email"
                                    name="email"
                                    placeholder="Enter email address"
                                    value={formData.email}
                                    onChange={handleChange}
                                    required
                                />

                            </div>


                            {/* ============================
        PASSWORD
    ============================ */}

                            <div className="form-group">

                                <label>
                                    Password
                                </label>

                                <input
                                    type="password"
                                    name="password"
                                    placeholder={
                                        isEdit
                                            ? "Leave blank to keep current password"
                                            : "Enter password"
                                    }
                                    value={formData.password}
                                    onChange={handleChange}
                                    required={!isEdit}
                                />

                                {isEdit && (
                                    <small>
                                        Leave blank if you don't want to change the password
                                    </small>
                                )}

                            </div>


                            {/* ============================
        ROLE
    ============================ */}

                            <div className="form-group">

                                <label>Role</label>

                                <select
                                    name="roleId"
                                    value={formData.roleId}
                                    onChange={handleChange}
                                >

                                    <option value="1">
                                        Admin
                                    </option>

                                    <option value="2">
                                        User
                                    </option>

                                </select>

                            </div>


                            {/* ============================
        FACE IMAGE
    ============================ */}

                            <div className="form-group">

                                <label>
                                    Face Image
                                </label>

                                <input
                                    type="file"
                                    name="faceImage"
                                    accept="image/*"
                                    onChange={handleChange}
                                />

                                <small>
                                    {isEdit
                                        ? "Choose a new photo only if you want to change the existing photo"
                                        : "Upload a clear front-face photo"}
                                </small>

                            </div>


                            {/* ============================
        BUTTONS
    ============================ */}

                            <div className="modal-actions">

                                <button
                                    type="button"
                                    className="cancel-btn"
                                    onClick={handleCloseModal}
                                >
                                    Cancel
                                </button>

                                <button
                                    type="submit"
                                    className="save-user-btn"
                                >
                                    {isEdit
                                        ? "Update User"
                                        : "Save User"}
                                </button>

                            </div>

                        </form>

                    </div>

                </div>

            )}

        </div>
    );
};

export default Users;