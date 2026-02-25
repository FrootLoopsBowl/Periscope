<template>
  <div class="dash">
    <div class="filters">
      <select name="team" id="team-select">
        <option value="">Équipes</option>
        <option value="team1">équipe1</option>
        <option value="team2">équipe2</option>
      </select>
      <select name="athletes" id="athletes-select">
        <option value="" disabled selected>Athlètes</option>
        <option value="athlete1">athlète1</option>
        <option value="athlete2">athlète2</option>
      </select>
    </div>

    <div class="grid">
      <section class="card graph">
        <div class="cardHeader">
          <h2>Charge moyenne par athlète</h2>
        </div>
      </section>

      <section class="card injuries">
        <div class="cardHeader">
          <h2>Blessures récentes</h2>
        </div>

        <button class="btnGhost">Voir les blessures</button>
      </section>

      <section class="card table span2">
        <div class="cardHeader">
          <h2>Athlètes à surveiller</h2>
          <button class="dots">⋯</button>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.dash {
  margin: 0;
}

.filters {
  display: grid;
  grid-template-columns: 220px 280px 1fr;
  gap: 12px;
  align-items: center;
  margin-bottom: 14px;
}

select {
  background: #fff;
  border: 1px solid #e6eaf2;
  border-radius: 10px;
  padding: 10px 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: #334155;
  box-shadow: 0 6px 18px rgba(15, 23, 42, 0.04);
}

.tabs {
  justify-self: end;
  display: flex;
  gap: 10px;
}


.grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 14px;
}

.card {
  background: #fff;
  border: 1px solid #e6eaf2;
  border-radius: 16px;
  box-shadow: 0 10px 24px rgba(15, 23, 42, 0.06);
  padding: 14px;
}

.cardHeader {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}

h2 {
  margin: 0;
  font-size: 14px;
  color: #475569;
  font-weight: 800;
}



.btnGhost {
  width: 100%;
  margin-top: 10px;
  border: 1px solid #e6eaf2;
  background: #fff;
  padding: 10px 12px;
  border-radius: 12px;
  cursor: pointer;
  color: #334155;
}

.span2 {
  grid-column: 1 / -1;
}

.dots {
  border: none;
  background: transparent;
  font-size: 18px;
  cursor: pointer;
  color: #94a3b8;
}



@media (max-width: 1100px) {
  .filters {
    grid-template-columns: 1fr;
  }
  .tabs {
    justify-self: start;
  }
  .grid {
    grid-template-columns: 1fr;
  }
  .span2 {
    grid-column: auto;
  }
}
</style>
