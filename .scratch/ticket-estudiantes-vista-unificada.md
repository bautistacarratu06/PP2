# Vista unificada de Estudiantes

- **Tipo:** Epic / Historia de usuario
- **Estado:** Listo para implementar (issues en `.scratch/estudiantes-vista-unificada/issues/`)
- **Prioridad:** Alta
- **Componentes afectados:** `GestionEscolar.Core`, `GestionEscolar.Data`, `UI`, tests

---

## Objetivo

Reemplazar Nómina, Seguimiento, Entregas y Asistencia por una única página **Estudiantes** con tabla completa y un único botón **Acciones** (modal tipo command palette).

## Issues

| # | Slice | Blocked by |
|---|---|---|
| 01 | Baja lógica de estudiantes | — |
| 02 | Página Estudiantes con consulta unificada y tabla | 01 |
| 03 | Filtros de cabecera, orden y atajos de riesgo | 02 |
| 04 | Selección, barra Acciones y modal command palette | 02 (01 para Desactivar) |
| 05 | Retirar páginas antiguas y unificar menú | 03, 04 |

## Supuestos confirmados

1. Desactivar es baja lógica reversible; inactivos ocultos por defecto.
2. Acciones solo con al menos un estudiante seleccionado.
3. La tabla muestra porcentajes agregados; el estado puntual de clase/trabajo vive en el modal.
4. Los inactivos no cuentan en tarjetas de riesgo ni en porcentajes.

## Fuera de alcance

- Calendario y Ajustes.
- Cambios a la fórmula de `RiskRule`.
- Lectura de umbrales desde `dbo.setting`.
