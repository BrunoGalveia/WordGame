interface StoredIdentity {
  playerId: string
  nickname: string
}

function key(roomCode: string): string {
  return `wordgame:room:${roomCode.toUpperCase()}`
}

export function getStoredIdentity(roomCode: string): StoredIdentity | null {
  const raw = localStorage.getItem(key(roomCode))
  if (!raw) return null
  try {
    return JSON.parse(raw) as StoredIdentity
  } catch {
    return null
  }
}

export function setStoredIdentity(roomCode: string, identity: StoredIdentity): void {
  localStorage.setItem(key(roomCode), JSON.stringify(identity))
}

export function createPlayerId(): string {
  return crypto.randomUUID()
}
