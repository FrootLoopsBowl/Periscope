import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {IAthleteService} from "@/injection/interfaces"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {ICreateAthleteRequest} from "@/types/requests"
import {Athlete, NoteBlessure} from "@/types/entities"

@injectable()
export class AthleteService extends ApiService implements IAthleteService {
  public async createAthlete(request: ICreateAthleteRequest): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes`,
        request,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })

    if (response.status === 201) {
      return new SucceededOrNotResponse(true)
    }

    const errorResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(false, errorResponse?.errors)
  }

  public async getAll(pageIndex: number, pageSize: number): Promise<PaginatedResponse<Athlete>> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<PaginatedResponse<Athlete>>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes?pageIndex=${pageIndex}&pageSize=${pageSize}`)
      .catch(function (error: AxiosError): AxiosResponse<PaginatedResponse<Athlete>> {
        return error.response as AxiosResponse<PaginatedResponse<Athlete>>
      })
    return response.data as PaginatedResponse<Athlete>
  }

  public async getInjured(): Promise<Athlete[]> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<Athlete[]>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes/injured`)
      .catch(function (error: AxiosError): AxiosResponse<Athlete[]> {
        return error.response as AxiosResponse<Athlete[]>
      })
    return response.data ?? []
  }

  public async createNoteBlessure(athleteId: string, contenu: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes/${athleteId}/notes-blessure`,
        { contenu },
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })

    if (response.status === 201) {
      return new SucceededOrNotResponse(true)
    }
    const errorResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(false, errorResponse?.errors)
  }

  public async getNotesBlessure(athleteId: string): Promise<NoteBlessure[]> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<NoteBlessure[]>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes/${athleteId}/notes-blessure`)
      .catch(function (error: AxiosError): AxiosResponse<NoteBlessure[]> {
        return error.response as AxiosResponse<NoteBlessure[]>
      })
    return response.data ?? []
  }

  public async getBySubmissionToken(token: string): Promise<{ firstName: string; lastName: string } | null> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<{ firstName: string; lastName: string }>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes/submission/${token}`)
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })

    if (response.status === 200) {
      return response.data
    }
    return null
  }
}
