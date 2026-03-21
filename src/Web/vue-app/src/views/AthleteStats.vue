<template>
  <div class="athlete-stats">
    <div class="athlete-stats__header">
      <div class="athlete-stats__info">
        <h1 class="athlete-stats__title">Statistiques de {{ athleteName }}</h1>
        <p v-if="formattedBirthDate" class="athlete-stats__birthdate">
          {{ formattedBirthDate }} <span v-if="athleteAge !== null" class="athlete-stats__age">({{ athleteAge }} ans)</span>
        </p>
      </div>
      <button class="btn btn--purple" @click="goBack">Retour à la liste</button>
    </div>

    <div class="athlete-stats__current">
      <h2 class="athlete-stats__subsection-title">Stats actuelles</h2>
      <div class="athlete-stats__cards">
        <div class="athlete-stats__card card">
          <h3 class="card__title">Effort</h3>
          <p class="card__value">{{ currentStats.effort }}/10</p>
        </div>
        <div class="athlete-stats__card card">
          <h3 class="card__title">Durée</h3>
          <p class="card__value">{{ currentStats.duration }} min</p>
        </div>
        <div class="athlete-stats__card card">
          <h3 class="card__title">Plaisir</h3>
          <p class="card__value">{{ currentStats.plaisir }}/10</p>
        </div>
      </div>
    </div>

    <div class="athlete-stats__history">
      <h2 class="athlete-stats__subsection-title">Historique des 4 dernières semaines</h2>
      <div class="athlete-stats__chart">
        <ApexChart
          type="line"
          :options="chartOptions"
          :series="chartSeries"
        />
      </div>
      <div class="athlete-stats__table">
        <table class="table">
          <thead>
            <tr>
              <th>Semaine</th>
              <th>Effort</th>
              <th>Durée (min)</th>
              <th>Plaisir</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(week, index) in history" :key="index">
              <td>Semaine {{ index + 1 }}</td>
              <td>{{ week.effort }}/10</td>
              <td>{{ week.duration }}</td>
              <td>{{ week.plaisir }}/10</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import ApexChart from 'vue3-apexcharts';
import { useAthleteService } from "@/inversify.config";

const route = useRoute();
const router = useRouter();
const athleteService = useAthleteService();

const athleteName = ref('');
const athleteBirthDate = ref('');
const athleteId = route.params.id as string;

// Fonction pour formater la date de naissance
const formatBirthDate = (birthDate: string) => {
  if (!birthDate) return '';
  
  // Essayer de parser la date dans différents formats
  let date = new Date(birthDate);
  
  // Si la date n'est pas valide, essayer de parser en tant que timestamp
  if (isNaN(date.getTime())) {
    const timestamp = parseInt(birthDate);
    if (!isNaN(timestamp)) {
      date = new Date(timestamp);
    }
  }
  
  // Si la date est toujours invalide, retourner la date originale
  if (isNaN(date.getTime())) return birthDate;
  
  // Formater la date au format français (Canada)
  return date.toLocaleDateString('fr-CA');
};

// Fonction pour calculer l'âge à partir de la date de naissance
const calculateAge = (birthDate: string) => {
  if (!birthDate) return null;
  
  // Essayer de parser la date dans différents formats
  let birthDateObj = new Date(birthDate);
  
  // Si la date n'est pas valide, essayer de parser en tant que timestamp
  if (isNaN(birthDateObj.getTime())) {
    const timestamp = parseInt(birthDate);
    if (!isNaN(timestamp)) {
      birthDateObj = new Date(timestamp);
    }
  }
  
  // Si la date est toujours invalide, retourner null
  if (isNaN(birthDateObj.getTime())) return null;
  
  const today = new Date();
  let age = today.getFullYear() - birthDateObj.getFullYear();
  const monthDiff = today.getMonth() - birthDateObj.getMonth();
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDateObj.getDate())) {
    age--;
  }
  return age;
};

const formattedBirthDate = computed(() => {
  return formatBirthDate(athleteBirthDate.value);
});

const athleteAge = computed(() => {
  return calculateAge(athleteBirthDate.value);
});

const currentStats = ref({
  effort: 0,
  duration: 0,
  plaisir: 0
});

const history = ref([
  { effort: 0, duration: 0, plaisir: 0 },
  { effort: 0, duration: 0, plaisir: 0 },
  { effort: 0, duration: 0, plaisir: 0 },
  { effort: 0, duration: 0, plaisir: 0 }
]);

