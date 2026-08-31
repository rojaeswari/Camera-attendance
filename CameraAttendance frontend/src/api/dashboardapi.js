import axios from "axios";

const API_URL = "https://localhost:7035/api/Dashboard";

export const getDashboard = async () => {
    const response = await axios.get(API_URL);
    return response.data;
};