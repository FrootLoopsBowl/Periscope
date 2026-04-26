export interface ICreateTeamEventRequest {
  type: string
  startDateTime: string
  endDateTime: string
  description?: string | null
}

export interface IUpdateTeamEventRequest {
  type: string
  startDateTime: string
  endDateTime: string
  description?: string | null
}
