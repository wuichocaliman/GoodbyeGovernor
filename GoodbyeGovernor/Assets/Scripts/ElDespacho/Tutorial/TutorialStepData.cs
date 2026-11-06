using UnityEngine;

namespace ElDespacho
{
    /// <summary>Qué acción del jugador hace avanzar un paso del tutorial.</summary>
    public enum TutorialAdvanceCondition
    {
        ClickAnywhere,      // cualquier clic sobre el oscurecido o la burbuja
        PressYesOrNo,       // estampar un sello (APROBADO / DENEGADO) sobre el expediente
        DragDocumentToTray, // soltar el expediente en la zona de lectura (se abre)
        PressContinue,      // pulsar CONTINUAR tras leer el resultado
        PressAssumeOffice,  // pulsar ASUMIR EL CARGO en la pantalla de título
        GrabDocument        // coger el expediente de la pila (empezar a arrastrarlo)
    }

    /// <summary>
    /// Un paso del tutorial. Se crea como asset (menú ElDespacho/Tutorial Step o
    /// Tools > El Despacho > Tutorial > Crear assets de los pasos) y se guarda en
    /// Assets/Resources/Tutorial/ para que TutorialManager lo cargue en runtime.
    /// Los pasos se muestran en orden alfabético de nombre de archivo (Step_00, Step_01...).
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialStep", menuName = "ElDespacho/Tutorial Step")]
    public class TutorialStepData : ScriptableObject
    {
        [Header("Contenido del paso")]
        [Tooltip("Texto de la burbuja de ayuda.")]
        [TextArea(2, 5)]
        public string instructionText;
        public string persistentId, context, actionFact;
        public int lesson;

        [Header("Qué destacar (foco)")]
        [Tooltip("Nombre del objeto de la interfaz a destacar, generado por código: AssumeButton, DeskDocument, " +
                 "ReadingZone, CardArea... Tiene prioridad sobre spotlightAnchor. El foco sigue al objeto aunque " +
                 "la pantalla se redibuje. Vacío = usar spotlightAnchor.")]
        public string targetName;

        [Tooltip("Margen extra alrededor del objeto destacado, en píxeles de referencia (1920x1080).")]
        public float targetPadding = 14f;

        [Tooltip("Solo si targetName está vacío: centro del foco en coordenadas normalizadas de pantalla (0-1). " +
                 "(-1,-1) = paso sin foco.")]
        public Vector2 spotlightAnchor = new Vector2(-1f, -1f);

        [Tooltip("Solo si targetName está vacío: mitad del lado del foco, en píxeles de referencia (1920x1080).")]
        public float spotlightRadius = 120f;

        [Header("Condición de avance")]
        [Tooltip("Qué tiene que hacer el jugador para pasar al siguiente paso.")]
        public TutorialAdvanceCondition advanceCondition;

        /// <summary>Pasos por defecto: se usan si no hay assets en Resources/Tutorial y sirven de plantilla para crearlos.</summary>
        internal static TutorialStepData[] CreateDefaults()
        {
            return new[]
            {
                Make("Step_00_Welcome",
                    "Bienvenido al Despacho. Eres el nuevo líder de Aurelia.\nPulsa ASUMIR EL CARGO para empezar.",
                    "AssumeButton", TutorialAdvanceCondition.PressAssumeOffice),
                Make("Step_01_Pile",
                    "Este es tu montón de asuntos pendientes. Hay un expediente esperando tu decisión.\n" +
                    "Haz clic sobre él y mantén pulsado el botón del ratón.",
                    "DeskDocument", TutorialAdvanceCondition.GrabDocument),
                Make("Step_02_ReadZone",
                    "¡Bien! Sin soltar el botón, arrastra el expediente hasta la ZONA DE LECTURA para abrirlo.",
                    "CardArea", TutorialAdvanceCondition.DragDocumentToTray),
                Make("Step_03_Decision",
                    "Lee el expediente. Cuando lo tengas claro, coge un sello (APROBADO o DENEGADO) " +
                    "y estámpalo sobre el papel.\nPasa el ratón por un sello para ver a qué afecta.",
                    "DeskDocument", TutorialAdvanceCondition.PressYesOrNo),
                Make("Step_04_Result",
                    "Cada decisión tiene consecuencias: mira cómo cambian los indicadores de arriba.\n" +
                    "Cuando termines de leer, pulsa CONTINUAR.",
                    "DeskDocument", TutorialAdvanceCondition.PressContinue),
                Make("Step_05_Done",
                    "¡Eso es todo! Sella los asuntos de tu mesa y visita a los países desde el MAPA DEL MUNDO.\n" +
                    "Buena suerte, líder.",
                    "", TutorialAdvanceCondition.ClickAnywhere),
            };
        }

        static TutorialStepData Make(string name, string text, string target, TutorialAdvanceCondition condition)
        {
            var step = CreateInstance<TutorialStepData>();
            step.name = name;
            step.instructionText = text;
            step.targetName = target;
            step.advanceCondition = condition;
            return step;
        }
    }
}
