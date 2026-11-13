using System.Linq;
using UnityEngine;

namespace ElDespacho
{
    // Stable IDs are the persistence contract. Legacy six-step assets remain untouched.
    public static class TutorialLessons
    {
        public static readonly string[] Titles = { "Bienvenido a Aurelia", "Los tres indicadores", "Manipular documentos", "Tomar decisiones", "Comprender consecuencias", "El mapa del mundo", "Conversaciones diplomáticas", "Investigar documentos", "Alianzas y conflictos", "Cartas encadenadas", "Avanzar de año", "Guardar y cargar" };
        public static TutorialStepData[] Create(TutorialStepData[] legacy)
        {
            TutorialStepData S(string id,int lesson,string screen,string target,string text,string action="",int old=-1) {
                var s=old>=0 && old<legacy.Length ? Object.Instantiate(legacy[old]) : ScriptableObject.CreateInstance<TutorialStepData>();
                s.persistentId=id;s.lesson=lesson;s.context=screen;s.targetName=target;s.instructionText=text;s.actionFact=action;
                s.spotlightAnchor=new Vector2(-1,-1);s.targetPadding=10;return s;
            }
            return new[] {
                S("welcome",0,"Introducción","AssumeButton","Presidente, soy Julián Paz, su jefe de gabinete. Tiene cuatro años por delante: las decisiones políticas y diplomáticas marcarán el futuro de Aurelia. Pulse ASUMIR EL CARGO.","assumed",0),
                S("indicators",1,"Despacho","HUD","Estabilidad: confianza ciudadana y orden político. Seguridad: defensa frente a amenazas. Prosperidad: economía y comercio. Si cualquiera llega a cero, termina su mandato."),
                S("grab",2,"Despacho","DeskDocument","Este es su primer asunto. Mantenga pulsado el expediente de la pila para cogerlo; después llévelo a la zona de lectura.","grabbed",1),
                S("open",2,"Despacho","CardArea","Arrastre el expediente hasta la zona de lectura iluminada y suéltelo. Si lo suelta fuera, vuelve a la pila: puede intentarlo de nuevo.","opened",2),
                S("decision",3,"Despacho","DeskDocument","Lea la petición. APROBADO y DENEGADO equivalen a SÍ y NO; algunas cartas usan otras respuestas. Señale un sello para ver los indicadores afectados y suéltelo sobre el papel para decidir.","answered",3),
                S("result",4,"Despacho","HUD","Revise el resultado y sus cambios numéricos. Algunas decisiones abren otros asuntos o tienen efectos posteriores. Pulse CONTINUAR para archivar; sus consecuencias ya se han aplicado.","archived",4),
                S("map",5,"Mapa","Side","Seleccione un país disponible. Su emblema identifica Aurelia y su bloque; las marcas de amenaza o aliado son sospechas. Los países ??? llegarán más adelante. La agenda muestra lo pendiente."),
                S("map_symbols",5,"Mapa","Side","La leyenda identifica sin visitar, amenaza y aliado. La marca de visitado se usa al cortar lazos; los aliados quedan en su bloque. Las líneas doradas unen sus alianzas."),
                S("questions",6,"Visita","Dialog","Puede hacer {questions} preguntas por visita. Leer informes no consume preguntas. Elija qué necesita aclarar: algunas preguntas tensan la relación. No tiene que agotarlas para decidir.","asked"),
                S("dossier",7,"Visita","Docs","El dossier reúne gobierno, ejército y relación con Aurelia. Compare estos datos con las palabras del líder. Puede cambiar de pestaña sin gastar preguntas."),
                S("intel",7,"Visita","Docs","Compruebe la fuente y la fecha del informe de inteligencia. Una fuente puede equivocarse y un dato antiguo puede no describir la situación actual. Busque contradicciones, no certezas automáticas."),
                S("trade",7,"Visita","Docs","El informe comercial muestra exportaciones, importaciones y nuestra dependencia. Téngalo en cuenta antes de cortar lazos: perder suministros puede tener costes duraderos."),
                S("region",7,"Visita","Docs","Región y riesgo muestra el contexto y los rivales conocidos. Compare sus indicios con los demás informes. Las barras no sustituyen su juicio ni revelan las intenciones del líder."),
                S("relations",8,"Visita","Buttons","Puede hacer alianza o cortar lazos. Lea el botón rojo: implica sanciones o un ataque según el país. La decisión es inmediata y puede afectar a sus rivales. No hay opción de esperar."),
                S("chain",9,"Despacho","DeskDocument","Este asunto retoma una decisión anterior. Algunas decisiones abren nuevos expedientes; otras tienen consecuencias años después. Resuelva lo que conoce ahora, sin dar por seguro el desenlace."),
                S("year",10,"Despacho","YearEndButton","Para cerrar el año debe atender los expedientes y los países pendientes. Las cadenas pueden añadir asuntos. Al avanzar llegan noticias y desgaste del cargo. El mandato dura cuatro años."),
                S("save",11,"Pausa","","Puede pausar con ESC y cambiar ajustes. Hay tres slots manuales y un autoguardado separado. Guardar y salir o volver usa el slot 1, con confirmación."),
                S("load",11,"Pausa","","El autoguardado funciona por tiempo fuera de los menús: revise su intervalo en Ajustes. CONTINUAR carga el slot válido más reciente; CARGAR permite elegir. La guía se guarda con la campaña.")
            };
        }
        public static string Text(TutorialStepData step,int questions) => AccessLanguage.Tutorial(step,questions);
    }
}
