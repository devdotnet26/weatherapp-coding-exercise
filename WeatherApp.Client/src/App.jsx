import { useState, useEffect, useCallback } from 'react'
import WeatherTable from './components/WeatherTable.jsx'
import DetailPanel from './components/DetailPanel.jsx'
import './App.css'

const API_URL = '/api/weather'

export default function App() {
  const [entries, setEntries]     = useState([])
  const [loading, setLoading]     = useState(false)
  const [error, setError]         = useState(null)
  const [selected, setSelected]   = useState(null)

  // Sorting state
  const [sortField, setSortField] = useState('date')
  const [sortDir, setSortDir]     = useState('asc')

  // Filter state
  const [filterMinTemp, setFilterMinTemp] = useState('')

  const fetchWeather = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await fetch(API_URL)
      if (!res.ok) {
        throw new Error(`Server returned ${res.status} ${res.statusText}`)
      }
      const data = await res.json()
      setEntries(data)
    } catch (err) {
      setError(err.message || 'Failed to load weather data.')
    } finally {
      setLoading(false)
    }
  }, [])

  // Fetch on mount
  useEffect(() => {
    fetchWeather()
  }, [fetchWeather])

  // Derived: sorted + filtered list
  const displayEntries = [...entries]
    .filter(e => {
      if (filterMinTemp === '') return true
      const threshold = parseFloat(filterMinTemp)
      if (isNaN(threshold)) return true
      return e.isSuccess && e.minTemperatureCelsius != null && e.minTemperatureCelsius >= threshold
    })
    .sort((a, b) => {
      let aVal, bVal
      if (sortField === 'date') {
        aVal = a.date ?? ''
        bVal = b.date ?? ''
      } else if (sortField === 'minTemp') {
        aVal = a.minTemperatureCelsius ?? -Infinity
        bVal = b.minTemperatureCelsius ?? -Infinity
      } else if (sortField === 'maxTemp') {
        aVal = a.maxTemperatureCelsius ?? -Infinity
        bVal = b.maxTemperatureCelsius ?? -Infinity
      } else if (sortField === 'precipitation') {
        aVal = a.precipitationMm ?? -Infinity
        bVal = b.precipitationMm ?? -Infinity
      }
      if (aVal < bVal) return sortDir === 'asc' ? -1 : 1
      if (aVal > bVal) return sortDir === 'asc' ? 1 : -1
      return 0
    })

  const handleSort = (field) => {
    if (field === sortField) {
      setSortDir(d => d === 'asc' ? 'desc' : 'asc')
    } else {
      setSortField(field)
      setSortDir('asc')
    }
  }

  return (
    <div className="app">
      <header className="app-header">
        <div className="header-inner">
          <h1>🌤️ Dallas Historical Weather</h1>
          <p className="subtitle">Daily weather data for Dallas, TX · Open-Meteo Historical API</p>
        </div>
      </header>

      <main className="app-main">
        {/* Controls bar */}
        <div className="controls-bar">
          <div className="filter-group">
            <label htmlFor="minTempFilter">Min Temp ≥ (°C)</label>
            <input
              id="minTempFilter"
              type="number"
              placeholder="e.g. 10"
              value={filterMinTemp}
              onChange={e => setFilterMinTemp(e.target.value)}
            />
            {filterMinTemp && (
              <button className="btn-clear" onClick={() => setFilterMinTemp('')} title="Clear filter">✕</button>
            )}
          </div>
          <button className="btn-refresh" onClick={fetchWeather} disabled={loading}>
            {loading ? 'Refreshing…' : '↺ Refresh'}
          </button>
        </div>

        {/* Loading state */}
        {loading && (
          <div className="state-box loading-box" role="status" aria-live="polite">
            <div className="spinner" aria-hidden="true" />
            <span>Fetching weather data…</span>
          </div>
        )}

        {/* Error state */}
        {!loading && error && (
          <div className="state-box error-box" role="alert">
            <strong>⚠️ Error:</strong> {error}
            <button className="btn-retry" onClick={fetchWeather}>Retry</button>
          </div>
        )}

        {/* Data */}
        {!loading && !error && (
          <>
            <div className="result-count">
              {displayEntries.length} result{displayEntries.length !== 1 ? 's' : ''}
              {filterMinTemp ? ` (filtered from ${entries.length})` : ''}
            </div>
            <WeatherTable
              entries={displayEntries}
              sortField={sortField}
              sortDir={sortDir}
              onSort={handleSort}
              onRowClick={setSelected}
              selectedDate={selected?.date}
            />
          </>
        )}
      </main>

      {/* Detail panel (modal-like slide-in) */}
      {selected && (
        <DetailPanel entry={selected} onClose={() => setSelected(null)} />
      )}
    </div>
  )
}
