# Barre de progression — Import athlètes — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Afficher une barre de progression animée (shimmer indéterminé) dans le modal d'import d'athlètes pendant que l'import est en cours.

**Architecture:** Modification uniquement dans le composant Vue frontend. Aucun changement backend. La barre apparaît quand `isImporting === true` et disparaît à la réception de la réponse. Elle est insérée entre le contenu du modal et les boutons d'action.

**Tech Stack:** Vue 3, TypeScript, TailwindCSS, CSS `@keyframes`

---

### Task 1 : Ajouter la barre de progression dans le template

**Files:**
- Modify: `src/Web/vue-app/src/views/admin/athletes/AdminAthleteIndex.vue:151-170`

- [ ] **Étape 1 : Ajouter le bloc HTML de la barre**

Dans `AdminAthleteIndex.vue`, insérer ce bloc entre la div `import-popup__content` (ligne 57, fermeture ligne 149) et la div `import-popup__actions` (ligne 151). Le bloc doit être ajouté à la ligne 151, juste avant `<div class="import-popup__actions">` :

```html
          <!-- Barre de progression -->
          <div v-if="isImporting" class="import-popup__progress">
            <div class="import-popup__progress-shimmer"></div>
          </div>
```

Le template dans cette zone doit ressembler à ceci après modification :

```html
          </div>
        </div>

          <!-- Barre de progression -->
          <div v-if="isImporting" class="import-popup__progress">
            <div class="import-popup__progress-shimmer"></div>
          </div>

          <div class="import-popup__actions">
```

- [ ] **Étape 2 : Vérifier que `v-if="isImporting"` est bien sur la div externe**

S'assurer que seule la div `.import-popup__progress` est conditionnelle, pas la div `.import-popup__actions`.

---

### Task 2 : Ajouter les styles CSS de la barre

**Files:**
- Modify: `src/Web/vue-app/src/views/admin/athletes/AdminAthleteIndex.vue:528-538` (section `<style scoped>`)

- [ ] **Étape 1 : Ajouter les styles avant la section `.fade-leave-active`**

Dans le bloc `<style scoped>`, ajouter ces règles juste avant `.fade-leave-active` (ligne 530) :

```css
/* Progress bar */
.import-popup__progress {
  position: relative;
  height: 4px;
  background: var(--color-grey-light, #e0e0e0);
  overflow: hidden;
}
.import-popup__progress-shimmer {
  position: absolute;
  top: 0;
  left: 0;
  height: 100%;
  width: 40%;
  background: linear-gradient(
    90deg,
    transparent 0%,
    var(--color-green, #4caf50) 50%,
    transparent 100%
  );
  animation: shimmer 1.5s infinite linear;
}
@keyframes shimmer {
  from { transform: translateX(-250%); }
  to   { transform: translateX(400%); }
}
```

- [ ] **Étape 2 : Vérifier l'indentation et la cohérence avec le reste du fichier**

S'assurer que les règles CSS respectent le style existant du fichier (pas de tabulation mixte).

---

### Task 3 : Vérification visuelle et commit

**Files:**
- Modify: aucun nouveau fichier

- [ ] **Étape 1 : Lancer le serveur de développement**

```bash
cd src/Web/vue-app
npm run dev
```

- [ ] **Étape 2 : Tester manuellement**

1. Ouvrir l'interface admin dans le navigateur
2. Aller dans la section Athlètes
3. Cliquer sur "Importer CSV"
4. Sélectionner un fichier CSV
5. Cliquer sur "Importer"
6. Vérifier que la barre de progression verte animée apparaît entre le contenu et les boutons pendant l'import
7. Vérifier qu'elle disparaît quand les résultats s'affichent

- [ ] **Étape 3 : Commit**

```bash
git add src/Web/vue-app/src/views/admin/athletes/AdminAthleteIndex.vue
git commit -m "feat: ajouter une barre de progression lors de l'import d'athlètes"
```
