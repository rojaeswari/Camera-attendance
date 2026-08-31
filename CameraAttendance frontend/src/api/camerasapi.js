import axios from "axios";

const API_URL = "https://localhost:7035/api/Cameras";

export const getCameras = async () => {
    const response = await axios.get(API_URL);
    return response.data;
};

export const addCamera = async (cameraData) => {
    const response = await axios.post(API_URL, cameraData);
    return response.data;
};

export const updateCamera = async (id, cameraData) => {
    const response = await axios.put(
        `${API_URL}/${id}`,
        cameraData
    );

    return response.data;
};

export const deleteCamera = async (id) => {
    const response = await axios.delete(
        `${API_URL}/${id}`
    );

    return response.data;
};