import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {ITeamService} from "@/injection/interfaces"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {ICreateTeamRequest} from "@/types/requests"
import {Team} from "@/types/entities"

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
}
