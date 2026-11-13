using UnityEngine;
namespace ElDespacho {
 public static class AccessLanguage {
  public static string Tutorial(TutorialStepData s,int questions){
   string t=s.instructionText.Replace("{questions}",questions.ToString());
   if(AccessPreferences.Current.simpleLanguage){switch(s.persistentId){
    case "welcome":t="Soy Julián Paz. Le ayudaré a gobernar Aurelia durante cuatro años. Sus decisiones cambiarán el país. Pulse ASUMIR EL CARGO.";break;
    case "indicators":t="Estabilidad: confianza y orden. Seguridad: defensa. Prosperidad: economía. Si un valor llega a 0, termina el mandato.";break;
    case "grab":t="Seleccione el expediente de la pila. Después, llévelo a la zona de lectura.";break;
    case "open":t="Coloque el expediente en la zona de lectura. Se abrirá. Si lo suelta fuera, vuelve a la pila. Puede repetirlo.";break;
    case "decision":t="Lea la petición. Elija APROBADO (SÍ) o DENEGADO (NO). Algunas cartas tienen otras respuestas. Al seleccionar el sello verá qué indicadores cambia. Confirme sobre el papel.";break;
    case "result":t="Su decisión ya se ha aplicado. Revise los números y el resultado. Puede traer otros asuntos ahora o en años futuros. CONTINUAR archiva el expediente.";break;
    case "map":t="Elija un país disponible. [?] aún no visitado. [!] posible amenaza. [+] posible aliado. Son pistas, no certezas. Los países ??? llegan después.";break;
    case "map_symbols":t="Su bloque reúne a sus aliados. Las líneas doradas los unen. Visitado indica que cortó lazos. Consulte la agenda para ver qué falta.";break;
    case "questions":t="Puede hacer "+questions+" preguntas en esta visita. Algunas molestan al líder. Leer informes no gasta preguntas. Puede decidir antes de agotarlas.";break;
    case "dossier":t="El dossier resume gobierno, ejército y relación con Aurelia. Compare esos datos con lo que dice el líder. Cambiar de pestaña no gasta preguntas.";break;
    case "intel":t="Mire quién informa y la fecha. La información puede ser falsa o antigua. Compare varias fuentes antes de decidir.";break;
    case "trade":t="Revise qué compramos y vendemos. Cortar lazos puede dejar al país sin suministros y causar pérdidas durante varios años.";break;
    case "region":t="Revise la región y sus rivales. Compare estos datos con los otros informes. Las barras no revelan las intenciones del líder.";break;
    case "relations":t="Alianza: cooperar. Cortar lazos: imponer sanciones o atacar, según el botón rojo. La decisión se aplica al elegirla y puede afectar a otros países. No puede esperar.";break;
    case "chain":t="Este asunto viene de una decisión anterior. Algunas decisiones abren nuevos expedientes. Otras tienen efectos años después. El final aún no está decidido.";break;
    case "year":t="Atienda todos los expedientes y países pendientes para cerrar el año. Pueden aparecer más expedientes. Al avanzar llegan noticias y desgaste. El mandato dura cuatro años.";break;
    case "save":t="ESC abre la pausa. Hay 3 guardados manuales y 1 automático. Guardar y salir o volver utiliza el guardado 1 y pide confirmación. Los ajustes están aquí.";break;
    case "load":t="El guardado automático usa el intervalo de Ajustes y funciona fuera del menú. CONTINUAR abre el guardado válido más reciente. CARGAR permite elegir. También se guarda el progreso de la guía.";break;
   }}
   if(s.persistentId=="grab"||s.persistentId=="open"){
    if(AccessPreferences.Current.simpleDocuments)t="1. Pulse el expediente. 2. Pulse la zona de lectura para abrirlo. Pulse fuera o ESC para cancelar.";
    if(AccessPreferences.Current.keyboard)t+=" Con teclado: seleccione el expediente con TAB y pulse INTRO; vuelva a pulsar INTRO para colocarlo.";
   }
   if(s.persistentId=="decision"&&(AccessPreferences.Current.simpleDocuments||AccessPreferences.Current.keyboard))t+=" Seleccione un sello y confirme sobre el expediente o con la acción Aplicar sello seleccionado.";
   return (AccessPreferences.Current.symbols?"[D] ":"")+t;
  }
 }
 public partial class DespachoGame {
  public string AccessIndicators()=>"[=] Estabilidad "+estabilidad+" / 100: "+AccessController.Status(estabilidad)+". [S] Seguridad "+seguridad+" / 100: "+AccessController.Status(seguridad)+". [$] Prosperidad "+prosperidad+" / 100: "+AccessController.Status(prosperidad)+". Año "+year+" de 4.";
 }
 public enum AccessAudioChannel {Music,Ambient}
 // Optional adapter for future authored music/ambient sources. No fabricated tracks.
 [RequireComponent(typeof(AudioSource))] public sealed class AccessAudioChannelVolume:MonoBehaviour {
  public AccessAudioChannel channel;AudioSource source;float original;
  void Awake(){source=GetComponent<AudioSource>();original=source.volume;if(GetComponent<AccessAudioFilter>()==null)gameObject.AddComponent<AccessAudioFilter>();}
  void Update(){source.volume=original*(channel==AccessAudioChannel.Music?AccessPreferences.Current.music:AccessPreferences.Current.ambient);}
 }
}
