import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'

import ProtectedRoute from '@/components/guards/ProtectedRoute'
import AdminLayout from '@/components/layouts/AdminLayout'
import AgentLayout from '@/components/layouts/AgentLayout'

import LoginPage from '@/pages/shared/LoginPage'

import ListingsPage from '@/pages/admin/ListingsPage'
import CreateListingPage from '@/pages/admin/CreateListingPage'
import EditListingPage from '@/pages/admin/EditListingPage'
import MediaUploadPage from '@/pages/admin/MediaUploadPage'

import MyListingsPage from '@/pages/agent/MyListingsPage'
import SelectMediaPage from '@/pages/agent/SelectMediaPage'
import EditDisplayPage from '@/pages/agent/EditDisplayPage'

import PreviewPage from '@/pages/shared/PreviewPage'
import PropertyWebsitePage from '@/pages/public/PropertyWebsitePage'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/listing/:token/view" element={<PropertyWebsitePage />} />

        {/* Admin routes - protected */}
        <Route element={<ProtectedRoute allowedRole="Admin" />}>
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<Navigate to="/admin/listings" replace />} />
            <Route path="listings" element={<ListingsPage />} />
            <Route path="listings/create" element={<CreateListingPage />} />
            <Route path="listings/:id/edit" element={<EditListingPage />} />
            <Route path="listings/:id/media" element={<MediaUploadPage />} />
            <Route path="listings/:id/preview" element={<PreviewPage />} />
          </Route>
        </Route>

        {/* Agent routes - protected */}
        <Route element={<ProtectedRoute allowedRole="Agent" />}>
          <Route path="/agent" element={<AgentLayout />}>
            <Route index element={<Navigate to="/agent/listings" replace />} />
            <Route path="listings" element={<MyListingsPage />} />
            <Route path="listings/:id/select-media" element={<SelectMediaPage />} />
            <Route path="listings/:id/edit-display" element={<EditDisplayPage />} />
            <Route path="listings/:id/preview" element={<PreviewPage />} />
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  )
}
