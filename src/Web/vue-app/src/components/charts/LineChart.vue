<script setup>
import { ref, watch, onMounted } from 'vue';
import { Chart, registerables } from 'chart.js';

// Register Chart.js components
Chart.register(...registerables);

const props = defineProps({
  chartData: {
    type: Object,
    required: true
  },
  options: {
    type: Object,
    default: () => ({})
  }
});

const chartCanvas = ref(null);
let chartInstance = null;

const renderChart = () => {
  if (chartInstance) {
    chartInstance.destroy();
  }
  
  if (chartCanvas.value) {
    chartInstance = new Chart(chartCanvas.value, {
      type: 'line',
      data: props.chartData,
      options: props.options
    });
  }
};

watch(() => props.chartData, renderChart, { deep: true });
onMounted(renderChart);
</script>

<template>
  <canvas ref="chartCanvas"></canvas>
</template>

<style scoped>
canvas {
  width: 100% !important;
  height: 100% !important;
}
</style>