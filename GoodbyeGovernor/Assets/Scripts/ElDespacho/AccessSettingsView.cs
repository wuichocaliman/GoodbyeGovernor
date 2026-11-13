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
  public void OpenCategory(string name){page=name;focused=-1;Refresh();}
  public void ApplyProfile(int profile){Set(()=>{
   if(profile==0){P.textScale=1.5f;DeskPreferences.Current.highContrast=true;P.independentColor=true;P.largeTargets=true;}
   if(profile==1){P.simpleDocuments=true;P.largeTargets=true;}
   if(profile==2){P.simpleLanguage=true;P.navigationHelp=true;P.symbols=true;P.glossary=true;}
   if(profile==3){P.visualSounds=true;P.narration=Speech.Ready&&Speech.Error=="";}
  });P.profileMask|=1<<profile;P.profileSnapshot=ProfileSnapshot();P.Save();Refresh();}
  string ProfileSnapshot(){var old=P.profileSnapshot;P.profileSnapshot="";var s=JsonUtility.ToJson(P)+DeskPreferences.Current.highContrast+DeskPreferences.Current.reduceMotion+"|"+DeskPreferences.Current.master+"|"+DeskPreferences.Current.effects+"|"+DeskPreferences.Current.ui;P.profileSnapshot=old;return s;}
  public string ConfigurationStatus=>P.profileMask!=0&&P.profileSnapshot==ProfileSnapshot()?"Configuración recomendada: "+string.Join(" + ",new[]{0,1,2,3}.Where(i=>(P.profileMask&(1<<i))!=0).Select(i=>new[]{"Lectura cómoda","Interacción sencilla","Comprensión guiada","Escucha asistida"}[i])):"Configuración personalizada";
  public void DemoDocument(){if(!P.simpleDocuments)return;demoSelected=true;demoOpened=false;Refresh();}
  public void DemoPlace(){if(!P.simpleDocuments||!demoSelected)return;demoOpened=true;demoSelected=false;Refresh();}
  public bool DemoOpened=>demoOpened;
  public void AdjustFocused(float direction){if(focused<0||focused>=controls.Count)return;var slider=controls[focused].target.GetComponentInChildren<Slider>();if(slider!=null&&slider.interactable)slider.value=Mathf.Clamp01(slider.value+direction*.05f);}
  List<Entry> SettingsEntries(){
   if(page=="Controles"||page=="Glosario"||page=="Restaurar")return LegacySettingsEntries();
   var a=new List<Entry>();
   void Info(string title,string desc,string icon="D")=>a.Add(new Entry(title){description=desc,icon=icon});
   void Action(string title,string desc,string icon,Action f,string value="",bool enabled=true)=>a.Add(new Entry(title,f,null,enabled){description=desc,icon=icon,value=value});
   void Toggle(string title,string desc,string icon,bool value,Action f)=>Action(title,desc,icon,()=>Set(f),value?"Activado":"Desactivado");
   void Volume(string title,string desc,float value,Action<float> f,bool enabled=true)=>a.Add(new Entry(title,()=>{},null,enabled){description=desc,icon="audio",value=Mathf.RoundToInt(value*100)+" %",sliderValue=value,sliderChange=v=>Set(()=>f(v))});
   if(page=="Inicio"){
    Info("Personaliza cómo quieres jugar.","Adapta la lectura, los controles, las instrucciones y el sonido a tus necesidades.","access");
    Info("Perfiles recomendados",ConfigurationStatus+". Puedes combinarlos y ajustar cada opción después.","gear");
    string[] names={"Lectura cómoda","Interacción sencilla","Comprensión guiada","Escucha asistida"};
    string[] descriptions={"Facilita la lectura con textos grandes, mayor contraste y elementos más visibles.","Utiliza clics y controles más grandes para interactuar con documentos y menús.","Recibe instrucciones más sencillas, símbolos explicativos y ayudas durante la partida.","Escucha información importante y recibe avisos visuales de los acontecimientos."};
    string[] icons={"eye","cursor","book","audio"};
    for(int i=0;i<4;i++){int n=i;Action(names[i],descriptions[i]+(i==3?" "+VoiceStatus():""),icons[i],()=>ApplyProfile(n),"Activar perfil");a[a.Count-1].card=true;}
    Info("Ajustes por categoría","Selecciona una categoría para personalizar las opciones.","gear");
    string[] ids={"Visual","Motor","Cognitiva","Audio","General"},titles={"VISIÓN","MOVIMIENTO Y CONTROLES","COMPRENSIÓN","AUDIO","GENERAL"},desc={"Texto, contraste y lectura.","Documentos, teclado y animaciones.","Instrucciones, símbolos y tutorial.","Volúmenes, voz y avisos.","Estado, preferencias y restauración."};
    for(int i=0;i<5;i++){string id=ids[i];Action(titles[i],desc[i],new[]{"eye","cursor","book","audio","gear"}[i],()=>OpenCategory(id),"Abrir");}
   }else if(page=="Visual"){
    Toggle("Alto contraste","Mejora la diferencia entre el texto y el fondo.","eye",DeskPreferences.Current.highContrast,()=>DeskPreferences.Current.highContrast=!DeskPreferences.Current.highContrast);
    Action("Tamaño de texto","Cambia las letras en la vista de lectura, los menús y las instrucciones.","eye",()=>Set(()=>P.textScale=Cycle(P.textScale,new[]{.75f,1f,1.25f,1.5f,1.75f})),Mathf.RoundToInt(P.textScale*100)+" %");
    Action("Escala de interfaz","Amplía los elementos de la vista de lectura y de este centro.","eye",()=>Set(()=>P.interfaceScale=Cycle(P.interfaceScale,new[]{.8f,.9f,1f,1.1f,1.2f})),Mathf.RoundToInt(P.interfaceScale*100)+" %");
    Toggle("Indicadores con etiquetas","Muestra símbolos, nombres y estado sin depender solo del color.","=",P.independentColor,()=>P.independentColor=!P.independentColor);
    Toggle("Reducir destellos","Elimina el pulso del tutorial y reduce los temblores de la interfaz.","eye",P.reduceFlashes,()=>P.reduceFlashes=!P.reduceFlashes);
    Toggle("Preferencia de lectura","Recuerda que prefieres la alternativa de texto. No la abre al iniciar ni al cargar una campaña; pulsa F2 o Abrir ahora.","D",P.readingView,()=>P.readingView=!P.readingView);
    Action("Vista de lectura","Abre la alternativa de texto para esta sesión. F2, ESC o Volver al modo visual permiten salir. Los demás ajustes nunca la abren automáticamente.","D",OpenReadingView,"Abrir ahora");
   }else if(page=="Motor"){
    Toggle("Seleccionar documentos con clics","Pulsa un documento y después la zona de lectura. No hace falta arrastrarlo. Desactivado: arrastra el documento.","cursor",P.simpleDocuments,()=>SetDemoMode());
    Toggle("Navegación por teclado","TAB elige; INTRO activa; las flechas izquierda y derecha ajustan los volúmenes.","cursor",P.keyboard,()=>P.keyboard=!P.keyboard);
    Toggle("Botones y zonas más grandes","Activa controles amplios en la vista de lectura.","cursor",P.largeTargets,()=>P.largeTargets=!P.largeTargets);
    Toggle("Reducir animaciones","Simplifica la apertura y el archivo de documentos y elimina temblores.","cursor",DeskPreferences.Current.reduceMotion,()=>DeskPreferences.Current.reduceMotion=!DeskPreferences.Current.reduceMotion);
    Action("Controles y atajos","Consulta los controles y cambia las teclas de repetir y detener voz.","cursor",()=>OpenCategory("Controles"),"Consultar");
    Info("Movimiento de cámara","Este prototipo no tiene movimiento de cámara.","cursor");
   }else if(page=="Cognitiva"){
    Toggle("Instrucciones sencillas","Utiliza explicaciones más cortas en el tutorial. La narrativa conserva su significado.","book",P.simpleLanguage,()=>P.simpleLanguage=!P.simpleLanguage);
    Toggle("Ayudas de navegación","Añade instrucciones sobre el siguiente paso. El teclado muestra un marco de enfoque.","cursor",P.navigationHelp,()=>P.navigationHelp=!P.navigationHelp);
    Toggle("Símbolos explicativos","Acompaña las etiquetas con símbolos de significado constante.","=",P.symbols,()=>P.symbols=!P.symbols);
    Toggle("Glosario disponible","Añade un acceso a los términos políticos en la vista de lectura.","book",P.glossary,()=>P.glossary=!P.glossary);
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
    Toggle("Audio mono","Combina los canales de los sonidos del juego. La voz usa la salida de Windows.","audio",P.mono,()=>P.mono=!P.mono);
    Toggle("Reducir sonidos agudos","Suaviza cambios bruscos de los sonidos del juego; no procesa la voz de Windows.","audio",P.softAudio,()=>P.softAudio=!P.softAudio);
    Toggle("Avisos visuales","Muestra mensajes de los acontecimientos que ya emiten avisos en el juego.","!",P.visualSounds,()=>P.visualSounds=!P.visualSounds);
    Action("Probar narración","Escucha una frase de ejemplo.","audio",()=>Speech.Say("Presidente, hay un expediente pendiente.",true),"Escuchar",P.narration&&Speech.Ready);
    Action("Repetir última narración","Vuelve a escuchar el último mensaje.","audio",Speech.Repeat,"Repetir",P.narration&&Speech.Ready&&Speech.Last!="");
    Action("Detener narración","Interrumpe la lectura actual.","audio",Speech.Stop,"Detener",P.narration&&Speech.Ready);
   }else if(page=="General"){
    Info("Estado de accesibilidad",ConfigurationStatus,"access");
    Info("Preferencias personales","Se guardan automáticamente fuera de los archivos de campaña. Cargar una partida no cambia tus ajustes.","gear");
    Info("Opciones activas",ActiveOptions(),"gear");
    Action("Aplicar cambios","Guarda los ajustes actuales.","gear",()=>Set(()=>{}),"Guardar");
    Action("Restaurar valores","Solicita confirmación antes de restaurar accesibilidad, contraste y movimiento. No altera las partidas.","gear",()=>OpenCategory("Restaurar"),"Restaurar…");
    Action("Controles y atajos","Consulta y personaliza las teclas de voz.","cursor",()=>OpenCategory("Controles"),"Consultar");
   }
   if(page!="Inicio"){
    Info("VISTA PREVIA · sin efectos en tu partida","Presidente, hay un expediente pendiente de lectura.","D");
    if(P.independentColor||P.symbols)Info("[=] Estabilidad: 65 · estable","[S] Seguridad: 48 · atención. [$] Prosperidad: 30 · en riesgo. Valores de ejemplo.","=");
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
  string ActiveOptions(){var a=new List<string>();if(DeskPreferences.Current.highContrast)a.Add("alto contraste");if(P.readingView)a.Add("preferencia de lectura (apertura manual)");if(P.simpleDocuments)a.Add("clics simplificados");if(P.keyboard)a.Add("teclado");if(P.largeTargets)a.Add("áreas grandes");if(P.symbols)a.Add("símbolos");if(P.simpleLanguage)a.Add("instrucciones sencillas");if(P.navigationHelp)a.Add("ayudas de navegación");if(P.glossary)a.Add("glosario");if(P.independentColor)a.Add("indicadores con etiquetas");if(P.visualSounds)a.Add("avisos visuales");if(P.narration&&Speech.Ready)a.Add("narración");if(P.mono)a.Add("mono");if(P.softAudio)a.Add("sonidos suaves");if(P.reduceFlashes)a.Add("menos destellos");if(DeskPreferences.Current.reduceMotion)a.Add("menos movimiento");return "Texto "+Mathf.RoundToInt(P.textScale*100)+" % · interfaz "+Mathf.RoundToInt(P.interfaceScale*100)+" %. "+(a.Count==0?"Sin opciones adicionales.":string.Join(", ",a)+".");}
 }
}
