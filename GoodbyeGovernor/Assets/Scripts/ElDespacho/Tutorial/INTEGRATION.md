# Tutorial — integración (ya aplicada en el proyecto)

Archivos: `Tutorial/TutorialStepData.cs`, `Tutorial/TutorialManager.cs`. El tutorial arranca solo; no se añade nada a la escena.

**DespachoGame.cs — 4 llamadas** (`TutorialManager.Instance?.TryAdvance(...)`, no hace nada si no hay tutorial):

1. `ShowTitle`, botón ASUMIR EL CARGO, tras `ShowOffice();` → `PressAssumeOffice`.
   Además: `assume.gameObject.name = "AssumeButton";` (el tutorial busca el botón por nombre).
2. `BuildCard`, callback `onOpened` de `deskDocument.Initialize`:
   antes `() => documentOpened = true` · después `() => { documentOpened = true; ...TryAdvance(DragDocumentToTray); }`
3. `AnswerCard`, justo después de la línea `if (k == null || cardAnswered || ...) return;` → `PressYesOrNo`
   (se dispara al estampar un sello; tras las comprobaciones, para no avanzar con un sello inválido).
4. `ContinueCard`, primera línea → `PressContinue`.

**DeskDocument.cs — 1 evento:** `public static event Action Grabbed;` y `Grabbed?.Invoke();` al final de `OnPointerDown`.
Lo escucha el tutorial para el paso «coge el expediente» (`GrabDocument`).

**ClickAnywhere** no necesita integración: lo dispara el propio overlay al hacer clic en el oscurecido o la burbuja.

**Editar los pasos:** Tools > El Despacho > Tutorial > *Crear assets de los pasos* crea `Assets/Resources/Tutorial/Step_00…05`.
Se editan en el Inspector y se muestran por orden de nombre. Sin assets, se usan los pasos por defecto del código.

**Volver a verlo:** Tools > El Despacho > Tutorial > *Volver a mostrar el tutorial* (borra `tutorial_done_v1` de PlayerPrefs).

**Nombres que puede usar `targetName`:** `AssumeButton`, `DeskDocument` (el expediente, cerrado o abierto),
`CardArea` (pila + zona de lectura), `ReadingZone`, `StampRack`, `StampYes`, `StampNo`.
