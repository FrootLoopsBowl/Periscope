import {Athlete} from "@/types/entities/athlete"

export class Team {
  id?: string
  name?: string
  createdAt?: string
  athletes?: Athlete[]
}
