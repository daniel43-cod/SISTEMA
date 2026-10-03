import './controls.css'

type RefreshButtonProps = {
  onClick: () => void
  loading: boolean
  label: string
}

// Comparte el ícono y la accesibilidad; cada listado proporciona su propia consulta.
export function RefreshButton({ onClick, loading, label }: RefreshButtonProps) {
  return <button type="button" className="refresh-button" onClick={onClick}
    disabled={loading} aria-label={label} title={label}>
    <svg width="25" height="25" viewBox="0 0 24 24" fill="none"
      stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M20 7v5h-5" />
      <path d="M20 12a8 8 0 1 0-2.3 5.7" />
    </svg>
  </button>
}
