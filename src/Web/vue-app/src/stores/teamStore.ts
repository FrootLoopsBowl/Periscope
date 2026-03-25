import {defineStore} from 'pinia'
import {Team} from "@/types/entities"

interface TeamState {
  teams: Team[]
}

export const useTeamStore = defineStore('team', {
  state: (): TeamState => ({
    teams: []
  }),

  actions: {
    setTeams(teams: Team[]) {
      this.teams = teams
    },
    addTeam(team: Team) {
      this.teams.push(team)
    }
  },

  getters: {
    getTeams: (state) => state.teams
  }
})
