# Design — Barre de progression indéterminée (import athlètes)

**Date:** 2026-04-20  
**Branche:** barre-de-progression

## Contexte

Le processus d'import d'athlètes via CSV est un appel HTTP synchrone unique (`POST /api/athletes/import`). Pour les fichiers avec beaucoup de nouveaux athlètes, la durée peut atteindre plusieurs dizaines de secondes à cause de l'envoi séquentiel d'emails. Le seul feedback visuel actuel est le texte du bouton qui change en "Importation en cours..." et les inputs désactivés.

## Objectif

Ajouter une barre de progression animée (indéterminée) dans le modal d'import pour rassurer l'utilisateur que l'opération est bien en cours, sans laisser croire à une erreur ou un blocage.

## Architecture

Aucun changement backend. Modification uniquement dans :
- `src/Web/vue-app/src/views/admin/athletes/AdminAthleteIndex.vue`

## Comportement

- La barre apparaît quand `isImporting === true`
- Elle disparaît dès que la réponse arrive (succès ou erreur)
- Elle est placée sous le bouton "Importation en cours..." dans le modal
- Pleine largeur du contenu du modal

## Apparence

- Fond gris clair (`bg-gray-200`) pleine largeur, hauteur `h-2`, coins arrondis
- Un reflet animé (shimmer) traverse la barre de gauche à droite en boucle
- Couleur du shimmer : couleur primaire du projet (bleu, cohérent avec les autres éléments)
- Animation CSS `@keyframes shimmer` avec `translateX(-100%) → translateX(100%)`, durée 1.5s, boucle infinie
- Aucun pourcentage affiché

## Composants touchés

| Fichier | Changement |
|---|---|
| `AdminAthleteIndex.vue` | Ajout du bloc HTML de la barre + styles `<style scoped>` |

## Gestion des erreurs

Aucun cas d'erreur spécifique à gérer — la barre s'arrête simplement quand `isImporting` repasse à `false`, que l'import ait réussi ou échoué.
