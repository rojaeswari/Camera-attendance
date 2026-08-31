import { useEffect, useState } from "react";
import {
    getCameras,
    addCamera,
    updateCamera,
    deleteCamera
} from "../api/camerasapi";

import "./Cameras.css";

const Cameras = () => {

    const [cameras, setCameras] = useState([]);
    const [loading, setLoading] = useState(true);

    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);

    const [formData, setFormData] = useState({
        id: null,
        cameraName: "",
        ipAddress: "",
        location: "",
        status: "Active"
    });


    // =========================
    // LOAD CAMERAS
    // =========================

    const loadCameras = async () => {

        try {

            const response = await getCameras();

            if (response.success) {
                setCameras(response.data);
            }

        } catch (error) {

            console.error("Camera loading error:", error);

        } finally {

            setLoading(false);

        }
    };


    useEffect(() => {
        loadCameras();
    }, []);


    // =========================
    // INPUT CHANGE
    // =========================

    const handleChange = (e) => {

        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });

    };


    // =========================
    // ADD CAMERA
    // =========================

    const handleAddCamera = () => {

        setFormData({
            id: null,
            cameraName: "",
            ipAddress: "",
            location: "",
            status: "Active"
        });

        setIsEdit(false);
        setShowModal(true);

    };


    // =========================
    // EDIT CAMERA
    // =========================

    const handleEditCamera = (camera) => {

        setFormData({
            id: camera.id,
            cameraName: camera.cameraName,
            ipAddress: camera.ipAddress,
            location: camera.location || "",
            status: camera.status
        });

        setIsEdit(true);
        setShowModal(true);

    };


    // =========================
    // CLOSE MODAL
    // =========================

    const handleCloseModal = () => {
        setShowModal(false);
    };


    // =========================
    // SAVE CAMERA
    // =========================

    const handleSubmit = async (e) => {

        e.preventDefault();

        try {

            const cameraData = {
                cameraName: formData.cameraName,
                ipAddress: formData.ipAddress,
                location: formData.location,
                status: formData.status
            };

            let response;

            if (isEdit) {

                response = await updateCamera(
                    formData.id,
                    cameraData
                );

            } else {

                response = await addCamera(cameraData);

            }


            if (response.success) {

                alert(
                    isEdit
                        ? "Camera updated successfully"
                        : "Camera added successfully"
                );

                setShowModal(false);

                setIsEdit(false);

                loadCameras();

            } else {

                alert(
                    response.message ||
                    "Operation failed"
                );

            }

        } catch (error) {

            console.error("Camera save error:", error);

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


    // =========================
    // DELETE CAMERA
    // =========================

    const handleDeleteCamera = async (id, name) => {

        const confirmDelete = window.confirm(
            `Are you sure you want to delete ${name}?`
        );

        if (!confirmDelete) {
            return;
        }

        try {

            const response = await deleteCamera(id);

            if (response.success) {

                alert("Camera deleted successfully");

                loadCameras();

            } else {

                alert(
                    response.message ||
                    "Failed to delete camera"
                );

            }

        } catch (error) {

            console.error("Delete camera error:", error);

            alert("Unable to connect to server");

        }
    };


    return (

        <div className="cameras-page">

            {/* ================= HEADER ================= */}

            <div className="cameras-header">

                <div>

                    <h1>Cameras</h1>

                    <p>
                        Manage CCTV cameras
                    </p>

                </div>


                <button
                    className="add-camera-btn"
                    onClick={handleAddCamera}
                >
                    + Add Camera
                </button>

            </div>


            {/* ================= CAMERA TABLE ================= */}

            <div className="cameras-card">

                {loading ? (

                    <p className="loading">
                        Loading cameras...
                    </p>

                ) : cameras.length === 0 ? (

                    <p className="no-cameras">
                        No cameras found
                    </p>

                ) : (

                    <table>

                        <thead>

                            <tr>

                                <th>ID</th>

                                <th>Camera Name</th>

                                <th>IP Address</th>

                                <th>Location</th>

                                <th>Status</th>

                                <th>Actions</th>

                            </tr>

                        </thead>


                        <tbody>

                            {cameras.map((camera) => (

                                <tr key={camera.id}>

                                    <td>
                                        {camera.id}
                                    </td>

                                    <td className="camera-name">
                                        {camera.cameraName}
                                    </td>

                                    <td>
                                        {camera.ipAddress}
                                    </td>

                                    <td>
                                        {camera.location || "N/A"}
                                    </td>

                                    <td>

                                        <span
                                            className={
                                                camera.status === "Active"
                                                    ? "camera-status active"
                                                    : "camera-status inactive"
                                            }
                                        >
                                            {camera.status}
                                        </span>

                                    </td>

                                    <td>

                                        <button
                                            className="edit-btn"
                                            onClick={() =>
                                                handleEditCamera(camera)
                                            }
                                        >
                                            Edit
                                        </button>


                                        <button
                                            className="delete-btn"
                                            onClick={() =>
                                                handleDeleteCamera(
                                                    camera.id,
                                                    camera.cameraName
                                                )
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


            {/* ================= MODAL ================= */}

            {showModal && (

                <div
                    className="modal-overlay"
                    onClick={handleCloseModal}
                >

                    <div
                        className="camera-modal"
                        onClick={(e) =>
                            e.stopPropagation()
                        }
                    >

                        {/* HEADER */}

                        <div className="modal-header">

                            <div>

                                <h2>
                                    {isEdit
                                        ? "Edit Camera"
                                        : "Add Camera"}
                                </h2>

                                <p>
                                    {isEdit
                                        ? "Update camera details"
                                        : "Add a new CCTV camera"}
                                </p>

                            </div>


                            <button
                                className="close-btn"
                                onClick={handleCloseModal}
                            >
                                ×
                            </button>

                        </div>


                        {/* FORM */}

                        <form
                            className="camera-form"
                            onSubmit={handleSubmit}
                        >

                            <div className="form-group">

                                <label>
                                    Camera Name
                                </label>

                                <input
                                    type="text"
                                    name="cameraName"
                                    placeholder="Example: Indoor Camera"
                                    value={formData.cameraName}
                                    onChange={handleChange}
                                    required
                                />

                            </div>


                            <div className="form-group">

                                <label>
                                    IP Address
                                </label>

                                <input
                                    type="text"
                                    name="ipAddress"
                                    placeholder="Example: 192.168.1.101"
                                    value={formData.ipAddress}
                                    onChange={handleChange}
                                    required
                                />

                            </div>


                            <div className="form-group">

                                <label>
                                    Location
                                </label>

                                <input
                                    type="text"
                                    name="location"
                                    placeholder="Example: Main Gate"
                                    value={formData.location}
                                    onChange={handleChange}
                                />

                            </div>


                            <div className="form-group">

                                <label>
                                    Status
                                </label>

                                <select
                                    name="status"
                                    value={formData.status}
                                    onChange={handleChange}
                                >

                                    <option value="Active">
                                        Active
                                    </option>

                                    <option value="Inactive">
                                        Inactive
                                    </option>

                                </select>

                            </div>


                            {/* BUTTONS */}

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
                                    className="save-camera-btn"
                                >
                                    {isEdit
                                        ? "Update Camera"
                                        : "Save Camera"}
                                </button>

                            </div>

                        </form>

                    </div>

                </div>

            )}

        </div>
    );
};

export default Cameras;