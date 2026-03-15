import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {IAthleteService} from "@/injection/interfaces"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {IAssignTeamToAthleteRequest, ICreateAthleteRequest} from "@/types/requests"
import {Athlete} from "@/types/entities"

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

    if (response.status === 200 || response.status === 201) {
      return new SucceededOrNotResponse(true)
    }

    const errorResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(false, errorResponse?.errors)
  }

  public async resendAccessLink(athleteId: string, athletePageRelativeUrl: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes/${athleteId}/resend-access-link`,
        {athletePageRelativeUrl},
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })

    if (response.status === 200) {
      const responseData = response.data as SucceededOrNotResponse
      if (responseData?.succeeded === false)
        return new SucceededOrNotResponse(false, responseData.errors)

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

  public async submitSubmission(token: string, effort: number, durationMinutes: number): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes/submission`,
        { token, effort, durationMinutes },
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })

    if (response.status === 200) {
      return new SucceededOrNotResponse(true)
    }

    const errorResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(false, errorResponse?.errors)
  }

  public async getAllNonPaginated(): Promise<Athlete[]> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<Athlete[]>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes/all`)
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return response.data as Athlete[]
  }

  public async getById(id: string): Promise<Athlete | null> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<Athlete>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    if (response.status === 200) {
      return response.data
    }
    return null
  }

  public async assignTeam(athleteId: string, request: IAssignTeamToAthleteRequest): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .put<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes/${athleteId}/team`,
        request,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }

  public async deleteAthlete(id: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .delete<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/athletes/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }
}
