# 0001 — Quizzes HTML generados

## Context
Tras lecciones 1–9 en chat, se pidieron quizzes. Un intento previo entregó markdown plano; el skill `teach` exige lecciones HTML con componentes en `assets/` (incl. quiz widget).

## Insight
El “script del quiz” no viene preempaquetado en el skill: se crea como componente reutilizable `assets/quiz.js` (shuffle de opciones + feedback) y cada lección lo linkea.

## Implications
Próximas lecciones teach deben reutilizar `lesson.css` + `quiz.js`, no reinventar el widget ni volver a markdown.
