# Prompt: Sistema de Tutorial Paso a Paso — El Despacho

---

## CONTEXTO DEL PROYECTO (léelo completo antes de escribir una sola línea)

Estás trabajando en el juego **El Despacho** (proyecto Unity, namespace `ElDespacho`).

### Restricción fundamental — UI 100% generada por código
**No existe ningún Canvas, Panel ni GameObject de UI en la Escena.** Todo se construye en tiempo de ejecución usando `Kit.Panel`, `Kit.Label`, `Kit.MakeButton`, etc. (ver `Kit.cs`). El Canvas principal se crea en `DespachoGame.Start()` y su `Transform` se guarda en `root`.

Esta restricción se aplica también al tutorial: **el overlay del tutorial debe crearse también por código**, igual que el resto del juego.

### Arranque del juego
`DespachoGame` se instancia con `[RuntimeInitializeOnLoadMethod]` — no vive en la escena. Por tanto, el `TutorialManager` que crearás **también debe autoarrancarse** con `[RuntimeInitializeOnLoadMethod]`, o bien ser inicializado explícitamente desde `DespachoGame.Start()`.

### Clases relevantes del proyecto
| Clase | Rol |
|---|---|
| `DespachoGame : MonoBehaviour` | Orquestador principal. Genera toda la UI. |
| `Kit` (static) | Utilidades para crear UI: `Panel`, `Label`, `MakeButton`, `Bar`, `Dot`, `Box` |
| `Pal` (static) | Paleta de colores del juego (`Pal.Wood`, `Pal.Gold`, `Pal.Ink`, etc.) |
| `DeskDocument : MonoBehaviour` | Maneja el drag del documento de escritorio. Expone `CanDecide` (bool). |
| `StatsManager` | Servicio POCO de estadísticas |
| `CardManager` | Servicio POCO de cartas CSV |
| `Model.cs` | Enums y structs de datos (namespace `ElDespacho`) |

### Flujo de una partida (lo que el tutorial debe enseñar)
1. Pantalla de título → botón **"ASUMIR EL CARGO"**
2. Pantalla del despacho → hay un documento en la pila (esquina inferior izquierda)
3. El jugador **arrastra** el documento hasta la **Zona de Lectura** (panel superior)
4. El documento se abre; aparecen los botones **"SÍ"** y **"NO"** (rojos/verdes)
5. El jugador pulsa uno → aparece el resultado y el botón **"CONTINUAR"**
6. Pulsa "CONTINUAR" → el documento se archiva y aparece la siguiente carta

---

## LO QUE DEBES CREAR

### Componente principal: `TutorialManager.cs`

Un `MonoBehaviour` en el namespace `ElDespacho` con las siguientes características:

#### 1. ScriptableObject de configuración: `TutorialStepData`
Crea un `ScriptableObject` llamado `TutorialStepData` con estos campos configurables desde el Inspector:

```csharp
[CreateAssetMenu(fileName = "TutorialStep", menuName = "ElDespacho/Tutorial Step")]
public class TutorialStepData : ScriptableObject
{
    [Header("Contenido del paso")]
    [TextArea(2, 5)]
    public string instructionText;       // Texto de la burbuja de ayuda

    [Header("Posición del spotlight")]
    // Coordenadas anchor normalizadas (0-1) del elemento a destacar en pantalla.
    // Ejemplo: (0.22f, 0.16f) apunta a la pila de documentos.
    // Déjalo en (-1,-1) si este paso no necesita spotlight.
    public Vector2 spotlightAnchor = new Vector2(-1f, -1f);
    public float   spotlightRadius = 120f;   // Radio del círculo en píxeles

    [Header("Condición de avance")]
    public TutorialAdvanceCondition advanceCondition;
    // Valores posibles:
    // ClickAnywhere      → cualquier clic cierra este paso
    // PressYesOrNo       → solo avanza cuando el jugador pulsa SÍ o NO
    // DragDocumentToTray → solo avanza cuando el documento entra en la zona de lectura
    // PressContinue      → solo avanza cuando el jugador pulsa "CONTINUAR"
    // PressAssumeOffice  → solo avanza cuando pulsa "ASUMIR EL CARGO" en el título
}

public enum TutorialAdvanceCondition
{
    ClickAnywhere,
    PressYesOrNo,
    DragDocumentToTray,
    PressContinue,
    PressAssumeOffice
}
```

#### 2. `TutorialManager.cs` — MonoBehaviour

```csharp
public class TutorialManager : MonoBehaviour
{
    // ── Configuración en Inspector ─────────────────────────────────
    [Header("Pasos del tutorial (arrastrar en orden)")]
    [SerializeField] TutorialStepData[] steps;

    [Header("Opciones")]
    [SerializeField] string playerPrefsKey = "tutorial_done_v1";
    // Pon a true en Editor para forzar el tutorial aunque ya se haya visto
    [SerializeField] bool forceShowInEditor = false;
    // ...
}
```

