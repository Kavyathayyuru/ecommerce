import axios from "axios";

export const API_URL = "https://localhost:7070/api";

export const api = axios.create({
    baseURL: API_URL,
    headers: { "Content-Type": "application/json" },
});