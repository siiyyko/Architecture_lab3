import axios from 'axios';

const API_URL = 'http://localhost:5000';

const apiClient = axios.create({
  baseURL: API_URL,
});

apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// === UserService ===

export const registerUser = (userData) => {
  return apiClient.post('/api/auth/register', userData);
};

export const loginUser = (credentials) => {
  return apiClient.post('/api/auth/login', credentials);
};

export const getUsers = () => {
  return apiClient.get('/api/users');
};

// === TaskService ===
export const getTasks = () => {
  return apiClient.get('/api/tasks');
};

export const updateTaskStatus = (taskId, newStatus) => {
  return apiClient.patch(`/api/tasks/${taskId}/status`, { newStatus });
};

export const updateTask = (taskId, taskData) => {
  return apiClient.put(`/api/tasks/${taskId}`, taskData);
};

export const createTask = (taskData) => {
  return apiClient.post('/api/tasks', taskData);
};

// === CommunicationService ===
export const getCommentsForTask = (taskId) => {
  return apiClient.get(`/api/tasks/${taskId}/comments`);
};

export const postCommentAsync = (commentData) => {
  return apiClient.post('/api/comments/async', commentData);
};

export const postCommentSync = (commentData) => {
  return apiClient.post('/api/comments/sync', commentData);
};

export default apiClient;