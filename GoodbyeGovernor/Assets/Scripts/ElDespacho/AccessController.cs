using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace ElDespacho {
 [DefaultExecutionOrder(500)] public sealed partial class AccessController:MonoBehaviour {
  public static AccessController Instance {get;private set;}
  public sealed class Entry {public string description,icon,value;public bool card;public float sliderValue;public Action<float> sliderChange;public string text; public Action action;public Transform target;public bool enabled=true;public Entry(string t,Action a=null,Transform o=null,bool e=true){text=t;action=a;target=o;enabled=e;}}
#if ENABLE_INPUT_SYSTEM
  InputActionMap keys; readonly HashSet<string> pressed=new HashSet<string>(); bool mouseClick;
  bool Press(string key)=>pressed.Remove(key);
  void BindKeys(){keys=new InputActionMap("Accessibility09");foreach(var name in new[]{"f1","f2","f3","f6","f7","f8","tab","upArrow","downArrow","enter","space","pageDown","pageUp","leftArrow","rightArrow"}){string id=name;keys.AddAction(id,InputActionType.Button,"<Keyboard>/"+id).performed+=_=>pressed.Add(id);}keys.AddAction("place",InputActionType.Button,"<Mouse>/leftButton").performed+=_=>mouseClick=true;keys.Enable();}
#endif
  DespachoGame game;DeskMenus menus; Canvas canvas,focusCanvas;RectTransform root,content,focusRect;ScrollRect scroll;Image focus;Text notice;
  List<Entry> source=new List<Entry>(), controls=new List<Entry>();List<RectTransform> rows=new List<RectTransform>();
  public readonly AccessSpeech Speech=new AccessSpeech();
  string layoutContext=""; int focused=-1;string signature="",page="";float nextScan;bool force=true;bool reader;bool lastNarration;string lastTutorial="";
  public StampTool SelectedStamp {get;private set;}
  public string CustomPage=>page;
  AccessPreferences P=>AccessPreferences.Current;
  public static string Plain(string t)=>Regex.Replace(t??"","<[^>]*>","");
  public static string Status(int n)=>n<=0?"agotado":n<35?"en riesgo":n<60?"atención":"estable";
  void Awake(){Instance=this;AccessPreferences.Load();game=GetComponent<DespachoGame>();menus=GetComponent<DeskMenus>();Build();
#if ENABLE_INPUT_SYSTEM
   BindKeys();
#endif
  }
  void OnDestroy(){
#if ENABLE_INPUT_SYSTEM
   keys?.Dispose();
#endif
   Speech.Dispose();if(Instance==this)Instance=null;}
  void Build(){var go=new GameObject("Accessibility",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.transform.SetParent(transform,false);canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=700;root=(RectTransform)go.transform;var sc=go.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1920,1080);sc.matchWidthOrHeight=.5f;canvas.enabled=false;}
  public bool IsSource(Transform t){if(t.IsChildOf(root))return false;var c=t.GetComponentInParent<Canvas>();if(c==null||!c.enabled||!c.gameObject.activeInHierarchy)return false;if(c.sortingOrder==100 && menus.IsVisible)return false;return c.sortingOrder==100||c.sortingOrder==400||c.sortingOrder==500||c.sortingOrder==300;}
  public List<Entry> Snapshot(){
   var list=new List<Entry>();
   if(P.navigationHelp)list.Add(new Entry(menus.IsVisible?"[>] Elija una opción. ESC vuelve a la pantalla anterior.":game.ScreenName=="Despacho"?"[>] Abra el expediente, lea y elija un sello. Después, continúe. También puede abrir el mapa.":game.ScreenName=="Mapa"?"[>] Elija un país disponible. Las marcas de amenaza y aliado son sospechas.":"[>] Lea la información. Elija una pregunta, una pestaña o una decisión."));
   if(P.glossary||P.symbols)list.Add(new Entry("[?] Consultar símbolos y términos",()=>{page="Glosario";Refresh();}));
   if(!menus.IsVisible&&game.HasSession && (P.independentColor||P.symbols||P.narration)) foreach(var indicator in game.AccessIndicators().Split(new[]{". "},StringSplitOptions.RemoveEmptyEntries))list.Add(new Entry(indicator));
   foreach(var t in game.GetComponentsInChildren<Text>(false)){
    if(!IsSource(t.transform)||!t.enabled||t.GetComponentInParent<Button>()!=null||string.IsNullOrWhiteSpace(t.text))continue;
    if(!menus.IsVisible&&(P.independentColor||P.symbols||P.narration)&&t.GetComponentsInParent<Transform>().Any(x=>x.name=="HUD"))continue;
    list.Add(new Entry(Plain(t.text),null,t.transform));
   }
   foreach(var b in game.GetComponentsInChildren<Button>(false)){
    if(!IsSource(b.transform)||!b.enabled)continue;
    string label=string.Join(" ",b.GetComponentsInChildren<Text>().Select(t=>Plain(t.text)));
    var semantic=b.GetComponent<AccessName>();if(semantic!=null)label=semantic.label;
    if(string.IsNullOrWhiteSpace(label))label=b.name;
    var button=b;list.Add(new Entry(label,()=>{if(button!=null&&button.IsActive()&&button.IsInteractable())button.onClick.Invoke();},b.transform,b.IsInteractable()));
   }
   foreach(var slider in game.GetComponentsInChildren<Slider>(false))if(IsSource(slider.transform)&&slider.IsInteractable()){
    var s=slider;var txt=s.transform.parent.GetComponentsInChildren<Text>().Where(t=>t.rectTransform.anchorMin.y>=((RectTransform)s.transform).anchorMin.y).OrderBy(t=>Mathf.Abs(t.rectTransform.anchorMin.y-((RectTransform)s.transform).anchorMin.y)).FirstOrDefault();string label=txt!=null?Plain(txt.text):"Valor";
    list.Add(new Entry(label+" - reducir",()=>s.value=Mathf.Max(s.minValue,s.value-(s.maxValue-s.minValue)/10),s.transform));
    list.Add(new Entry(label+" - aumentar",()=>s.value=Mathf.Min(s.maxValue,s.value+(s.maxValue-s.minValue)/10),s.transform));
   }
   if(!menus.IsVisible){
    foreach(var d in game.GetComponentsInChildren<DeskDocument>())if(!d.CanDecide&&d.AccessAvailable){var doc=d;list.Add(new Entry(doc.AccessSelected?"[>] Colocar expediente en zona de lectura":"[D] Seleccionar expediente",()=>{if(doc.AccessSelected)doc.AccessPlace();else doc.AccessSelect();},d.transform));}
    foreach(var s in game.GetComponentsInChildren<StampTool>())if(s.AccessAvailable){var stamp=s;list.Add(new Entry("[S] Seleccionar sello: "+Plain(string.Join(" ",s.GetComponentsInChildren<Text>().Select(t=>t.text))),()=>SelectStamp(stamp),s.transform));}
    if(SelectedStamp!=null)list.Add(new Entry("[V] Aplicar sello seleccionado al expediente",()=>{var s=SelectedStamp;SelectedStamp=null;s.AccessApply();}));
   }
   return list;
  }
  void Update(){
   if(SelectedStamp!=null&&!SelectedStamp.AccessAvailable)SelectedStamp=null;
   if(!P.narration&&lastNarration)Speech.Stop();lastNarration=P.narration;
   if(EventSystem.current!=null && (P.keyboard || page!=""))EventSystem.current.sendNavigationEvents=false;
   if(!P.keyboard&&page==""&&EventSystem.current!=null)EventSystem.current.sendNavigationEvents=true;
#if ENABLE_INPUT_SYSTEM
   var k=Keyboard.current;
   if(k!=null){
    if(Press("f1"))ShowReference();
    if(Press("f2")){reader=!reader;force=true;}
    if(Press("f3"))Speech.Say(string.Join(". ",Snapshot().Where(x=>x.action==null).Select(x=>x.text)),true);
    if(KeyPressed(P.repeatKey))Speech.Repeat();if(KeyPressed(P.stopKey))Speech.Stop();
    if(P.keyboard || page!=""){if(Press("tab")||Press("downArrow"))MoveFocus(k.shiftKey.isPressed?-1:1);if(Press("upArrow"))MoveFocus(-1);if(Press("enter")||Press("space"))Activate();if(Press("leftArrow"))AdjustFocused(-1);if(Press("rightArrow"))AdjustFocused(1);}
    if(canvas.enabled&&scroll!=null){if(Press("pageDown"))scroll.verticalNormalizedPosition-=.2f;if(Press("pageUp"))scroll.verticalNormalizedPosition+=.2f;}
   }
   if(P.simpleDocuments && !canvas.enabled && !menus.IsVisible && Mouse.current!=null && mouseClick){
    var pos=Mouse.current.position.ReadValue();
    if(SelectedStamp!=null){var s=SelectedStamp;if(s.AccessContains(pos)){SelectedStamp=null;s.AccessApply();}else if(!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)s.transform,pos)){SelectedStamp=null;Notify("Selección cancelada");}}
    foreach(var doc in game.GetComponentsInChildren<DeskDocument>())if(doc.AccessSelected&&!doc.AccessOnSheet(pos)){if(doc.AccessOnTray(pos))doc.AccessPlace();else doc.AccessCancel();}
   }
   pressed.Clear();mouseClick=false;
#endif
  }
