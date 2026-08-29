import axios from "axios";

const API_URL = "https://localhost:7035/api/Users";

export const getUsers = async () => {
    const response = await axios.get(API_URL);
    return response.data;
};