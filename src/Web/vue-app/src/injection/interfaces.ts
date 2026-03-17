// eslint-disable-next-line @typescript-eslint/no-empty-interface
import {
  IAssignAthletesToTeamRequest,
  IAssignTeamToAthleteRequest,
  ICreateAthleteRequest,
  ICreateBookRequest,
  ICreateTeamRequest,
  IEditBookRequest,
  IForgotPasswordRequest,
  ILoginRequest,
  IResetPasswordRequest,
  ITwoFactorRequest
} from "@/types/requests"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {Administrator, Athlete, NoteBlessure, Book, Member, Team, User} from "@/types/entities"
import {Guid} from "@/types";

export interface IApiService {
  headersWithJsonContentType(): any

  headersWithFormDataContentType(): any

  buildEmptyBody(): string
}

export interface IAdministratorService {
  getAuthenticated(): Promise<Administrator | undefined>
}

export interface IAthleteService {
  createAthlete(request: ICreateAthleteRequest): Promise<SucceededOrNotResponse>
  resendAccessLink(athleteId: string, athletePageRelativeUrl: string): Promise<SucceededOrNotResponse>
  getBySubmissionToken(token: string): Promise<{ firstName: string; lastName: string } | null>
  getAll(pageIndex: number, pageSize: number): Promise<PaginatedResponse<Athlete>>
  getInjured(): Promise<Athlete[]>
  createNoteBlessure(athleteId: string, contenu: string): Promise<SucceededOrNotResponse>
  getNotesBlessure(athleteId: string): Promise<NoteBlessure[]>
  getAllNonPaginated(): Promise<Athlete[]>
  getById(id: string): Promise<Athlete | null>
  assignTeam(athleteId: string, request: IAssignTeamToAthleteRequest): Promise<SucceededOrNotResponse>
  deleteAthlete(id: string): Promise<SucceededOrNotResponse>
}


export interface IAuthenticationService {
  login(request: ILoginRequest): Promise<SucceededOrNotResponse>

  twoFactor(request: ITwoFactorRequest): Promise<SucceededOrNotResponse>

  forgotPassword(request: IForgotPasswordRequest): Promise<SucceededOrNotResponse>

  resetPassword(request: IResetPasswordRequest): Promise<SucceededOrNotResponse>

  logout(): Promise<SucceededOrNotResponse>
}

export interface IMemberService {

  getAuthenticated(): Promise<Member | undefined>

  search(pageIndex: number, pageSize: number, searchValue: string): Promise<PaginatedResponse<Member>>

  getMember(id: string): Promise<Member>

  createMember(member: Member): Promise<SucceededOrNotResponse>

  updateMember(member: Member): Promise<SucceededOrNotResponse>

  deleteMember(id: Guid): Promise<SucceededOrNotResponse>
}

export interface IBookService {
  getAllBooks(): Promise<Book[]>

  getBook(bookId: string): Promise<Book>

  deleteBook(bookId: string): Promise<SucceededOrNotResponse>

  createBook(request: ICreateBookRequest): Promise<SucceededOrNotResponse>

  editBook(request: IEditBookRequest): Promise<SucceededOrNotResponse>
}

export interface ITeamService {
  createTeam(request: ICreateTeamRequest): Promise<SucceededOrNotResponse>
  getAll(pageIndex: number, pageSize: number): Promise<PaginatedResponse<Team>>
  getAllNonPaginated(): Promise<Team[]>
  getById(id: string): Promise<Team | null>
  assignAthletes(teamId: string, request: IAssignAthletesToTeamRequest): Promise<SucceededOrNotResponse>
  deleteTeam(id: Guid): Promise<SucceededOrNotResponse>
}

export interface IUserService {
  getCurrentUser(): Promise<User>
}
