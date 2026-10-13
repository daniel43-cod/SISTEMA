import { PublicCategoriesPage } from '../features/public-catalog/pages/PublicCategoriesPage'
import { StaffLoginPage, useAuth } from '../features/auth'
import { AppProviders } from './providers/AppProviders'
import { StaffLayout } from './layouts/StaffLayout'

function AppContent() {
  const { session } = useAuth()
  return session ? <StaffLayout /> : <StaffLoginPage />
}
export default function App() {
  const catalogPath = `${import.meta.env.BASE_URL}catalogo`
  const currentPath = window.location.pathname.replace(/\/+$/, '')
  if (currentPath === catalogPath.replace(/\/+$/, '')) return <PublicCategoriesPage />
  return <AppProviders><AppContent /></AppProviders>
}