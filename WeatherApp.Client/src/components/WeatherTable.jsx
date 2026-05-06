/**
 * WeatherTable — displays weather entries in a sortable table.
 * Props:
 *   entries      — array of WeatherEntry objects from the API
 *   sortField    — currently active sort field key
 *   sortDir      — 'asc' | 'desc'
 *   onSort       — callback(field)
 *   onRowClick   — callback(entry)
 *   selectedDate — iso date string of the currently selected row (or null)
 */
export default function WeatherTable({ entries, sortField, sortDir, onSort, onRowClick, selectedDate }) {
  const arrow = (field) => {
    if (sortField !== field) return <span className="sort-arrow">⇅</span>
    return <span className="sort-arrow">{sortDir === 'asc' ? '↑' : '↓'}</span>
  }

  const thClass = (field) =>
    `sortable${sortField === field ? ' active-sort' : ''}`

  const fmt = (val, decimals = 1) =>
    val != null ? (
      <span className="temp-value">{Number(val).toFixed(decimals)}</span>
    ) : (
      <span className="na-value">—</span>
    )

  if (entries.length === 0) {
    return (
      <div className="state-box" style={{ background: '#fff', border: '1px solid #e2e8f0' }}>
        No entries to display.
      </div>
    )
  }

  return (
    <div className="weather-table-wrapper">
      <table className="weather-table" aria-label="Historical weather data">
        <thead>
          <tr>
            <th
              className={thClass('date')}
              onClick={() => onSort('date')}
              aria-sort={sortField === 'date' ? (sortDir === 'asc' ? 'ascending' : 'descending') : 'none'}
            >
              Date {arrow('date')}
            </th>
            <th
              className={thClass('minTemp')}
              onClick={() => onSort('minTemp')}
              aria-sort={sortField === 'minTemp' ? (sortDir === 'asc' ? 'ascending' : 'descending') : 'none'}
            >
              Min Temp (°C) {arrow('minTemp')}
            </th>
            <th
              className={thClass('maxTemp')}
              onClick={() => onSort('maxTemp')}
              aria-sort={sortField === 'maxTemp' ? (sortDir === 'asc' ? 'ascending' : 'descending') : 'none'}
            >
              Max Temp (°C) {arrow('maxTemp')}
            </th>
            <th
              className={thClass('precipitation')}
              onClick={() => onSort('precipitation')}
              aria-sort={sortField === 'precipitation' ? (sortDir === 'asc' ? 'ascending' : 'descending') : 'none'}
            >
              Precipitation (mm) {arrow('precipitation')}
            </th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {entries.map((entry, i) => (
            <tr
              key={entry.date ?? `invalid-${i}`}
              onClick={() => onRowClick(entry)}
              className={[
                entry.date === selectedDate ? 'selected-row' : '',
                !entry.isSuccess ? 'error-row' : '',
              ].join(' ').trim()}
              title="Click to view details"
              tabIndex={0}
              onKeyDown={e => e.key === 'Enter' && onRowClick(entry)}
              aria-selected={entry.date === selectedDate}
            >
              <td>{entry.date ?? <span className="na-value">{entry.rawDate}</span>}</td>
              <td>{fmt(entry.minTemperatureCelsius)}</td>
              <td>{fmt(entry.maxTemperatureCelsius)}</td>
              <td>{fmt(entry.precipitationMm)}</td>
              <td>
                {entry.isSuccess
                  ? <span className="badge-success">✓ OK</span>
                  : <span className="badge-error">✗ Error</span>
                }
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
