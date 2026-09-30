import { StaffLoginPage, useAuth } from '../features/auth'
import { AppProviders } from './providers/AppProviders'
import { StaffLayout } from './layouts/StaffLayout'

function AppContent() {
  const { session } = useAuth()
  return session ? <StaffLayout /> : <StaffLoginPage />
}
export default function App() {
  return <AppProviders><AppContent /></AppProviders>
}