using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 // Presentation only: all settings use the existing preference stores and controller.
 public sealed partial class AccessController {
  bool demoSelected, demoOpened, demoNotice;
  string applied="Los ajustes se guardan automáticamente.";
  string glossaryReturnPage;bool glossaryReturnReader;
  public void OpenCategory(string name){if(name=="Glosario"&&page!="Glosario"){glossaryReturnPage=page;glossaryReturnReader=reader;}generalAudio=false;page=name=="General"?"Inicio":name;focused=-1;Refresh();}
#if UNITY_EDITOR
  public void ApplyProfile(int profile){
   if(profile<0||profile>3)return;
   P.ToggleProfile(profile,Speech.Ready&&Speech.Error=="");
   SaveSettings();
  }
  public string ConfigurationStatus=>P.profileMask==0?"Configuración personalizada":
   "Configuración recomendada: "+string.Join(" + ",new[]{0,1,2,3}.Where(i=>(P.profileMask&(1<<i))!=0).Select(i=>new[]{"Lectura cómoda","Interacción sencilla","Comprensión guiada","Escucha asistida"}[i]))+
   (P.HasProfileOverrides?" · con ajustes personales prioritarios":"");
#endif
  public void DemoDocument(){if(!P.simpleDocuments)return;demoSelected=true;demoOpened=false;Refresh();}
  public void DemoPlace(){if(!P.simpleDocuments||!demoSelected)return;demoOpened=true;demoSelected=false;Refresh();}
  public bool DemoOpened=>demoOpened;
  public void AdjustFocused(float direction){if(focused<0||focused>=controls.Count)return;var target=controls[focused].target;if(target==null)return;var slider=target.GetComponentInChildren<Slider>();if(slider!=null&&slider.interactable)slider.value=Mathf.Clamp(slider.value+direction*(slider.maxValue-slider.minValue)*.05f,slider.minValue,slider.maxValue);}
  List<Entry> SettingsEntries(){
   if(page=="Controles"||page=="Glosario"||page=="Restaurar")return LegacySettingsEntries();
   var a=new List<Entry>();
   void Info(string title,string desc,string icon="D")=>a.Add(new Entry(title){description=desc,icon=icon});
   void Action(string title,string desc,string icon,Action f,string value="",bool enabled=true)=>a.Add(new Entry(title,f,null,enabled){description=desc,icon=icon,value=value});
   void Toggle(string title,string desc,string icon,bool value,Action f)=>Action(title,desc,icon,()=>Set(f),value?"ON":"OFF");
   void Volume(string title,string desc,float value,Action<float> f,bool enabled=true)=>a.Add(new Entry(title,()=>{},null,enabled){description=desc,icon="audio",value=Mathf.RoundToInt(value*100)+" %",sliderValue=value,sliderChange=v=>Set(()=>f(v))});
   if(page=="Inicio"){
    Info("Elige una categoría","Configura cada ayuda por separado. Los cambios se guardan automáticamente.","gear");
    string[] ids={"Visual","Motor","Cognitiva","Audio"};
    string[] names={"VISIÓN Y LECTURA","CONTROLES","COMPRENSIÓN","AUDIO Y NARRACIÓN"};
    string[] descriptions={"Texto, contraste y lectura opcional.","Documentos, teclado y movimiento.","Instrucciones, símbolos y ayudas.","Volúmenes, voz y avisos visuales."};
    string[] icons={"eye","cursor","book","audio"};
    for(int i=0;i<4;i++){string id=ids[i];Action(names[i],descriptions[i],icons[i],()=>OpenCategory(id),"Abrir");a[a.Count-1].card=true;}
    Action("Restaurar accesibilidad","", "gear",()=>OpenCategory("Restaurar"),"Restaurar accesibilidad");
   }else if(page=="Visual"){
    Toggle("Alto contraste","Mejora la diferencia entre el texto y el fondo.","eye",DeskPreferences.Current.highContrast,()=>DeskPreferences.Current.highContrast=!DeskPreferences.Current.highContrast);
    Action("Tamaño de texto","Amplía los textos funcionales del juego, Accesibilidad y el lector. Los textos largos se desplazan dentro de su panel; la portada mantiene su ajuste al espacio.","eye",()=>Set(()=>P.textScale=Cycle(P.textScale,new[]{.75f,1f,1.25f,1.5f,1.75f})),Mathf.RoundToInt(P.textScale*100)+" %");
    Action("Escala de interfaz","Amplía los espacios, símbolos y controles de Accesibilidad y del lector. Para cambiar las letras, utiliza Tamaño de texto.","eye",()=>Set(()=>P.interfaceScale=Cycle(P.interfaceScale,new[]{.8f,.9f,1f,1.1f,1.2f})),Mathf.RoundToInt(P.interfaceScale*100)+" %");
    Toggle("Indicadores con etiquetas","Añade nombres y estado de los indicadores al lector. Símbolos y narración también muestran esas etiquetas.","=",P.independentColor,()=>P.independentColor=!P.independentColor);
    Toggle("Reducir destellos","Elimina el pulso del tutorial y reduce los temblores de la interfaz.","eye",P.reduceFlashes,()=>P.reduceFlashes=!P.reduceFlashes);
    Info("Lectura opcional",P.readingView?"Tu preferencia anterior se conserva como recordatorio. El lector solo se abre al pulsar F2 o Abrir ahora.":"El lector es una alternativa opcional. Pulsa F2 o Abrir ahora; vuelve al modo visual con F2 o ESC.","D");
    Action("Vista de lectura","Abre la alternativa de texto para esta sesión. F2, ESC o Volver al modo visual permiten salir. Los demás ajustes nunca la abren automáticamente.","D",OpenReadingView,"Abrir ahora");
   }else if(page=="Motor"){
    Toggle("Seleccionar documentos con clics","Pulsa un documento y después la zona de lectura. No hace falta arrastrarlo. Desactivado: arrastra el documento.","cursor",P.simpleDocuments,()=>SetDemoMode());
    Toggle("Navegación por teclado","Flechas: navegar. INTRO/Espacio: activar. TAB también avanza. En deslizadores, izquierda/derecha ajustan el valor.","cursor",P.keyboard,()=>P.SetKeyboard(!P.keyboard));
    Toggle("Botones y zonas más grandes","Amplía las zonas de documentos, sellos y botones pequeños del juego, además del lector.","cursor",P.largeTargets,()=>P.largeTargets=!P.largeTargets);
    Toggle("Reducir animaciones","Simplifica la apertura y el archivo de documentos y elimina temblores.","cursor",DeskPreferences.Current.reduceMotion,()=>DeskPreferences.Current.reduceMotion=!DeskPreferences.Current.reduceMotion);
    Action("Controles y atajos","Consulta los controles y cambia las teclas de repetir y detener voz.","cursor",()=>OpenCategory("Controles"),"Consultar");
    Info("Movimiento de cámara","Este prototipo no tiene movimiento de cámara.","cursor");
   }else if(page=="Cognitiva"){
    Toggle("Instrucciones sencillas","Aclara las acciones del tutorial, despacho, mapa e informes. No cambia declaraciones ni pruebas narrativas.","book",P.simpleLanguage,()=>P.simpleLanguage=!P.simpleLanguage);
    Toggle("Ayudas de navegación","Indica el siguiente paso en la nota del asesor y en el lector. El marco de teclado es independiente.","cursor",P.navigationHelp,()=>P.navigationHelp=!P.navigationHelp);
    Toggle("Símbolos explicativos","Identifica indicadores, sellos, informes y estados del mapa con símbolos y texto. El glosario se controla por separado.","=",P.symbols,()=>P.symbols=!P.symbols);
    Toggle("Glosario disponible","Permite consultar términos desde el despacho, tutorial, diálogos, informes y lector, y volver al mismo lugar.","book",P.glossary,()=>P.glossary=!P.glossary);
    Action("Glosario político","Consulta términos y símbolos. Las sospechas nunca se presentan como certezas.","book",()=>OpenCategory("Glosario"),"Consultar");
    Action("Tutorial y ayudas contextuales","Consulta las doce lecciones del asesor. Las ayudas de la campaña conservan su progreso.","book",()=>{ClosePanels();menus.ShowLesson(0);},"Consultar tutorial");
   }else if(page=="Audio"){
    Volume("Volumen maestro","Ajusta el volumen general, incluida la voz.",DeskPreferences.Current.master,v=>DeskPreferences.Current.master=v);
    Volume("Música","Pendiente: el prototipo no incluye pistas de música activas.",P.music,v=>P.music=v,false);
    Volume("Efectos de sonido","Ajusta los sonidos de las acciones del juego.",DeskPreferences.Current.effects,v=>DeskPreferences.Current.effects=v);
    Volume("Interfaz","Ajusta los sonidos de los controles.",DeskPreferences.Current.ui,v=>DeskPreferences.Current.ui=v);
    Volume("Ambiente","Pendiente: el prototipo no incluye pistas de ambiente activas.",P.ambient,v=>P.ambient=v,false);
    Action("Narración del sistema","Lee información importante con una voz de Windows. "+VoiceStatus(),"audio",()=>Set(()=>{P.narration=!P.narration;if(P.narration)Speech.Say("Presidente, hay un expediente pendiente.",true);else Speech.Stop();}),P.narration&&Speech.Ready?"Activada":"Desactivada",Speech.Ready&&Speech.Error=="");
    Volume("Volumen de narración","Ajusta la voz; también depende del volumen maestro.",P.narrationVolume,v=>P.narrationVolume=v,Speech.Ready);
    Action("Velocidad de voz","Elige el ritmo de lectura que te resulte cómodo.","audio",()=>Set(()=>P.speechRate=P.speechRate<0?0:P.speechRate==0?3:-3),P.speechRate<0?"Lenta":P.speechRate==0?"Normal":"Rápida",Speech.Ready);
    Toggle("Audio mono","Mezcla los canales del juego. Los sonidos actuales ya son mono, por lo que no cambian. No procesa la voz de Windows.","audio",P.mono,()=>P.mono=!P.mono);
    Toggle("Reducir sonidos agudos","Suaviza cambios bruscos de los sonidos del juego; no procesa la voz de Windows.","audio",P.softAudio,()=>P.softAudio=!P.softAudio);
    Toggle("Avisos visuales","Muestra mensajes de los acontecimientos que ya emiten avisos en el juego.","!",P.visualSounds,()=>P.visualSounds=!P.visualSounds);
    Action("Probar narración","Escucha una frase de ejemplo.","audio",()=>Speech.Say("Presidente, hay un expediente pendiente.",true),"Escuchar",P.narration&&Speech.Ready);
    Action("Repetir última narración","Vuelve a escuchar el último mensaje.","audio",Speech.Repeat,"Repetir",P.narration&&Speech.Ready&&Speech.Last!="");
    Action("Detener narración","Interrumpe la lectura actual.","audio",Speech.Stop,"Detener",P.narration&&Speech.Ready);

   }
   if(page!="Inicio"){
    Info("VISTA PREVIA · sin efectos en tu partida",P.simpleLanguage?"Abra el expediente. Lea la petición. Después elija una respuesta.":"Presidente, hay un expediente pendiente de lectura. Revise su contenido antes de responder.","D");
    if(P.independentColor||P.symbols)Info((P.symbols?"[=] ":"")+"Estabilidad: 65 · estable",(P.symbols?"[S] ":"")+"Seguridad: 48 · atención. "+(P.symbols?"[$] ":"")+"Prosperidad: 30 · en riesgo. Valores de ejemplo.","=");
    if(page=="Motor"){
     Action(demoOpened?"Documento de ejemplo abierto":"Documento de ejemplo",demoOpened?"Lectura completada. Puedes repetir la demostración.":demoSelected?"Seleccionado. Pulsa ahora la zona de lectura.":"Activa la selección con clics y pulsa este documento.","D",DemoDocument,"Seleccionar",P.simpleDocuments);
     Action("Zona de lectura",demoOpened?"El documento se ha abierto sin arrastrar.":"Coloca aquí el documento seleccionado.","cursor",DemoPlace,"Colocar",P.simpleDocuments&&demoSelected);
    }
    if(page=="Audio")Action("Ejemplo de aviso",demoNotice&&P.visualSounds?"Expediente recibido. Este es un aviso de demostración.":"Activa los avisos visuales y prueba la notificación.","!",()=>{demoNotice=true;Refresh();},"Mostrar aviso",P.visualSounds);
   }
   Action(page=="Inicio"?"Volver a Ajustes":"Volver a Accesibilidad","Tus preferencias quedan guardadas.","cursor",()=>{page=page=="Inicio"?"":"Inicio";focused=-1;Refresh();},"Volver");
   return a;
  }
  void SetDemoMode(){P.simpleDocuments=!P.simpleDocuments;demoOpened=demoSelected=false;}
  string VoiceStatus()=>Speech.Error!=""?Speech.Error:!Speech.Ready?"Comprobando voces locales; la narración aún no está disponible.":Speech.Voice.Contains("|es-")?"Voz española disponible: "+Speech.Voice.Replace("|"," · "):"No hay voz española disponible. Voz detectada: "+Speech.Voice.Replace("|"," · ")+". Instala una voz española compatible con Windows System.Speech.";
  string ActiveOptions(){var a=new List<string>();if(DeskPreferences.Current.highContrast)a.Add("alto contraste");if(P.simpleDocuments)a.Add("clics simplificados");if(P.keyboard)a.Add("teclado");if(P.largeTargets)a.Add("áreas grandes");if(P.symbols)a.Add("símbolos");if(P.simpleLanguage)a.Add("instrucciones sencillas");if(P.navigationHelp)a.Add("ayudas de navegación");if(P.glossary)a.Add("glosario");if(P.independentColor)a.Add("indicadores con etiquetas");if(P.visualSounds)a.Add("avisos visuales");if(P.narration&&Speech.Ready)a.Add("narración");if(P.mono)a.Add("mono");if(P.softAudio)a.Add("sonidos suaves");if(P.reduceFlashes)a.Add("menos destellos");if(DeskPreferences.Current.reduceMotion)a.Add("menos movimiento");return "Texto "+Mathf.RoundToInt(P.textScale*100)+" % · interfaz "+Mathf.RoundToInt(P.interfaceScale*100)+" %. "+(a.Count==0?"Sin opciones adicionales.":string.Join(", ",a)+".");}
 }
}
