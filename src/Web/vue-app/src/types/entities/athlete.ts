export class Athlete {
  id?: string
  firstName?: string
  lastName?: string
  email?: string
  dateOfBirth?: string
  submissionToken?: string
  active?: boolean
  isInjured?: boolean
  createdAt?: string
}

export class NoteBlessure {
  id?: string
  athleteId?: string
  contenu?: string
  createdAt?: string
  teamId?: string
  teamName?: string
}
