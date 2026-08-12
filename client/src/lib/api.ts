export const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5216'

export interface CreateRoomResponse {
  roomCode: string
  hostPlayerId: string
}

export async function createRoom(): Promise<CreateRoomResponse> {
  const response = await fetch(`${API_BASE_URL}/api/rooms`, { method: 'POST' })
  if (!response.ok) {
    throw new Error('Não foi possível criar a sala.')
  }
  return response.json()
}
