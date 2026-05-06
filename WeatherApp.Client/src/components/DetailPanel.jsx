import { useEffect } from 'react'

/**
 * DetailPanel — slide-in side panel showing full details for a selected weather entry.
 * Closes on Escape key or overlay click.
 */
export default function DetailPanel({ entry, onClose }) {
  // Close on Escape
  useEffect(() => {
    const handler = (e) => { if (e.key === 'Escape') onClose() }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [onClose])

  const fmt = (val, decimals = 1, fallback = 'N/A') =>
    val != null ? Number(val).toFixed(decimals) : fallback

  return (
    <div
      className="detail-overlay"
      onClick={e => { if (e.target === e.currentTarget) onClose() }}
      role="dialog"
      aria-modal="true"
      aria-label={`Weather details for ${entry.date ?? entry.rawDate}`}
    >
      <div className="detail-panel">
        <button className="btn-close" onClick={onClose} aria-label="Close detail panel">✕ Close</button>

        <h2>📅 {entry.date ?? 'Invalid Date'}</h2>
        <p className="detail-raw">Original: <em>{entry.rawDate}</em></p>

        {entry.isSuccess ? (
          <div className="detail-grid">
            <div className="detail-card">
              <div className="label">Min Temp</div>
              <div className="value">
                {fmt(entry.minTemperatureCelsius)}
                <span className="unit">°C</span>
              </div>
            </div>
            <div className="detail-card">
              <div className="label">Max Temp</div>
              <div className="value">
                {fmt(entry.maxTemperatureCelsius)}
                <span className="unit">°C</span>
              </div>
            </div>
            <div className="detail-card" style={{ gridColumn: '1 / -1' }}>
              <div className="label">Precipitation</div>
              <div className="value">
                {fmt(entry.precipitationMm)}
                <span className="unit">mm</span>
              </div>
            </div>
          </div>
        ) : (
          <div className="detail-error-msg">
            <strong>⚠️ Could not retrieve data</strong><br />
            {entry.errorMessage}
          </div>
        )}
      </div>
    </div>
  )
}
