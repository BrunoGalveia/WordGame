import { useCallback, useEffect, useRef, useState } from 'react'
import * as signalR from '@microsoft/signalr'
import { API_BASE_URL } from './api'
import type { JoinRoomResult, PlayerSummary, WordAssignment } from './gameTypes'

export type ConnectionStatus = 'connecting' | 'connected' | 'error' | 'disconnected'

interface UseGameConnectionResult {
  status: ConnectionStatus
  error: string | null
  isHost: boolean
  players: PlayerSummary[]
  assignment: WordAssignment | null
  requestNewWord: () => Promise<void>
}

export function useGameConnection(
  roomCode: string,
  nickname: string,
  playerId: string,
): UseGameConnectionResult {
  const [status, setStatus] = useState<ConnectionStatus>('connecting')
  const [error, setError] = useState<string | null>(null)
  const [isHost, setIsHost] = useState(false)
  const [players, setPlayers] = useState<PlayerSummary[]>([])
  const [assignment, setAssignment] = useState<WordAssignment | null>(null)
  const connectionRef = useRef<signalR.HubConnection | null>(null)

  useEffect(() => {
    let cancelled = false
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/game`)
      .withAutomaticReconnect()
      .build()

    connectionRef.current = connection

    connection.on('PlayerListUpdated', (updated: PlayerSummary[]) => {
      setPlayers(updated)
    })

    connection.on('YourAssignment', (updated: WordAssignment) => {
      setAssignment(updated)
    })

    async function joinRoom() {
      const result: JoinRoomResult = await connection.invoke('JoinRoom', roomCode, nickname, playerId)
      if (cancelled) return
      setIsHost(result.isHost)
      setPlayers(result.players)
      setAssignment(result.currentAssignment)
    }

    connection.onreconnecting(() => setStatus('connecting'))
    connection.onreconnected(() => {
      // The transport reconnected with a brand new server-side connection id,
      // so the server has forgotten this player until we JoinRoom again.
      joinRoom()
        .then(() => {
          if (!cancelled) setStatus('connected')
        })
        .catch((err) => {
          if (cancelled) return
          setError(err instanceof Error ? err.message : 'Falha ao reentrar na sala.')
          setStatus('error')
        })
    })
    connection.onclose(() => setStatus('disconnected'))

    async function start() {
      try {
        await connection.start()
        await joinRoom()
        if (cancelled) return
        setStatus('connected')
      } catch (err) {
        if (cancelled) return
        setError(err instanceof Error ? err.message : 'Falha ao ligar à sala.')
        setStatus('error')
      }
    }

    start()

    return () => {
      cancelled = true
      connection.stop()
    }
  }, [roomCode, nickname, playerId])

  const requestNewWord = useCallback(async () => {
    if (!connectionRef.current || connectionRef.current.state !== signalR.HubConnectionState.Connected) {
      return
    }
    try {
      await connectionRef.current.invoke('RequestNewWord', roomCode)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao pedir nova palavra.')
    }
  }, [roomCode])

  return { status, error, isHost, players, assignment, requestNewWord }
}
