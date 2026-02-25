import {IPerson} from "@/types/entities/person";

export class Entraineur implements IPerson {
  id?: string
  firstName?: string
  lastName?: string
  fullName?: string
  email?: string
  phoneNumber?: string
  phoneExtension?: number
}
