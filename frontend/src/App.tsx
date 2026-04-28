import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useEffect, useMemo, useRef, useState } from 'react'
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import './App.css'

type TelemetryDto = {
  id: string
  hiveId: string
  timestamp: string
  weight: number
  temperature: number
  humidity: number
  batteryLevel: number
}

function App() {
  const apiBase =
    import.meta.env.VITE_API_BASE_URL?.toString() ?? 'http://localhost:5099'

  const [hiveId, setHiveId] = useState('hive-1')
  const [items, setItems] = useState<TelemetryDto[]>([])
  const [status, setStatus] = useState<'idle' | 'loading' | 'ready' | 'error'>(
    'idle',
  )
  const [realtime, setRealtime] = useState<'disconnected' | 'connected'>(
    'disconnected',
  )
  const [error, setError] = useState<string | null>(null)

  const chartData = useMemo(
    () =>
      items.map((x) => ({
        ...x,
        time: new Date(x.timestamp).toLocaleTimeString(),
      })),
    [items],
  )

  async function loadTelemetry(targetHiveId: string) {
    setStatus('loading')
    setError(null)
    try {
      const res = await fetch(`${apiBase}/api/telemetry/${targetHiveId}`)
      if (!res.ok) throw new Error(`HTTP ${res.status}`)
      const data = (await res.json()) as TelemetryDto[]
      setItems(data)
      setStatus('ready')
    } catch (e) {
      setStatus('error')
      setError(e instanceof Error ? e.message : 'Failed to load telemetry')
    }
  }

  useEffect(() => {
    if (!hiveId) return
    void loadTelemetry(hiveId)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hiveId])

  useRealtimeTelemetry(
    apiBase,
    hiveId,
    (t) => {
      // Keep the chart live: append new points for the current hive.
      if (t.hiveId !== hiveId) return
      setItems((prev) => [...prev, t])
    },
    setRealtime,
    setError,
  )

  return (
    <div className="page">
      <header className="header">
        <div>
          <h1>Smart Apiary</h1>
          <p className="sub">
            Telemetry + realtime updates (SignalR) starter
          </p>
        </div>

        <div className="controls">
          <label className="field">
            <span>Hive</span>
            <input
              value={hiveId}
              onChange={(e) => setHiveId(e.target.value)}
              placeholder="e.g. hive-1"
            />
          </label>
          <button
            className="button"
            onClick={() => loadTelemetry(hiveId)}
            disabled={!hiveId || status === 'loading'}
          >
            Refresh
          </button>
        </div>
      </header>

      <section className="meta">
        <div className="pill">
          <strong>API</strong> <span>{apiBase}</span>
        </div>
        <div className="pill">
          <strong>Status</strong> <span>{status}</span>
        </div>
        <div className="pill">
          <strong>Realtime</strong> <span>{realtime}</span>
        </div>
        <div className="pill">
          <strong>Points</strong> <span>{items.length}</span>
        </div>
      </section>

      {error ? (
        <section className="error">
          <strong>Error:</strong> {error}
        </section>
      ) : null}

      <section className="card">
        <h2>Telemetry</h2>
        <div className="chart">
          <ResponsiveContainer width="100%" height={360}>
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" />
              <YAxis yAxisId="left" />
              <YAxis yAxisId="right" orientation="right" />
              <Tooltip />
              <Legend />
              <Line
                yAxisId="left"
                type="monotone"
                dataKey="temperature"
                name="Temperature"
                stroke="#ef4444"
                dot={false}
              />
              <Line
                yAxisId="left"
                type="monotone"
                dataKey="humidity"
                name="Humidity"
                stroke="#3b82f6"
                dot={false}
              />
              <Line
                yAxisId="right"
                type="monotone"
                dataKey="weight"
                name="Weight"
                stroke="#22c55e"
                dot={false}
              />
              <Line
                yAxisId="right"
                type="monotone"
                dataKey="batteryLevel"
                name="Battery"
                stroke="#a855f7"
                dot={false}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </section>
    </div>
  )
}

export default App

// Data fetch + SignalR hookup kept in the same file on purpose (starter project).
// If you expand the app, move this into `src/api/` and `src/realtime/`.
function useRealtimeTelemetry(
  apiBase: string,
  hiveId: string,
  onInserted: (t: TelemetryDto) => void,
  setRealtime: (s: 'disconnected' | 'connected') => void,
  setError: (s: string | null) => void,
) {
  const onInsertedRef = useRef(onInserted)
  onInsertedRef.current = onInserted

  useEffect(() => {
    if (!hiveId) return

    let cancelled = false

    async function connect() {
      setError(null)
      setRealtime('disconnected')

      const conn = new HubConnectionBuilder()
        .withUrl(`${apiBase}/hubs/telemetry`, { withCredentials: true })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build()

      conn.on('telemetryInserted', (t: TelemetryDto) => {
        onInsertedRef.current(t)
      })

      try {
        await conn.start()
        if (cancelled) return

        await conn.invoke('SubscribeHive', hiveId)
        setRealtime('connected')
      } catch (e) {
        setRealtime('disconnected')
        setError(e instanceof Error ? e.message : 'Realtime connection failed')
      }

      return conn
    }

    let currentConn: Awaited<ReturnType<typeof connect>> | undefined
    void connect().then((c) => (currentConn = c))

    return () => {
      cancelled = true
      void (async () => {
        try {
          if (currentConn) {
            await currentConn.invoke('UnsubscribeHive', hiveId)
            await currentConn.stop()
          }
        } catch {
          // Ignore cleanup errors (dev refreshes / disconnects).
        }
      })()
    }
  }, [apiBase, hiveId, setError, setRealtime])
}

