# Umbrales de riesgo leídos y guardados en dbo.setting

**Estado:** lo implementa otro integrante. Conecta los umbrales con la base y el patrón repositorio. Este trabajo no lo implementa.

## What to build

Core expone un puerto para leer y guardar los dos umbrales. Data lo implementa contra `dbo.setting`, con SQL parametrizado y `CancellationToken`. Un caso de uso carga esos valores al arrancar y los aplica a la misma regla que usa Seguimiento. Guardar desde Ajustes persiste y actualiza la configuración en memoria: la próxima carga de Seguimiento, en la misma sesión, reclasifica con los valores nuevos. Al volver a abrir la app se leen de la tabla. La validación que hoy está en la pantalla pasa al caso de uso: enteros entre 0 y 100, y el umbral medio menor que el alto. Los textos de error de la pantalla se mantienen. Repositorios singleton; el caso de uso, transitorio.

`RiskConfig` deja 50 y 25 fijos, y Ajustes solo valida: avisa que todavía no se guardan. La tabla ya tiene `high_threshold` y `medium_threshold` (0 a 100). No hace falta migración nueva ni editar la ya aplicada. Los pesos 60/40 siguen fijos en la regla.

Hoy no hay caso de uso de lectura ni de guardado con tests.

Al terminar se puede verificar: Ajustes muestra 50 y 25 tomados de la base. Guardar 30 y 10 deja un mensaje de guardado, y un alumno de riesgo 32 pasa a Alto en Seguimiento. Cerrar y abrir la app conserva 30 y 10. Un medio mayor o igual que el alto no se guarda.

## Acceptance criteria

- [ ] Dados 50 y 25 en el puerto, al cargar, la regla clasifica con esos umbrales.
- [ ] Dados 30 y 10 válidos, al guardar, el puerto queda con esos valores y un riesgo 32 pasa a Alto.
- [ ] Dado un valor no numérico, fuera de 0–100, o un medio mayor o igual que el alto, al guardar, se rechaza y el puerto no cambia.
- [ ] Al reabrir la app, Seguimiento usa lo último guardado en la tabla.
- [ ] Los tests se llaman `Given…_When…_Then…` y usan un puerto falso, sin SQL.
- [ ] Ajustes deja de decir que los umbrales no se guardan cuando el guardado fue válido.

## Blocked by

- [02 — Seguimiento calcula grilla y tarjetas en un solo caso de uso](../02-seguimiento-grilla-y-tarjetas.md)
