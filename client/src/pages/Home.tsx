import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { createRoom } from '../lib/api'
import { setStoredIdentity } from '../lib/roomStorage'

export default function Home() {
  const navigate = useNavigate()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleCreateRoom() {
    setLoading(true)
    setError(null)
    try {
      const { roomCode, hostPlayerId } = await createRoom()
      setStoredIdentity(roomCode, { playerId: hostPlayerId, nickname: '' })
      navigate(`/room/${roomCode}?setup=1`)
    } catch {
      setError('Não foi possível criar a sala. Tenta novamente.')
      setLoading(false)
    }
  }

  return (
    <main className="home">
      <h1>Palavra Secreta</h1>
      <p>Cria uma sala, partilha o link com os teus amigos e joga.</p>
      <button type="button" onClick={handleCreateRoom} disabled={loading}>
        {loading ? 'A criar sala…' : 'Criar Sala'}
      </button>
      {error && <p className="error">{error}</p>}
      <p className="fallback">
        Já tens um código de sala? Usa o link que te partilharam.
      </p>
      <JoinByCode />
    </main>
  )
}

function JoinByCode() {
  const navigate = useNavigate()
  const [code, setCode] = useState('')

  function handleJoin(e: React.FormEvent) {
    e.preventDefault()
    const trimmed = code.trim().toUpperCase()
    if (trimmed.length > 0) {
      navigate(`/room/${trimmed}?setup=1`)
    }
  }

  return (
    <form onSubmit={handleJoin} className="join-form">
      <input
        value={code}
        onChange={(e) => setCode(e.target.value)}
        placeholder="Código da sala"
        maxLength={6}
      />
      <button type="submit" disabled={code.trim().length === 0}>
        Entrar
      </button>
    </form>
  )
}
