import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {ITeamEventService} from "@/injection/interfaces"
import {SucceededOrNotResponse} from "@/types/responses"
import {ICreateTeamEventRequest, IUpdateTeamEventRequest} from "@/types/requests"
import {TeamEvent} from "@/types/entities"

@injectable()
export class TeamEventService extends ApiService implements ITeamEventService {
  public async getEvents(teamId: string, from: string, to: string): Promise<TeamEvent[]> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<TeamEvent[]>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/${teamId}/events?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`)
      .catch(function (error: AxiosError): AxiosResponse<any> | undefined {
        return error.response
      })
    if (response?.status === 200) {
      return response.data as TeamEvent[]
    }
    return []
  }

  public async createEvent(teamId: string, request: ICreateTeamEventRequest): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/${teamId}/events`,
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

  public async updateEvent(teamId: string, eventId: string, request: IUpdateTeamEventRequest): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .put<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/${teamId}/events/${eventId}`,
        request,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }

  public async deleteEvent(teamId: string, eventId: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .delete<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/${teamId}/events/${eventId}`)
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }
}
