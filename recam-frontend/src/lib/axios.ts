import axios from 'axios'
import { useAuthStore } from '@/stores/authStore'

// Backend wraps every response as { succeed, data, message, errorMessage, errorCode }.
// These interceptors unwrap that envelope so callers just get the real payload back.
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
})

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => {
    const body = response.data
    if (!body.succeed) {
      return Promise.reject(new Error(body.errorMessage ?? 'Request failed'))
    }
    return body.data
  },
  (error) => {
    const message = error.response?.data?.errorMessage ?? error.message
    return Promise.reject(new Error(message))
  }
)