#if ENABLE_INPUT_SYSTEM
  bool KeyPressed(int n)=>Press("f"+n);
#endif
  void LateUpdate(){
   if(SettingsPointerDrag)return;
   if(Time.unscaledTime<nextScan&&!force){DrawFocus();return;}nextScan=Time.unscaledTime+.15f;
   var list=page!=""?SettingsEntries():Snapshot();string sig=Speech.Ready+"|"+Speech.Error+"|"+Speech.Voice+"|"+page+"|"+JsonUtility.ToJson(P)+"|"+DeskPreferences.Current.highContrast+"|"+string.Join("|",list.Select(x=>x.text+":"+x.enabled));
   bool show=page!=""||reader||P.Reflow;
   if(sig!=signature||force||canvas.enabled!=show){
    bool changed=sig!=signature;signature=sig;source=list;force=false;
    canvas.enabled=show;
    if(show)Render(list);else {controls=list.Where(x=>x.action!=null&&x.enabled).ToList();if(focused>=controls.Count)focused=-1;}
    if(changed&&P.narration)Speech.Say((page!=""?"Accesibilidad. ":menus.IsVisible?menus.PageName+". ":game.ScreenName+". ")+string.Join(". ",list.Where(x=>x.action==null).Select(x=>x.text)));
   }
   var tutorial=TutorialManager.Instance;
   if(tutorial!=null && tutorial.IsActive && tutorial.ActiveId!=lastTutorial){lastTutorial=tutorial.ActiveId;if(P.narration){var s=tutorial.Steps.First(x=>x.persistentId==lastTutorial);Speech.Say(TutorialLessons.Text(s,game.TutorialQuestions));}}
   DrawFocus();
  }
  public void Refresh(){signature="";force=true;}
  public void SelectStamp(StampTool stamp){SelectedStamp=stamp;stamp.AccessPreview();Notify("Sello seleccionado. Pulse el documento para confirmar; ESC cancela.");Refresh();}
  public bool Escape(){if(SelectedStamp!=null){SelectedStamp=null;Notify("Sello cancelado");return true;}foreach(var d in game.GetComponentsInChildren<DeskDocument>())if(d.AccessSelected){d.AccessCancel();return true;}if(page!=""){if(page=="Inicio")page="";else page="Inicio";Refresh();return true;}return false;}
  public static bool CancelOrBack()=>Instance!=null&&Instance.Escape();
  public void ShowSettings(){Speech.Probe();page="Inicio";focused=-1;Refresh();}
  public void ClosePanels(){page="";SelectedStamp=null;Refresh();}
  public void ShowReference(){if(!menus.IsVisible&&game.HasSession)menus.ShowPause();page="Controles";Refresh();}
  public void Notify(string message){if(P.visualSounds||P.navigationHelp){if(notice!=null)notice.text=message;}Speech.Say(message);}
  public static void Cue(string message){Instance?.Notify(message);}
  public void MoveFocus(int direction){if(controls.Count==0)return;focused=(focused+direction+controls.Count)%controls.Count;var e=controls[focused];Speech.Say(e.text);if(canvas.enabled&&scroll!=null&&focused<rows.Count){Canvas.ForceUpdateCanvases();float y=-rows[focused].anchoredPosition.y;float travel=Mathf.Max(1,content.rect.height-scroll.viewport.rect.height);scroll.verticalNormalizedPosition=1-Mathf.Clamp01((y-80)/travel);}DrawFocus();}
  public void Activate(){if(focused<0||focused>=controls.Count)return;var action=controls[focused].action;action?.Invoke();force=true;}
  void DrawFocus(){
   bool show=(P.keyboard||page!="")&&focused>=0&&focused<controls.Count;
   if(focusCanvas==null){var go=new GameObject("KeyboardFocus",typeof(RectTransform),typeof(Canvas));go.transform.SetParent(transform,false);focusCanvas=go.GetComponent<Canvas>();focusCanvas.renderMode=RenderMode.ScreenSpaceOverlay;focusCanvas.sortingOrder=750;focusRect=Kit.Rect(go.transform,"Focus",Vector2.zero,Vector2.zero);focusRect.pivot=Vector2.zero;
    Kit.Panel(focusRect,Pal.Gold,new Vector2(0,0),new Vector2(1,0),new Vector2(0,-3),new Vector2(0,3));Kit.Panel(focusRect,Pal.Gold,new Vector2(0,1),new Vector2(1,1),new Vector2(0,-3),new Vector2(0,3));Kit.Panel(focusRect,Pal.Gold,new Vector2(0,0),new Vector2(0,1),new Vector2(-3,0),new Vector2(3,0));Kit.Panel(focusRect,Pal.Gold,new Vector2(1,0),new Vector2(1,1),new Vector2(-3,0),new Vector2(3,0));}
   focusCanvas.enabled=show;if(!show)return;var target=controls[focused].target as RectTransform;if(target==null){focusCanvas.enabled=false;return;}var corners=new Vector3[4];target.GetWorldCorners(corners);var min=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var max=RectTransformUtility.WorldToScreenPoint(null,corners[2]);focusRect.anchoredPosition=min;focusRect.sizeDelta=max-min;
  }
  void Render(List<Entry> entries){
   if(page!=""){RenderSettings(entries);return;}
   string context=page+"|"+menus.PageName+"|"+game.ScreenName;float previousScroll=scroll!=null&&context==layoutContext?scroll.verticalNormalizedPosition:1;layoutContext=context;
   foreach(Transform child in root){child.gameObject.SetActive(false);Destroy(child.gameObject);}rows.Clear();controls.Clear();
   var dark=DeskPreferences.Current.highContrast;var bg=Kit.Panel(root,dark?Color.black:Pal.Wood,Vector2.zero,Vector2.one);
   var head=Kit.Label(bg.transform,"EL DESPACHO · "+(page!=""?"ACCESIBILIDAD / "+page:"VISTA DE LECTURA"),Mathf.RoundToInt(24*P.textScale),Pal.Cream,TextAnchor.MiddleLeft,new Vector2(.035f,.92f),new Vector2(.96f,.99f));
   notice=Kit.Label(bg.transform,"TAB: elegir · INTRO: activar · RePág/AvPág: desplazar · ESC: volver · F1: ayuda",Mathf.RoundToInt(16*P.textScale),Pal.Cream,TextAnchor.MiddleLeft,new Vector2(.035f,.01f),new Vector2(.96f,.065f));
   var viewport=Kit.Panel(bg.transform,dark?Color.black:Pal.Paper,new Vector2(.035f,.075f),new Vector2(.965f,.915f));viewport.raycastTarget=true;viewport.gameObject.AddComponent<RectMask2D>();
   content=Kit.Rect(viewport.transform,"Flow",new Vector2(0,1),new Vector2(1,1));content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(0,1);
   scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport.rectTransform;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=70;
   Canvas.ForceUpdateCanvases();float width=viewport.rectTransform.rect.width;float y=16;float size=24*P.textScale*P.interfaceScale;
   foreach(var entry in entries){
    string label=entry.text+(entry.action!=null&&!entry.enabled?" (no disponible)":"");
    var rt=Kit.Rect(content,"Entry",new Vector2(0,1),new Vector2(1,1));rt.pivot=new Vector2(.5f,1);rt.offsetMin=new Vector2(18,0);rt.offsetMax=new Vector2(-18,0);
    var text=Kit.Label(rt,label,Mathf.RoundToInt(size),dark?Color.white:Pal.Ink,TextAnchor.UpperLeft,Vector2.zero,Vector2.one,new Vector2(18,10),new Vector2(-18,-10));text.supportRichText=false;text.verticalOverflow=VerticalWrapMode.Overflow;
    if(P.symbols && label.StartsWith("["))text.rectTransform.offsetMin=new Vector2(56*P.interfaceScale,10);
    Canvas.ForceUpdateCanvases();float height=Mathf.Max(entry.action==null?45:64*P.interfaceScale,text.preferredHeight+28);rt.sizeDelta=new Vector2(-36,height);rt.anchoredPosition=new Vector2(0,-y);y+=height+12;
    if(P.symbols && label.StartsWith("[")){var iconRect=Kit.Rect(rt,"Symbol",new Vector2(0,1),new Vector2(0,1));iconRect.pivot=new Vector2(0,1);iconRect.anchoredPosition=new Vector2(12,-12);iconRect.sizeDelta=new Vector2(30,30)*P.interfaceScale;var icon=iconRect.gameObject.AddComponent<AccessSymbol>();icon.symbol=label.Length>1?label.Substring(1,1):"D";icon.color=dark?Color.white:Pal.Ink;icon.raycastTarget=false;text.rectTransform.offsetMin=new Vector2(56*P.interfaceScale,10);}
    if(entry.action!=null){var img=rt.gameObject.AddComponent<Image>();img.color=dark?Kit.Hex("#202020"):Pal.PaperDark;img.raycastTarget=true;var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=img;button.interactable=entry.enabled;var e=entry;button.onClick.AddListener(()=>{e.action?.Invoke();Refresh();});if(entry.enabled){rows.Add(rt);controls.Add(new Entry(label,e.action,rt));}}
   }
   content.sizeDelta=new Vector2(0,y+15);scroll.verticalNormalizedPosition=previousScroll;if(focused>=controls.Count)focused=-1;
  }
  void Set(Action a){a();P.Clamp();bool saved=P.Save();applied=saved?"Cambios aplicados y guardados.":AccessPreferences.Error;DeskPreferences.Current.Apply(false);try{DeskPreferences.Current.Save();}catch(Exception e){applied="No se guardaron todos los ajustes: "+e.Message;Notify(applied);}Refresh();}
  static float Cycle(float v,float[] values){int i=Array.FindIndex(values,x=>Mathf.Abs(x-v)<.01f);return values[(i+1)%values.Length];}
  List<Entry> LegacySettingsEntries(){var a=new List<Entry>();void Text(string t)=>a.Add(new Entry(t));void Button(string t,Action f)=>a.Add(new Entry(t,f));void Toggle(string t,bool v,Action f)=>Button(t+": "+(v?"SÍ":"NO"),()=>Set(f));void Volume(string t,float v,Action<float> f){Button(t+": "+Mathf.RoundToInt(v*100)+" % - reducir",()=>Set(()=>f(Mathf.Max(0,v-.1f))));Button(t+" - aumentar",()=>Set(()=>f(Mathf.Min(1,v+.1f))));}
   if(page=="Inicio"){
    Text("Preferencias personales. Se aplican al cambiar y se guardan fuera de la campaña. Texto grande, escala, contraste y áreas grandes utilizan una vista con desplazamiento en todas las pantallas. F2 permite abrir también esa vista.");
    foreach(string name in new[]{"Visual","Motor","Cognitiva","Audio","General"}){string n=name;Button(n,()=>{page=n;focused=-1;Refresh();});}
   }else if(page=="Visual"){
    Toggle("Alto contraste",DeskPreferences.Current.highContrast,()=>DeskPreferences.Current.highContrast=!DeskPreferences.Current.highContrast);
    Button("Tamaño de texto: "+Mathf.RoundToInt(P.textScale*100)+" %",()=>Set(()=>P.textScale=Cycle(P.textScale,new[]{.75f,1f,1.25f,1.5f,1.75f})));
    Button("Escala de interfaz: "+Mathf.RoundToInt(P.interfaceScale*100)+" %",()=>Set(()=>P.interfaceScale=Cycle(P.interfaceScale,new[]{.8f,.9f,1f,1.1f,1.2f})));
    Toggle("Indicadores sin dependencia del color",P.independentColor,()=>P.independentColor=!P.independentColor);
    Toggle("Reducir animaciones y temblores",DeskPreferences.Current.reduceMotion,()=>DeskPreferences.Current.reduceMotion=!DeskPreferences.Current.reduceMotion);
    Toggle("Reducir destellos y pulso del tutorial",P.reduceFlashes,()=>P.reduceFlashes=!P.reduceFlashes);
    Text("No existe movimiento de cámara en este prototipo. Reducir animaciones también simplifica abrir y archivar documentos.");
   }else if(page=="Motor"){
    Toggle("Selección simplificada: clic en documento y clic en zona",P.simpleDocuments,()=>P.simpleDocuments=!P.simpleDocuments);
    Toggle("Navegación por teclado",P.keyboard,()=>P.keyboard=!P.keyboard);
    Toggle("Áreas amplias: controles en filas grandes",P.largeTargets,()=>P.largeTargets=!P.largeTargets);
    Button("Controles y atajos",()=>{page="Controles";Refresh();});
    Text("Sello simplificado: elija el sello y pulse el documento para confirmar. ESC cancela. No hay límite de tiempo para decidir.");
   }else if(page=="Cognitiva"){
    Toggle("Instrucciones simplificadas",P.simpleLanguage,()=>P.simpleLanguage=!P.simpleLanguage);
    Toggle("Ayudas de navegación",P.navigationHelp,()=>P.navigationHelp=!P.navigationHelp);
    Toggle("Símbolos explicativos",P.symbols,()=>P.symbols=!P.symbols);
    Toggle("Definiciones de términos",P.glossary,()=>P.glossary=!P.glossary);
    Button("Consultar símbolos y términos",()=>{page="Glosario";Refresh();});
    Text("Las instrucciones cambian; las peticiones, respuestas políticas e información de los países conservan su significado original.");
   }else if(page=="Audio"){
    Volume("Maestro",DeskPreferences.Current.master,v=>DeskPreferences.Current.master=v);Volume("Música",P.music,v=>P.music=v);Volume("Efectos",DeskPreferences.Current.effects,v=>DeskPreferences.Current.effects=v);Volume("Interfaz",DeskPreferences.Current.ui,v=>DeskPreferences.Current.ui=v);Volume("Ambiente",P.ambient,v=>P.ambient=v);
    Text("Este prototipo no contiene pistas de música ni ambiente. Sus controles quedan preparados para fuentes etiquetadas; no generan sonidos nuevos.");
    Toggle("Narración local Windows",P.narration,()=>{P.narration=!P.narration;if(P.narration)Speech.Say("Narración activada. El Despacho.",true);else Speech.Dispose();});Volume("Narración",P.narrationVolume,v=>P.narrationVolume=v);
    Button("Velocidad de voz: "+P.speechRate,()=>Set(()=>P.speechRate=P.speechRate==5?-5:P.speechRate+1));Button("Probar voz",()=>Speech.Say("Presidente, hay un expediente pendiente. Estabilidad, seguridad y prosperidad.",true));Button("Repetir narración",Speech.Repeat);Button("Detener narración",Speech.Stop);
    Toggle("Audio mono",P.mono,()=>P.mono=!P.mono);Toggle("Reducir sonidos agudos",P.softAudio,()=>P.softAudio=!P.softAudio);Toggle("Avisos visuales de sonidos",P.visualSounds,()=>P.visualSounds=!P.visualSounds);
    Text("La voz necesita Windows PowerShell y una voz de Windows instalada; se prefiere una voz española. No es un lector de pantalla del sistema. Mono y reducción de agudos procesan sonidos Unity; la voz Windows usa la salida del sistema.");if(Speech.Voice!="")Text("Voz instalada: "+Speech.Voice+". Si no es española, instale una voz española compatible con System.Speech en Windows.");if(Speech.Error!="")Text(Speech.Error);
   }else if(page=="Controles"){
    Text("F1: controles y acceso a opciones. F2: vista de lectura. F3: leer la pantalla. ESC: cancelar selección, volver o pausa. Con teclado activado: TAB/Mayús+TAB o flechas arriba/abajo eligen; INTRO/Espacio activan. RePág/AvPág desplazan la vista. Los expedientes y sellos requieren seleccionar y después confirmar, igual que con ratón simplificado.");
    Button("Repetir voz: F"+P.repeatKey,()=>Set(()=>{P.repeatKey=P.repeatKey==8?6:P.repeatKey+1;}));Button("Detener voz: F"+P.stopKey,()=>Set(()=>{P.stopKey=P.stopKey==8?6:P.stopKey+1;}));
    Toggle("Activar navegación por teclado",P.keyboard,()=>P.keyboard=!P.keyboard);
   }else if(page=="Glosario"){
    Text("Símbolos: [A] Aurelia, su país; [=] Estabilidad; [S] Seguridad; [$] Prosperidad; [!] posible amenaza; [+] posible aliado; [V] visitado/confirmado; [?] sin visitar; [D] documento; [>] siguiente acción; [X] cancelado. Amenaza y aliado son sospechas, no certezas. + / - indican aumento o descenso, no siempre una decisión buena o mala.");
    Text("Sanciones: restricciones económicas contra otro país. Relaciones diplomáticas: contactos oficiales entre gobiernos. Pacto militar: acuerdo de defensa. Acuerdo comercial: reglas para comprar y vender entre países. Gobierno autoritario: concentra el poder y limita la participación. Recesión: caída de la actividad económica. Inteligencia: información sobre riesgos; puede ser incorrecta o antigua.");
   }else if(page=="General"){
    Button("Aplicar y guardar",()=>Set(()=>{}));Button("Restaurar accesibilidad",()=>{page="Restaurar";Refresh();});Button("Controles",()=>{page="Controles";Refresh();});Text(AccessPreferences.Error==""?"Preferencias guardadas independientemente de los slots de campaña.":AccessPreferences.Error);
   }else if(page=="Restaurar"){
    Text("Se restablecerán accesibilidad, contraste y movimiento. La campaña y sus guardados no cambian.");Button("Confirmar restauración",()=>{AccessPreferences.Reset();Speech.Dispose();reader=false;page="Inicio";Refresh();});
   }
   Button(page=="Inicio"?"Volver a Ajustes":"Volver a Accesibilidad",()=>{page=page=="Inicio"?"":"Inicio";Refresh();});return a;
  }
 }
 public sealed class AccessName:MonoBehaviour {public string label;}
}