**El array `steps[]` es la única cosa que se configura en el Inspector**: el diseñador arrastra los `TutorialStepData` assets en el orden que quiera. Nada más requiere configuración manual.

#### 3. Cómo se añade `TutorialManager` a la escena
Dado que el juego no usa escena convencional, el `TutorialManager` debe añadirse al mismo `GameObject` que `DespachoGame`, o en un `GameObject` hermano, usando `RuntimeInitializeOnLoadMethod`. **No puede vivir en la escena.** Ejemplo:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
static void Boot()
{
    if (FindAnyObjectByType<TutorialManager>() != null) return;
    // Busca o crea el host (el mismo go que DespachoGame)
    var host = FindAnyObjectByType<DespachoGame>()?.gameObject
               ?? new GameObject("TutorialManager");
    host.AddComponent<TutorialManager>();
}
```

---

## DISEÑO VISUAL DEL OVERLAY

El overlay del tutorial es un Canvas separado con `sortingOrder = 200` (por encima del juego, cuyo Canvas tiene `sortingOrder = 100`). Se crea por código en el `Awake` del `TutorialManager`.

### Capas del overlay (de abajo a arriba):

```
[Canvas sortingOrder=200]
  └─ BlockerPanel         Panel negro semitransparente (alpha=0.72), cubre toda la pantalla.
                          raycastTarget = TRUE → bloquea todo clic al juego.
  └─ SpotlightHole        Imagen circular (Sprite círculo del Kit) en la posición del elemento.
                          color = Color.clear → "agujero" visual donde se ve el juego debajo.
                          Efecto: el blockerPanel tiene un RectMask2D invertido o
                          se usa un segundo Canvas con BlendMode Subtractive.
                          *** Implementación recomendada: ***
                          Usar un Panel con Image que tenga alpha=0 sobre la zona de interés,
                          y un borde animado (pulso de escala) para llamar la atención.
  └─ BubblePanel          Panel Pal.Paper con sombra, posicionado encima o debajo del spotlight.
      └─ InstructionText  Texto con el instructionText del paso actual.
      └─ SkipButton        Botón pequeño "SALTAR TUTORIAL" (solo en pasos ClickAnywhere).
```

**Implementación del "agujero" visual recomendada** (compatible con Unity uGUI sin shaders):
- El blocker cubre todo con alpha 0.72.
- Encima del blocker se coloca un círculo con el sprite circle de `Kit.Circle` (ya existe en el proyecto) con color `new Color(1,1,1,0)` y `CanvasGroup.blocksRaycasts = false`.
- Alrededor del círculo se dibuja un anillo pulsante con `Kit.Dot` escalado mediante `LeanTween` o una `Coroutine` de `Mathf.PingPong`.

---

## COMPORTAMIENTO PASO A PASO

### Inicio del tutorial
```
En Start() de TutorialManager:
  Si PlayerPrefs.GetInt(playerPrefsKey, 0) == 0  →  iniciar tutorial
  Si forceShowInEditor && Application.isEditor    →  iniciar tutorial igualmente
  Si ya completado                               →  no hacer nada, destruir el componente
