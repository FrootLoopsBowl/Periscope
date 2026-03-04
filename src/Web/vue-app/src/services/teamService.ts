import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {ITeamService} from "@/injection/interfaces"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {IAssignAthletesToTeamRequest, ICreateTeamRequest} from "@/types/requests"
import {Team} from "@/types/entities"
import {Guid} from "@/types"

@injectable()
export class TeamService extends ApiService implements ITeamService {
  public async createTeam(request: ICreateTeamRequest): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams`,
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

  public async getAll(pageIndex: number, pageSize: number): Promise<PaginatedResponse<Team>> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<PaginatedResponse<Team>>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams?pageIndex=${pageIndex}&pageSize=${pageSize}`)
      .catch(function (error: AxiosError): AxiosResponse<PaginatedResponse<Team>> {
        return error.response as AxiosResponse<PaginatedResponse<Team>>
      })
    return response.data as PaginatedResponse<Team>
  }

  public async getAllNonPaginated(): Promise<Team[]> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<Team[]>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/all`)
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return response.data as Team[]
  }

  public async getById(id: string): Promise<Team | null> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<Team>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    if (response.status === 200) {
      return response.data
    }
    return null
  }

  public async assignAthletes(teamId: string, request: IAssignAthletesToTeamRequest): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .put<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/${teamId}/athletes`,
        request,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }

  public async deleteTeam(id: Guid): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .delete<any, AxiosResponse<any>>(`${import.meta.env.VITE_API_BASE_URL}/teams/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }
}
