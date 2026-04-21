import {Container} from "inversify";
import axios, {AxiosInstance} from 'axios';
import "reflect-metadata";

import {TYPES} from "@/injection/types";
import {
  IAdministratorService,
  IApiService,
  IAthleteService,
  IAuthenticationService,
  IBookService,
  IMemberService,
  ITeamEventService,
  ITeamService,
  IUserService
} from "@/injection/interfaces";
import {
  ApiService,
  AthleteService,
  AuthenticationService,
  BookService,
  MemberService,
  TeamEventService,
  TeamService,
  UserService
} from "@/services";
import {AdministratorService} from "@/services/administratorService";

const dependencyInjection = new Container();
dependencyInjection.bind<AxiosInstance>(TYPES.AxiosInstance).toConstantValue(axios.create())
dependencyInjection.bind<IApiService>(TYPES.IApiService).to(ApiService).inSingletonScope()
dependencyInjection.bind<IAdministratorService>(TYPES.IAdministratorService).to(AdministratorService).inSingletonScope()
dependencyInjection.bind<IAthleteService>(TYPES.IAthleteService).to(AthleteService).inSingletonScope()
dependencyInjection.bind<IAuthenticationService>(TYPES.IAuthenticationService).to(AuthenticationService).inSingletonScope()
dependencyInjection.bind<IBookService>(TYPES.IBookService).to(BookService).inSingletonScope()
dependencyInjection.bind<IMemberService>(TYPES.IMemberService).to(MemberService).inSingletonScope()
dependencyInjection.bind<ITeamService>(TYPES.ITeamService).to(TeamService).inSingletonScope()
dependencyInjection.bind<ITeamEventService>(TYPES.ITeamEventService).to(TeamEventService).inSingletonScope()
dependencyInjection.bind<IUserService>(TYPES.IUserService).to(UserService).inSingletonScope()

function useAdministratorService() {
  return dependencyInjection.get<IAdministratorService>(TYPES.IAdministratorService);
}

function useAthleteService() {
  return dependencyInjection.get<IAthleteService>(TYPES.IAthleteService);
}

function useAuthenticationService() {
  return dependencyInjection.get<IAuthenticationService>(TYPES.IAuthenticationService);
}

function useMemberService() {
  return dependencyInjection.get<IMemberService>(TYPES.IMemberService);
}

function useBookService() {
  return dependencyInjection.get<IBookService>(TYPES.IBookService);
}

function useTeamService() {
  return dependencyInjection.get<ITeamService>(TYPES.ITeamService);
}

function useTeamEventService() {
  return dependencyInjection.get<ITeamEventService>(TYPES.ITeamEventService);
}

function useUserService() {
  return dependencyInjection.get<IUserService>(TYPES.IUserService);
}


export {
  dependencyInjection,
  useAdministratorService,
  useAthleteService,
  useAuthenticationService,
  useBookService,
  useMemberService,
  useTeamEventService,
  useTeamService,
  useUserService
};