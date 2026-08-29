import axios from "axios";

const API_URL = "https://localhost:7035/api/Auth";

export const registerUser = async (data) => {
    const response = await axios.post(
        `${API_URL}/register`,
        data
    );

    return response.data;
};

export const loginApi = async (email, password) => {
    const response = await axios.post(
        `${API_URL}/login`,
        null,
        {
            params: {
                email,
                password
            }
        }
    );

    return response.data;
};