onMounted(async () => {
  console.log('Athlete ID:', athleteId); // Log pour vérifier l'ID de l'athlète
  try {
    // Récupérer tous les athlètes et trouver celui avec l'ID correspondant
    const response = await athleteService.getAll(1, 100); // Récupérer jusqu'à 100 athlètes
    console.log('All athletes data:', response); // Log pour vérifier les données retournées
    if (response && response.items) {
      const foundAthlete = response.items.find(athlete => athlete.id === athleteId);
      if (foundAthlete && foundAthlete.firstName && foundAthlete.lastName) {
        athleteName.value = `${foundAthlete.firstName} ${foundAthlete.lastName}`;
        athleteBirthDate.value = foundAthlete.dateOfBirth || '';
        console.log('Athlete name set to:', athleteName.value); // Log pour vérifier le nom de l'athlète
        console.log('Athlete birth date set to:', athleteBirthDate.value); // Log pour vérifier la date de naissance de l'athlète
        console.log('Formatted birth date:', formattedBirthDate.value); // Log pour vérifier la date de naissance formatée
        console.log('Athlete age:', athleteAge.value); // Log pour vérifier l'âge de l'athlète
      } else {
        console.error('Athlete not found in the list:', athleteId);
      }
    } else {
      console.error('No athletes data returned');
    }
  } catch (error) {
    console.error('Error fetching athletes data:', error);
  }
  
  // Générer des données mockées pour les statistiques
  currentStats.value = {
    effort: Math.floor(Math.random() * 10) + 1,
    duration: Math.floor(Math.random() * 60) + 30,
    plaisir: Math.floor(Math.random() * 10) + 1
  };
  console.log('Current stats:', currentStats.value); // Log pour vérifier les statistiques actuelles
  
  // Générer des données mockées pour l'historique
  history.value = Array.from({ length: 4 }, () => ({
    effort: Math.floor(Math.random() * 10) + 1,
    duration: Math.floor(Math.random() * 60) + 30,
    plaisir: Math.floor(Math.random() * 10) + 1
  }));
  console.log('History:', history.value); // Log pour vérifier l'historique
  
  // Mettre à jour les séries du graphique avec les données de l'historique
  chartSeries.value = [
    {
      name: 'Effort',
      data: history.value.map(week => week.effort)
    },
    {
      name: 'Durée',
      data: history.value.map(week => week.duration)
    },
    {
      name: 'Plaisir',
      data: history.value.map(week => week.plaisir)
    }
  ];
  console.log('Chart series:', chartSeries.value); // Log pour vérifier les séries du graphique
});

const chartOptions = ref({
  chart: {
    id: 'athlete-stats-chart',
    toolbar: {
      show: false
    }
  },
  xaxis: {
    categories: ['Semaine 1', 'Semaine 2', 'Semaine 3', 'Semaine 4']
  },
  colors: ['#7FBA27', '#006945', '#101914']
});

const chartSeries = ref([
  {
    name: 'Effort',
    data: history.value.map(week => week.effort)
  },
  {
    name: 'Durée',
    data: history.value.map(week => week.duration)
  },
  {
    name: 'Plaisir',
    data: history.value.map(week => week.plaisir)
  }
]);

const goBack = () => {
  router.push({ name: 'admin.children.athletes.index' });
};

onMounted(() => {
  // Ici, vous pourriez charger les données réelles de l'athlète depuis une API
  // Par exemple :
  // const athleteData = await fetchAthleteData(athleteId);
  // athleteName.value = athleteData.name;
  // currentStats.value = athleteData.currentStats;
  // history.value = athleteData.history;
  // chartSeries.value = [
  //   { name: 'Effort', data: history.value.map(week => week.effort) },
  //   { name: 'Durée', data: history.value.map(week => week.duration) },
  //   { name: 'Plaisir', data: history.value.map(week => week.plaisir) }
  // ];
});
</script>

<style scoped>
.athlete-stats {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.athlete-stats__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.athlete-stats__title {
  font-size: 24px;
  font-weight: bold;
  color: #101914;
}

.athlete-stats__info {
  display: flex;
  flex-direction: column;
}

.athlete-stats__birthdate {
  font-size: 16px;
  color: #7FBA27;
  margin-top: 5px;
}

.athlete-stats__age {
  font-size: 14px;
  color: #006945;
  font-style: italic;
}

.athlete-stats__subsection-title {
  font-size: 20px;
  font-weight: bold;
  color: #101914;
  margin-bottom: 15px;
}

.athlete-stats__cards {
  display: flex;
  gap: 20px;
  margin-bottom: 30px;
}

.athlete-stats__card {
  flex: 1;
  padding: 20px;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  text-align: center;
}

.card__title {
  font-size: 16px;
  font-weight: bold;
  color: #101914;
  margin-bottom: 10px;
}

.card__value {
  font-size: 24px;
  font-weight: bold;
  color: #006945;
}

.athlete-stats__chart {
  margin-bottom: 30px;
  background-color: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.athlete-stats__table {
  background-color: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.table {
  width: 100%;
  border-collapse: collapse;
}

.table th,
.table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

.table th {
  background-color: #f5f5f5;
  font-weight: bold;
  color: #101914;
}

.table tr:hover {
  background-color: #f9f9f9;
}
</style>