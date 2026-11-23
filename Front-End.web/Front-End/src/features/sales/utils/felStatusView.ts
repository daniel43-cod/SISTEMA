import type { FelListStatus } from '../api/felApi.ts'

export function felStatusView(status: FelListStatus | undefined, loading = false): { label: string; tone: string } {
  if (!status) return { label: loading ? 'Consultando FEL…' : 'FEL no disponible', tone: 'unknown' }
  switch (status.estado) {
    case 'CERTIFICADA': return { label: status.ambiente === 'PRUEBAS' ? 'Certificada · prueba' : 'Certificada', tone: 'certified' }
    case 'RECHAZADA': return { label: 'Rechazada', tone: 'rejected' }
    case 'EN_PROCESO': return { label: 'En proceso', tone: 'processing' }
    case 'POR_CONFIRMAR': return { label: 'Por confirmar', tone: 'pending' }
    default: return { label: 'Sin certificar', tone: 'not-certified' }
  }
}
