# Tutorial Presidencial - integración v0.8-prototipo

El tutorial se inicia automáticamente mediante `TutorialManager`. No añadir otro gestor a la escena.

## Catálogo y progreso
- `TutorialLessons.cs`: 12 lecciones / 18 notas. IDs estables; textos actuales y condiciones de contexto.
- `TutorialProgress.cs`: datos por campaña, hechos observados y consultas de contexto de la clase parcial `DespachoGame`.
- `TutorialStepData.cs`: conserva enum y campos anteriores; añade ID, lección, contexto y hecho esperado.
- Los seis assets `Resources/Tutorial/Step_00...05` permanecen intactos. Se clonan como base; el catálogo actual define sus textos y añade las notas nuevas. El antiguo menú de editor para generar/resetear esos assets ya no controla el tutorial actual.

`SaveData.tutorial` es opcional y mantiene schema 1. Se copia en CaptureSave y restaura en RestoreSave. Slots antiguos sin campo tutorial reconstruyen hechos comprobables, sin repetir consecuencias. La preferencia global `tutorial_done_v1` ya no decide el progreso. La opción de guía sigue siendo una preferencia; desactivarla oculta notas pero conserva hechos reales.

Pendiente / EnCurso / Completado / Omitido se derivan del catálogo y listas persistidas. Completado exige todas las notas sin omisiones; todas resueltas con alguna omitida resulta Omitido. Saltar solo omite la lección activa. Las lecciones no visitadas pueden mantener EnCurso.

## Eventos e interfaz
Se conservan `TryAdvance` y `DeskDocument.Grabbed`: asumir, coger, abrir, decidir y continuar. El gestor reconcilia además visitas, preguntas, decisiones y año. Nombres de resaltado: AssumeButton, HUD, DeskDocument, CardArea, Dialog, Docs, Buttons, Side y YearEndButton.

La selección depende de pantalla/pestaña/estado; no conservar referencias a controles que `Clear` destruye. Overlay 500 sobre pausa 400; marcos transparentes no bloquean raycasts. La nota tiene botones Consultar, Siguiente y Saltar. Reducir movimiento elimina aparición gradual y pulso.

`GuidedDiplomacy` muestra la explicación antes del primer efecto diplomático si sigue pendiente. Ese clic no decide: confirmar/omitir la nota y volver a pulsar la decisión. No se almacena ninguna acción pendiente. Mantener las consecuencias en la lógica existente.

`DeskMenus.ShowLesson` es consulta de solo lectura con Atrás/Siguiente, desde principal y pausa. No marca progreso de campaña ni ejecuta acciones. No reinicia partida.

## Validación
Unity 6000.3.24f1: `-batchmode -nographics -projectPath <proyecto> -executeMethod TutorialValidationRunner.Run -logFile <log>`.
Regresión: sustituir executeMethod por `MenuValidationRunner.Run`.
Los runners aíslan guardados y preferencias en carpetas temporales. Tutorial: 54 PASS; menús: 58; skin: 21; documentos: 32. Son pruebas Play Mode automatizadas, con eventos uGUI y estados preparados, no una campaña manual completa.

Pendiente aceptación humana de arrastre y sellos con guía activa: el arrastre mediante la herramienta de escritorio no se completó; los eventos uGUI y raycasts sí pasaron. Revisar también visitas/mapa a otras resoluciones.

Al integrar: conservar GUID de .meta, combinar cambios en archivos compartidos, no tocar cartas/países/perfiles/balance. Mantener IDs al ampliar catálogo y el campo opcional al evolucionar el guardado. Consultar el informe técnico de Documentation y CHANGELOG.
