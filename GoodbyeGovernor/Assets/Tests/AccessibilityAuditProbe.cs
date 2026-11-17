#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEngine.EventSystems;using UnityEditor;using ElDespacho;
public sealed class AccessibilityAuditProbe:MonoBehaviour {
 AccessController a;DeskMenus menus;List<string> log=new List<string>(),audit=new List<string>();const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 UnityEngine.InputSystem.Keyboard keyboard;
 IEnumerator Key(UnityEngine.InputSystem.Key key){UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(key));yield return Tick();UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());yield return Tick();yield return new WaitForSecondsRealtime(.4f);}
 AccessPreferences P=>AccessPreferences.Current;Transform UI=>a.transform.Find("Accessibility");
 void Check(bool ok,string name){log.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines("Logs/Accessibility-v095.txt",log);if(!ok)throw new Exception(name);}
 void Error(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Assert||t==LogType.Exception){File.AppendAllText("Logs/Accessibility-v095.txt",m+stack);EditorApplication.Exit(1);}}
 IEnumerator Tick(){a.Refresh();yield return null;yield return null;yield return null;Canvas.ForceUpdateCanvases();}
 List<AccessController.Entry> Entries()=>(List<AccessController.Entry>)typeof(AccessController).GetMethod("SettingsEntries",F).Invoke(a,null);
 List<AccessController.Entry> Controls()=>(List<AccessController.Entry>)typeof(AccessController).GetField("controls",F).GetValue(a);
 void Click(string name){var rt=UI.GetComponentsInChildren<RectTransform>().First(x=>x.name=="Setting: "+name);var b=rt.GetComponentInChildren<Button>();Check(b!=null&&b.interactable,"enabled button "+name);ExecuteEvents.Execute(b.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
 void FocusVisible(){var list=Controls();for(int i=0;i<list.Count;i++){a.MoveFocus(1);Canvas.ForceUpdateCanvases();int index=(int)typeof(AccessController).GetField("focused",F).GetValue(a);var t=(RectTransform)list[index].target;var s=t.GetComponentInParent<ScrollRect>();if(s==null)continue;var b=RectTransformUtility.CalculateRelativeRectTransformBounds(s.viewport,t);Check(b.min.y>=s.viewport.rect.yMin-1&&b.max.y<=s.viewport.rect.yMax+1,"focused control fully visible: "+list[index].text.Split('.')[0]);}}
 string Values()=>P.textScale+"|"+P.interfaceScale+"|"+P.simpleDocuments+"|"+P.largeTargets+"|"+P.independentColor+"|"+P.simpleLanguage+"|"+P.navigationHelp+"|"+P.symbols+"|"+P.glossary+"|"+P.visualSounds+"|"+P.narration+"|"+DeskPreferences.Current.highContrast;
 void Defaults(){AccessPreferences.Reset();P.interfaceScale=1.1f;P.textScale=1.25f;P.Save();}
 IEnumerator Start(){Application.logMessageReceived+=Error;Directory.CreateDirectory("Logs");yield return null;yield return null;a=AccessController.Instance;menus=a.GetComponent<DeskMenus>();a.ShowSettings();yield return new WaitForSecondsRealtime(3);yield return Tick();
 // A: each profile is reversible against a non-default base, including shared fields.
 for(int i=0;i<4;i++){Defaults();string before=Values();a.ApplyProfile(i);yield return Tick();Check((P.profileMask&(1<<i))!=0,"profile on "+i);Check(Entries().Where(e=>e.card).ElementAt(i).value=="Desactivar perfil","active caption "+i);a.ApplyProfile(i);yield return Tick();Check(P.profileMask==0&&Values()==before,"profile restores custom baseline "+i);}
 // B: all ordered pairs and both deactivation orders.
 for(int i=0;i<4;i++)for(int j=0;j<4;j++)if(i!=j)for(int reverse=0;reverse<2;reverse++){Defaults();string before=Values();a.ApplyProfile(i);a.ApplyProfile(j);int first=reverse==0?i:j,last=reverse==0?j:i;a.ApplyProfile(first);Check(P.profileMask==(1<<last),"remaining layer "+i+j+reverse);if(last==0||last==1)Check(P.largeTargets,"shared large targets retained");a.ApplyProfile(last);Check(Values()==before,"pair restoration "+i+j+reverse);}
 Defaults();a.ApplyProfile(0);a.OpenCategory("Visual");yield return Tick();Click("Tamaño de texto");yield return Tick();Check(P.textScale==1.75f&&P.HasProfileOverrides,"manual text override");a.ApplyProfile(1);a.ApplyProfile(0);a.ApplyProfile(1);Check(P.textScale==1.75f,"manual override survives all deactivations");
 Defaults();a.ApplyProfile(0);DeskPreferences.Current.highContrast=false;DeskPreferences.Current.Save();a.ApplyProfile(0);Check(!DeskPreferences.Current.highContrast,"external normal settings edit preserved");
 File.WriteAllText(AccessPreferences.FilePath,"{\"textScale\":1.5,\"profileMask\":15,\"simpleDocuments\":true,\"profileSnapshot\":\"legacy\"}");AccessPreferences.Load();Check(P.textScale==1.5f&&P.simpleDocuments&&P.profileMask==0&&P.profilesMigrated,"legacy effective settings preserved without guessing baseline");
 // F/H: every real model action, both pointer callback and keyboard dispatcher, at maximum scale.
 foreach(string category in new[]{"Inicio","Visual","Motor","Cognitiva","Audio","Controles","Glosario","Restaurar"}){
  Defaults();a.OpenCategory(category);yield return Tick();var titles=Entries().Where(e=>e.action!=null&&!e.text.StartsWith("Volver a ")&&!(category=="Inicio"&&!e.card&&e.text!="Restaurar accesibilidad")).Select(e=>e.text).ToArray();
  foreach(string title in titles){
   for(int mode=0;mode<2;mode++){
    Defaults();P.narration=true;P.visualSounds=true;P.simpleDocuments=true;P.textScale=1.75f;P.interfaceScale=1.2f;DeskPreferences.Current.highContrast=true;P.Save();a.DemoDocument();a.Speech.Say("Prueba de auditoría",true);a.OpenCategory(category);yield return Tick();
    var e=Entries().First(x=>x.text==title);var row=UI.GetComponentsInChildren<RectTransform>().First(x=>x.name=="Setting: "+title);Check(row.GetComponentsInChildren<Text>().All(t=>t.preferredHeight<=t.rectTransform.rect.height+3),"text fits "+title);
    if(!e.enabled){Check(row.GetComponentInChildren<Selectable>()!=null&&!row.GetComponentInChildren<Selectable>().interactable,"unavailable control disabled "+title);if(mode==0)audit.Add(category+"\t"+title+"\tno funcional\tDesactivado; "+e.description+"\tInspección de UI y estado interactuable en Play Mode");continue;}
    string before=JsonUtility.ToJson(P);var beforeFields=Fields();float value=e.sliderValue;
    if(mode==0){if(e.sliderChange!=null){var slider=row.GetComponentInChildren<Slider>();slider.value=value>.5f?.3f:.8f;}else Click(title);}
    else {var controls=Controls();int index=controls.FindIndex(c=>c.text.StartsWith(title+"."));Check(index>=0,"keyboard target "+title);typeof(AccessController).GetField("focused",F).SetValue(a,index);if(e.sliderChange!=null)a.AdjustFocused(value>.5f?-1:1);else a.Activate();}
    yield return Tick();Validate(title,category,value,mode);Unrelated(title,beforeFields);
    if(a.CustomPage==category){Check(UI.GetComponentsInChildren<Text>().All(t=>t.preferredHeight<=t.rectTransform.rect.height+3),"post-action layout "+title);}
    if(mode==0)audit.Add(category+"\t"+title+"\tfunciona\tAcción y estado comprobados; alto contraste y texto máximo\tEvento de puntero y activación por controlador de teclado; persistencia cuando aplica");
    // Verify round-trip of the effective serialized preferences after every control.
    P.Save();string saved=JsonUtility.ToJson(P);AccessPreferences.Load();Check(JsonUtility.ToJson(P)==saved,"preference round-trip "+title);
   }
  }
  Defaults();P.textScale=1.75f;P.interfaceScale=1.2f;DeskPreferences.Current.highContrast=true;a.OpenCategory(category);yield return Tick();FocusVisible();
 }
 File.WriteAllLines("Logs/Accessibility-v095-audit.tsv",audit);
 // Actual reader targets, visual notifications and speech command stability.
 Defaults();P.textScale=1;P.interfaceScale=1;P.largeTargets=false;menus.ShowMain();a.SetReadingView(true);yield return Tick();
 float small=Controls().First().target.GetComponent<RectTransform>().rect.height;
 P.largeTargets=true;yield return Tick();Check(Controls().First().target.GetComponent<RectTransform>().rect.height>small,"large targets actually enlarge reader controls");
 a.SetReadingView(false);P.visualSounds=true;yield return Tick();a.Notify("Aviso visible fuera del lector");yield return null;
 var toast=a.transform.Find("AccessibilityNotice").GetComponent<Canvas>();Check(toast.enabled&&toast.GetComponentInChildren<Text>().text.Contains("fuera del lector"),"visual notice visible in normal mode");P.visualSounds=false;yield return null;Check(!toast.enabled,"visual notice obeys disabled preference");
 P.narration=true;a.OpenCategory("Audio");yield return Tick();yield return new WaitForSecondsRealtime(.6f);int accepted=a.Speech.Accepted;string speechLast=a.Speech.Last;
 Click("Detener narración");yield return Tick();yield return new WaitForSecondsRealtime(.6f);Check(a.Speech.Accepted==accepted+1&&a.Speech.Last==speechLast,"stop sends only stop; refresh does not restart narration");
 accepted=a.Speech.Accepted;Click("Repetir última narración");yield return Tick();yield return new WaitForSecondsRealtime(.6f);Check(a.Speech.Accepted==accepted+1&&a.Speech.Last==speechLast,"repeat sends previous message once");
 var settings=Instantiate(UnityEngine.InputSystem.InputSystem.settings);settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;UnityEngine.InputSystem.InputSystem.settings=settings;keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
 yield return Key(UnityEngine.InputSystem.Key.F1);Check(a.CustomPage=="Controles","F1 simulated input opens reference");
 yield return Key(UnityEngine.InputSystem.Key.F2);Check(a.IsReadingView,"F2 simulated input opens reader");yield return Key(UnityEngine.InputSystem.Key.F2);Check(!a.IsReadingView,"F2 simulated input closes reader");
 accepted=a.Speech.Accepted;yield return Key(UnityEngine.InputSystem.Key.F3);Check(a.Speech.Accepted>accepted,"F3 simulated input narrates current screen");
 foreach(int repeat in new[]{6,7,8}){P.repeatKey=repeat;P.stopKey=repeat==8?6:repeat+1;P.Save();yield return Tick();yield return new WaitForSecondsRealtime(.4f);accepted=a.Speech.Accepted;yield return Key(repeat==6?UnityEngine.InputSystem.Key.F6:repeat==7?UnityEngine.InputSystem.Key.F7:UnityEngine.InputSystem.Key.F8);Check(a.Speech.Accepted==accepted+1,"repeat shortcut F"+repeat);accepted=a.Speech.Accepted;yield return Key(P.stopKey==6?UnityEngine.InputSystem.Key.F6:P.stopKey==7?UnityEngine.InputSystem.Key.F7:UnityEngine.InputSystem.Key.F8);Check(a.Speech.Accepted==accepted+1,"stop shortcut F"+P.stopKey);}
 UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
 P.narration=false;menus.Resume();var game=a.GetComponent<DespachoGame>();game.StartFromMenu();typeof(DespachoGame).GetMethod("ShowOffice",F).Invoke(game,null);yield return Tick();
 var doc=game.GetComponentInChildren<DeskDocument>();doc.AccessSelect();doc.AccessPlace();yield return new WaitForSecondsRealtime(.4f);yield return Tick();
 var stamps=a.Snapshot().Where(e=>e.text.StartsWith("[S] Seleccionar sello:")).ToList();Check(stamps.Count==2&&stamps.All(e=>e.text.Substring(e.text.IndexOf(':')+1).Trim().Length>0),"illustrated stamps have accessible decision labels");
 menus.ShowMain();a.ShowSettings();yield return Tick();
 // D: deliberately leave active layers + override for a separate Unity process.
 Defaults();a.ApplyProfile(0);a.ApplyProfile(1);a.OpenCategory("Visual");yield return Tick();Click("Tamaño de texto");yield return Tick();P.Save();DeskPreferences.Current.Save();File.WriteAllText("Logs/Accessibility-v095-directory.txt",DeskPreferences.TestDirectory);
 Check(!a.IsReadingView,"profiles never open reader");log.Add("COMPLETE "+log.Count(x=>x.StartsWith("PASS "))+" PASS - automated Play Mode, no physical keyboard claim");File.WriteAllLines("Logs/Accessibility-v095.txt",log);EditorApplication.Exit(0);
 }
 Dictionary<string,string> Fields(){var d=new Dictionary<string,string>();foreach(var obj in new object[]{P,DeskPreferences.Current})foreach(var f in obj.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public))if(!f.Name.StartsWith("profile"))d[obj.GetType().Name+"."+f.Name]=Convert.ToString(f.GetValue(obj));return d;}
 void Unrelated(string title,Dictionary<string,string> before){
  var allowed=new Dictionary<string,string>{
   {"Lectura cómoda","textScale highContrast independentColor largeTargets"},{"Interacción sencilla","simpleDocuments largeTargets"},{"Comprensión guiada","simpleLanguage navigationHelp symbols glossary"},{"Escucha asistida","visualSounds narration"},
   {"Alto contraste","highContrast"},{"Tamaño de texto","textScale"},{"Escala de interfaz","interfaceScale"},{"Indicadores con etiquetas","independentColor"},{"Reducir destellos","reduceFlashes"},{"Preferencia de lectura","readingView"},
   {"Seleccionar documentos con clics","simpleDocuments"},{"Navegación por teclado","keyboard"},{"Botones y zonas más grandes","largeTargets"},{"Reducir animaciones","reduceMotion"},
   {"Instrucciones sencillas","simpleLanguage"},{"Ayudas de navegación","navigationHelp"},{"Símbolos explicativos","symbols"},{"Glosario disponible","glossary"},
   {"Volumen maestro","master"},{"Efectos de sonido","effects"},{"Interfaz","ui"},{"Narración del sistema","narration"},{"Volumen de narración","narrationVolume"},{"Velocidad de voz","speechRate"},{"Audio mono","mono"},{"Reducir sonidos agudos","softAudio"},{"Avisos visuales","visualSounds"}
  };
  if(title=="Confirmar restauración")return;
  string fields=allowed.ContainsKey(title)?allowed[title]:title.StartsWith("Repetir voz:")||title.StartsWith("Detener voz:")?"repeatKey stopKey":title.StartsWith("Activar navegación")?"keyboard":"";
  var after=Fields();Check(before.All(x=>after[x.Key]==x.Value||fields.Split(' ').Contains(x.Key.Split('.')[1])),"unrelated preferences unchanged "+title);
 }
 void Validate(string title,string cat,float old,int mode){
  var map=new Dictionary<string,Func<bool>>{
   {"Alto contraste",()=>!DeskPreferences.Current.highContrast},{"Tamaño de texto",()=>P.textScale==.75f},{"Escala de interfaz",()=>P.interfaceScale==.8f},{"Indicadores con etiquetas",()=>P.independentColor},{"Reducir destellos",()=>P.reduceFlashes},{"Preferencia de lectura",()=>P.readingView},{"Vista de lectura",()=>a.IsReadingView},
   {"Seleccionar documentos con clics",()=>!P.simpleDocuments},{"Navegación por teclado",()=>P.keyboard},{"Botones y zonas más grandes",()=>P.largeTargets},{"Reducir animaciones",()=>DeskPreferences.Current.reduceMotion},{"Controles y atajos",()=>a.CustomPage=="Controles"},
   {"Instrucciones sencillas",()=>P.simpleLanguage},{"Ayudas de navegación",()=>P.navigationHelp},{"Símbolos explicativos",()=>P.symbols},{"Glosario disponible",()=>P.glossary},{"Glosario político",()=>a.CustomPage=="Glosario"},{"Tutorial y ayudas contextuales",()=>a.CustomPage==""&&menus.IsVisible},
   {"Narración del sistema",()=>!P.narration},{"Velocidad de voz",()=>P.speechRate==3},{"Audio mono",()=>P.mono},{"Reducir sonidos agudos",()=>P.softAudio},{"Avisos visuales",()=>!P.visualSounds},
   {"Restaurar accesibilidad",()=>a.CustomPage=="Restaurar"},{"Confirmar restauración",()=>P.profileMask==0&&P.textScale==1&&!P.narration&&!DeskPreferences.Current.highContrast},
   {"Documento de ejemplo",()=>!a.DemoOpened},{"Zona de lectura",()=>a.DemoOpened},{"Ejemplo de aviso",()=> (bool)typeof(AccessController).GetField("demoNotice",F).GetValue(a)},
   {"Aplicar cambios",()=>File.Exists(AccessPreferences.FilePath)},
   {"Probar narración",()=>a.Speech.Last=="Presidente, hay un expediente pendiente."||a.Speech.Last.StartsWith("Accesibilidad.")},
   {"Repetir última narración",()=>a.Speech.Running},{"Detener narración",()=>a.Speech.Running}
  };
  if(title.StartsWith("Repetir voz:")){Check(P.repeatKey==7&&P.stopKey!=7,"repeat shortcut changed without collision");return;}
  if(title.StartsWith("Detener voz:")){Check(P.stopKey==8,"stop shortcut changed");return;}
  if(title.StartsWith("Activar navegación")){Check(P.keyboard,"legacy keyboard toggle");return;}
  int profile=Array.IndexOf(new[]{"Lectura cómoda","Interacción sencilla","Comprensión guiada","Escucha asistida"},title);if(profile>=0){Check(P.profileMask==(1<<profile),"profile callback "+title);return;}
  if(new[]{"Volumen maestro","Efectos de sonido","Interfaz","Volumen de narración"}.Contains(title)){float actual=title=="Volumen maestro"?DeskPreferences.Current.master:title=="Efectos de sonido"?DeskPreferences.Current.effects:title=="Interfaz"?DeskPreferences.Current.ui:P.narrationVolume;Check(Mathf.Abs(actual-(mode==0?(old>.5f?.3f:.8f):Mathf.Clamp01(old+(old>.5f?-.05f:.05f))))<.001f,"volume value "+title);return;}
  Check(map.ContainsKey(title)&&map[title](),"expected effect "+title);
 }
}
public static class AccessibilityAuditRunner {
 public static void Run(){SessionState.SetBool("Audit095",true);SessionState.SetString("Audit095Dir",Path.Combine(Path.GetTempPath(),"Despacho095-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 public static void Restart(){DeskPreferences.TestDirectory=File.ReadAllText("Logs/Accessibility-v095-directory.txt");DeskPreferences.Load();AccessPreferences.Load();var p=AccessPreferences.Current;bool ok=p.profileMask==3&&p.textScale==1.75f&&p.simpleDocuments;p.ToggleProfile(0,true);p.ToggleProfile(1,true);ok&=p.profileMask==0&&p.textScale==1.75f&&!p.simpleDocuments&&!p.largeTargets&&!DeskPreferences.Current.highContrast;File.WriteAllText("Logs/Accessibility-v095-restart.txt",ok?"PASS separate Unity process preserved layers, override and baseline restoration":"FAIL profile restart");EditorApplication.Exit(ok?0:1);}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!SessionState.GetBool("Audit095",false))return;DeskSaveStore.TestDirectory=SessionState.GetString("Audit095Dir","");DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!SessionState.GetBool("Audit095",false))return;SessionState.SetBool("Audit095",false);new GameObject("AccessibilityAudit").AddComponent<AccessibilityAuditProbe>();}
}
#endif
