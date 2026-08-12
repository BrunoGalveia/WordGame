export interface PlayerSummary {
  playerId: string
  nickname: string
}

export interface WordAssignment {
  wordId: string
  wordText: string
  isHintOnly: boolean
  content: string
}

export interface JoinRoomResult {
  playerId: string
  isHost: boolean
  players: PlayerSummary[]
  currentAssignment: WordAssignment | null
}
