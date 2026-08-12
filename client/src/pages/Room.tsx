import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { createPlayerId, getStoredIdentity, setStoredIdentity } from '../lib/roomStorage'
import { useGameConnection } from '../lib/useGameConnection'

export default function Room() {
  const { code } = useParams<{ code: string }>()
  const roomCode = (code ?? '').toUpperCase()
  const stored = getStoredIdentity(roomCode)

  const [nickname, setNickname] = useState(stored?.nickname ?? '')
  const [playerId] = useState(() => stored?.playerId ?? createPlayerId())
  const [confirmedNickname, setConfirmedNickname] = useState(
    stored?.nickname ? stored.nickname : null,
  )

  if (!confirmedNickname) {
    return (
      <main className="room">
        <h1>Sala {roomCode}</h1>
        <form
          className="nickname-form"
          onSubmit={(e) => {
            e.preventDefault()
            const trimmed = nickname.trim()
            if (trimmed.length === 0) return
            setStoredIdentity(roomCode, { playerId, nickname: trimmed })
            setConfirmedNickname(trimmed)
          }}
        >
          <label htmlFor="nickname">O teu nome</label>
          <input
            id="nickname"
            value={nickname}
            onChange={(e) => setNickname(e.target.value)}
            placeholder="Nome"
            autoFocus
            maxLength={30}
          />
          <button type="submit" disabled={nickname.trim().length === 0}>
            Entrar na sala
          </button>
        </form>
      </main>
    )
  }

  return <RoomGame roomCode={roomCode} nickname={confirmedNickname} playerId={playerId} />
}

function RoomGame({ roomCode, nickname, playerId }: { roomCode: string; nickname: string; playerId: string }) {
  const { status, error, isHost, players, assignment, requestNewWord } = useGameConnection(
    roomCode,
    nickname,
    playerId,
  )
  const [copied, setCopied] = useState(false)

  async function handleCopyLink() {
    await navigator.clipboard.writeText(window.location.origin + `/room/${roomCode}`)
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <main className="room">
      <header className="room-header">
        <div>
          <span className="room-code-label">Sala</span>
          <span className="room-code">{roomCode}</span>
        </div>
        <button type="button" onClick={handleCopyLink}>
          {copied ? 'Link copiado!' : 'Copiar link'}
        </button>
      </header>

      {status === 'connecting' && <p>A ligar…</p>}
      {status === 'error' && <p className="error">{error ?? 'Erro de ligação.'}</p>}

      <section className="players">
        <h2>Jogadores ({players.length})</h2>
        <ul>
          {players.map((p) => (
            <li key={p.playerId}>{p.nickname}</li>
          ))}
        </ul>
      </section>

      <section className="assignment">
        {assignment ? (
          assignment.isHintOnly ? (
            <div className="card hint">
              <span className="label">💡 A tua dica</span>
              <span className="content">{assignment.content}</span>
            </div>
          ) : (
            <div className="card word">
              <span className="label">A tua palavra</span>
              <span className="content">{assignment.content}</span>
            </div>
          )
        ) : (
          <p className="waiting">Aguarda o início da ronda…</p>
        )}
      </section>

      {isHost && (
        <button type="button" className="new-word" onClick={requestNewWord} disabled={status !== 'connected'}>
          Nova Palavra
        </button>
      )}
    </main>
  )
}