```

### Lógica de avance

El `TutorialManager` expone un método interno `TryAdvance(TutorialAdvanceCondition trigger)` que:
1. Comprueba si `trigger == steps[currentStep].advanceCondition`
2. Si sí → llama a `ShowStep(currentStep + 1)`
3. Si era el último paso → llama a `CompleteTutorial()`

### Integración con DespachoGame

`DespachoGame` debe llamar a `TutorialManager.Instance.TryAdvance(...)` en los momentos clave:

| Momento en DespachoGame | Condición a disparar |
|---|---|
| Al pulsar "ASUMIR EL CARGO" (callback del botón) | `TutorialAdvanceCondition.PressAssumeOffice` |
| Al soltar el documento en la Zona de Lectura (`DeskDocument.onOpened`) | `TutorialAdvanceCondition.DragDocumentToTray` |
| Al entrar en `AnswerCard(bool yes)` (antes de aplicar efecto) | `TutorialAdvanceCondition.PressYesOrNo` |
| Al entrar en `ContinueCard()` | `TutorialAdvanceCondition.PressContinue` |
| En cualquier clic del blocker | `TutorialAdvanceCondition.ClickAnywhere` |

`TutorialManager` expone un **Singleton de escena** (`public static TutorialManager Instance`). Si `Instance == null`, las llamadas desde `DespachoGame` simplemente no hacen nada (guard null).

### Bloqueo de input

Mientras el tutorial esté activo:
- El `BlockerPanel` tiene `raycastTarget = true` → bloquea todo.
- **Excepción**: cuando `advanceCondition` es `PressYesOrNo`, `DragDocumentToTray` o `PressContinue`, el blocker debe dejar pasar los clics **solo** en la zona del spotlight. Para ello el blocker se divide en cuatro paneles laterales (arriba, abajo, izquierda, derecha del agujero), dejando el centro libre.

---

## PASOS DE TUTORIAL SUGERIDOS (para los ScriptableObjects de ejemplo)

Créalos como assets de ejemplo en `Assets/Tutorial/`:

| Asset | `instructionText` | `spotlightAnchor` | `advanceCondition` |
|---|---|---|---|
| `Step_00_Welcome` | "Bienvenido al Despacho. Eres el nuevo líder de Aurelia. Pulsa ASUMIR EL CARGO para empezar." | (-1,-1) sin spotlight | `PressAssumeOffice` |
| `Step_01_Pile` | "Este es tu montón de asuntos pendientes. Hay un expediente esperando tu decisión. Cógelo con el ratón." | (0.22f, 0.16f) r=110 | `DragDocumentToTray` |
| `Step_02_ReadZone` | "¡Bien! Arrastra el documento hasta la zona de lectura para abrirlo." | (0.50f, 0.65f) r=140 | `DragDocumentToTray` |
| `Step_03_Decision` | "Lee el expediente. Cuando estés listo, pulsa SÍ o NO para tomar tu decisión." | (0.50f, 0.08f) r=200 | `PressYesOrNo` |
| `Step_04_Result` | "Tu decisión tiene consecuencias. Lee el resultado y pulsa CONTINUAR." | (0.50f, 0.08f) r=160 | `PressContinue` |
| `Step_05_Done` | "¡Eso es todo! Así funciona el despacho. Buena suerte, líder." | (-1,-1) sin spotlight | `ClickAnywhere` |

---

## REQUISITOS TÉCNICOS DE IMPLEMENTACIÓN

1. **Cero dependencias del Inspector de Unity para la UI** — toda la UI del overlay se construye con los métodos `Kit.*` existentes, o con `new GameObject(...).AddComponent<Image>()` directamente, igual que hace el resto del proyecto.

2. **Los ScriptableObjects SÍ se configuran en Inspector** — son la única excepción, ya que son assets que no dependen de la escena.

3. **El campo `steps[]` del Inspector es la única interfaz de diseño** — los diseñadores crean nuevos `TutorialStepData` assets, los rellenan, y los arrastran al array. No hay más configuración.

4. **`PlayerPrefs.SetInt(playerPrefsKey, 1)`** se llama al completar el último paso. En el Editor, `forceShowInEditor = true` permite repetirlo sin borrar prefs.

5. **Sin TextMeshPro** — el proyecto usa `UnityEngine.UI.Text` legacy. Usar ese mismo sistema.

6. **Sin paquetes externos** — no usar DOTween, LeanTween ni ningún paquete no presente ya en el proyecto. Las animaciones de pulso se hacen con `IEnumerator` + `Time.unscaledDeltaTime`.

7. **Namespace**: todo el código va dentro de `namespace ElDespacho { }`.

8. **Compatibilidad con la generación de UI en Runtime**: `DespachoGame` llama a `Clear()` (que destruye y recrea todos los hijos del Canvas raíz) en cada cambio de pantalla. El Canvas del tutorial es un **Canvas separado** con su propio `GameObject` raíz, por lo que `Clear()` no lo toca.

---

## ENTREGA ESPERADA

Entrega **tres archivos** completos y listos para pegar en el proyecto:

### `TutorialStepData.cs`
ScriptableObject completo con todos los campos descritos y sus `[Header]` y `[Tooltip]`.

### `TutorialManager.cs`
MonoBehaviour completo con:
- Singleton `Instance`
- Boot con `[RuntimeInitializeOnLoadMethod]`
- `ShowStep(int index)` que construye el overlay completo por código
- `TryAdvance(TutorialAdvanceCondition)` público
- `CompleteTutorial()` que guarda en PlayerPrefs y destruye el overlay
- Coroutine de animación de pulso del spotlight
- Toda la lógica de bloqueo de input descrita

### `INTEGRATION.md`
Documento Markdown breve (máximo 30 líneas) con los **exactos 5 lugares de `DespachoGame.cs`** donde hay que añadir la llamada `TutorialManager.Instance?.TryAdvance(...)`, con el fragmento de código antes/después de cada uno.

---

## RESTRICCIONES DE ESTILO DE CÓDIGO

- Mismo estilo que el proyecto: expresiones lambda, `var`, sin `public` innecesarios.
- Comentarios en **español** (el proyecto usa español en todos sus comentarios).
- Respetar los modificadores de acceso: lo que es privado, que sea privado.
- No usar `Update()` para polling — usar eventos y callbacks.
