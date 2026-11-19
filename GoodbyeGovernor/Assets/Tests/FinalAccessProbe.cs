#if UNITY_EDITOR || DESPACHO_AUDIT
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEngine.InputSystem;using UnityEngine.InputSystem.LowLevel;using ElDespacho;
#if UNITY_EDITOR
using UnityEditor;using UnityEditor.Build.Reporting;
#endif
public class FinalAccessProbe:MonoBehaviour {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;DespachoGame g;DeskMenus m;AccessController a;Keyboard keyboard;List<string> log=new List<string>();string dir;DespachoGame.SaveData baseline;AccessPreferences P=>AccessPreferences.Current;
 void Check(bool ok,string s){log.Add((ok?"PASS ":"FAIL ")+s);File.WriteAllLines(dir+"/results.txt",log);}
 void Call(string n,params object[] args)=>typeof(DespachoGame).GetMethod(n,F).Invoke(g,args);
 object Get(string n)=>typeof(DespachoGame).GetField(n,F).GetValue(g);void Set(string n,object value)=>typeof(DespachoGame).GetField(n,F).SetValue(g,value);
 string State(){var s=g.CaptureSave();s.utcTicks=1;return JsonUtility.ToJson(s);}
 IEnumerator Tick(){a.Refresh();yield return null;yield return null;yield return new WaitForSecondsRealtime(.3f);Canvas.ForceUpdateCanvases();}
 IEnumerator PressKey(Key k){InputSystem.QueueStateEvent(keyboard,new KeyboardState(k));yield return Tick();InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return Tick();}
 float outputPeak;
 IEnumerator Measure(AudioClip clip){var src=(AudioSource)typeof(Sfx).GetField("_src",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);src.Stop();yield return new WaitForSecondsRealtime(.2f);Sfx.Play(clip);outputPeak=0;for(int i=0;i<3;i++){yield return null;var data=new float[1024];src.GetOutputData(data,0);outputPeak=Mathf.Max(outputPeak,data.Max(v=>Mathf.Abs(v)));}}
 IEnumerator Shot(string name){yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(dir+"/"+name+".png",t.EncodeToPNG());Destroy(t);}
 IEnumerator Scene(string name){a.ClosePanels();m.Resume();DeskPreferences.Current.tutorial=false;Call("RestoreSave",JsonUtility.FromJson<DespachoGame.SaveData>(JsonUtility.ToJson(baseline)));Call("ShowOffice");
 if(name=="documento"||name=="resultado"){Set("documentOpened",true);Call("ShowOffice");if(name=="resultado")Call("AnswerCard",true);}
 if(name=="mapa")Call("ShowMap");
 if(new[]{"dialogo","dossier","inteligencia","comercio","region"}.Contains(name)){var c=((List<Country>)Get("countries")).First();Call("StartVisit",c);if(name=="dialogo")Call("Ask",c.questions.First());Set("docTab",name=="inteligencia"?1:name=="comercio"?2:name=="region"?3:0);Call("ShowVisit");}
 if(name=="mandato")Call("ShowEnding");if(name=="anual"){var method=typeof(DespachoGame).GetMethod("ShowYearEvents",F);method.Invoke(g,new object[]{"AÑO 2","Resumen de prueba",Activator.CreateInstance(method.GetParameters()[2].ParameterType),"CONTINUAR",new UnityEngine.Events.UnityAction(()=>Call("ShowOffice"))});}
 if(name=="tutorial"){m.ShowPause();m.ShowLesson(2);}if(name=="pausa")m.ShowPause();if(name=="guardados")m.ShowSlots(true);if(name=="fin")Call("ShowGameOver");yield return Tick();}
 string Visible()=>string.Join("\n",a.Snapshot().Where(e=>e.action==null).Select(e=>e.text));
 IEnumerator Start(){dir=Path.Combine(DeskPreferences.TestDirectory,"Evidence");Directory.CreateDirectory(dir);Application.logMessageReceived+=(s,st,t)=>{if(t==LogType.Error||t==LogType.Exception){File.AppendAllText(dir+"/errors.txt",s+"\n"+st);FinalAccessRunner.Exit(2);}};yield return null;yield return null;a=AccessController.Instance;g=a.GetComponent<DespachoGame>();m=g.GetComponent<DeskMenus>();keyboard=InputSystem.AddDevice<Keyboard>();
#if UNITY_EDITOR
 var input=Instantiate(InputSystem.settings);input.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;input.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings=input;
#endif
 QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
#if !UNITY_EDITOR
 Screen.SetResolution(1280,720,false);yield return new WaitForSecondsRealtime(.5f);Check(Screen.width==1280&&Screen.height==720,"actual player resolution 1280x720");
#endif

 if(File.Exists(Path.Combine(DeskPreferences.TestDirectory,"expected.json"))){Check(JsonUtility.ToJson(P)==File.ReadAllText(Path.Combine(DeskPreferences.TestDirectory,"expected.json")),"preferences survive fresh process");Check(!a.IsReadingView,"restart does not activate reader");Check(DeskSaveStore.TryRead(1,out var loaded,out _),"saved campaign exists after restart");m.LoadSlot(1);yield return Tick();Check(g.HasSession&&!a.IsReadingView,"existing campaign loads visually");FinalAccessRunner.Exit(log.Any(x=>x.StartsWith("FAIL"))?1:0);yield break;}
 Check(m.IsVisible&&!a.IsReadingView,"startup main menu and no forced reader");Check(!P.keyboard,"fresh arrows default OFF");yield return Shot("startup");DeskPreferences.Current.tutorial=false;DeskPreferences.Current.autosave=false;m.Resume();g.StartFromMenu();Call("ShowOffice");yield return Tick();baseline=g.CaptureSave();yield return Shot("office-before");
 a.Speech.Probe();for(int i=0;i<40&&!a.Speech.Ready;i++)yield return new WaitForSecondsRealtime(.2f);Check(a.Speech.Ready&&a.Speech.Error=="","Windows speech ready: "+a.Speech.Voice);File.WriteAllText(dir+"/voice.txt",a.Speech.Voice+"\nTechnical acceptance only; no human listening. Volume zero for automated narration.");P.narrationVolume=0;P.narration=true;yield return Tick();a.Speech.Say("Prueba de configuración",true);yield return Tick();P.speechRate=3;yield return Tick();Check(a.Speech.ConfiguredRate==3&&a.Speech.ConfiguredVolume==0,"speech live rate and mute configuration");int count=a.Speech.Accepted;a.Speech.Repeat();yield return Tick();Check(a.Speech.Accepted>count,"speech repeat accepted");count=a.Speech.Accepted;a.Speech.Stop();yield return Tick();Check(a.Speech.Accepted>count,"speech stop accepted");P.narrationVolume=.5f;DeskPreferences.Current.master=.4f;yield return Tick();Check(a.Speech.ConfiguredVolume==20,"speech volume combines voice and master");a.Speech.Stop();P.narration=false;yield return Tick();count=a.Speech.Accepted;a.Speech.Say("No debe leerse",true);yield return Tick();Check(a.Speech.Accepted==count,"narration OFF rejects new speech");P.narrationVolume=0;
 DeskPreferences.Current.effects=0;yield return Measure(Sfx.Low);Check(outputPeak<.0001f,"effects OFF mutes real source samples");DeskPreferences.Current.effects=1;yield return Measure(Sfx.Low);Check(outputPeak>.0001f,"effects ON produces real source samples");DeskPreferences.Current.ui=0;yield return Measure(Sfx.Click);Check(outputPeak<.0001f,"UI OFF mutes real source samples");DeskPreferences.Current.ui=1;yield return Measure(Sfx.Click);Check(outputPeak>.0001f,"UI ON produces real source samples");
 var mono=new[]{1f,-1f,.6f,.2f};AccessAudioFilter.Process(mono,2,true,false,new float[8]);Check(mono[0]==mono[1]&&mono[2]==mono[3],"mono real sample processing");var soft=new[]{1f,-1f,1f,-1f};AccessAudioFilter.Process(soft,1,false,true,new float[8]);Check(soft.Max(Mathf.Abs)<1,"high frequency attenuation sample processing");DeskPreferences.Current.master=.4f;DeskPreferences.Current.Apply(false);Check(Mathf.Approximately(AudioListener.volume,.4f),"master controls actual AudioListener");P.visualSounds=true;Sfx.Play(Sfx.Paper);yield return Tick();Check(g.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("Documento manipulado")),"paper sound has visible notification");P.visualSounds=false;
 string[] screens={"despacho","documento","resultado","mapa","dialogo","dossier","inteligencia","comercio","region","tutorial","pausa","guardados","anual","fin","mandato"};
 foreach(string screen in screens){P.simpleLanguage=false;P.symbols=false;P.glossary=false;P.navigationHelp=false;P.textScale=1;DeskPreferences.Current.highContrast=false;yield return Scene(screen);string state=State();string off=Visible();P.simpleLanguage=true;P.symbols=true;P.glossary=true;P.navigationHelp=true;P.SetKeyboard(true);P.largeTargets=true;P.narration=true;P.narrationVolume=0;P.textScale=1.75f;DeskPreferences.Current.highContrast=true;yield return Tick();
 Check(State()==state,"combined settings preserve campaign "+screen);Check(!a.IsReadingView,"combined settings remain illustrated "+screen);Check(Visible()!=off,"actual visible presentation changes "+screen);var adapters=g.GetComponentsInChildren<GameplayTextAccess>().Where(t=>a.IsSource(t.transform)).ToArray();Check(adapters.Length>0,"adapted text present "+screen);Check(adapters.All(t=>!t.Display.resizeTextForBestFit),"text not shrunk "+screen);Check(adapters.All(t=>t.Scroll==null||!t.Scroll.enabled||t.Scroll.content.rect.height+4>=t.Display.preferredHeight),"full text fits scroll content "+screen);
 string spoken=a.NarrationSnapshot();int accepted=a.Speech.Accepted;a.Speech.Say(spoken,true);yield return Tick();Check(a.Speech.Accepted>accepted&&a.Speech.Last==spoken,"speech receives current screen "+screen);File.WriteAllText(dir+"/speech-"+screen+".txt",spoken);yield return Shot(screen+"-175");
 if(screen!="pausa"&&screen!="guardados"){var b=g.GetComponentsInChildren<Button>().FirstOrDefault(b=>(b.name=="ContextGlossaryButton"||b.name=="GuidedGlossary")&&b.GetComponentInParent<Canvas>().enabled);Check(b!=null&&b.GetComponentInParent<Canvas>().enabled,"context glossary visible "+screen);b.onClick.Invoke();yield return Tick();Check(a.CustomPage=="Glosario","context glossary opens "+screen);Check(a.NarrationSnapshot().Contains("Sanciones"),"glossary narrated from current page "+screen);yield return PressKey(Key.Escape);Check(a.CustomPage==""&&State()==state,"glossary restores origin and state "+screen);}
 P.narration=false;P.simpleLanguage=false;P.symbols=false;P.glossary=false;P.navigationHelp=false;P.textScale=1;DeskPreferences.Current.highContrast=false;yield return Tick();Check(State()==state,"OFF restores without campaign change "+screen);Check(!Visible().Contains("[OK]"),"symbol OFF restores labels "+screen);
 }
 yield return Scene("documento");P.textScale=1.5f;yield return Tick();yield return Shot("document-150");yield return PressKey(Key.F2);Check(a.IsReadingView,"F2 opens reader explicitly");yield return PressKey(Key.F2);Check(!a.IsReadingView,"F2 returns to visual");
 P.mono=true;P.softAudio=true;P.simpleLanguage=true;P.symbols=true;P.glossary=true;P.largeTargets=true;P.reduceFlashes=true;P.interfaceScale=1.2f;P.narration=false;P.Save();DeskPreferences.Current.Save();m.SaveSlot(1);Check(DeskSaveStore.TryRead(1,out var save,out _),"campaign save works");string prefs=JsonUtility.ToJson(P);m.LoadSlot(1);yield return Tick();Check(JsonUtility.ToJson(P)==prefs&&!a.IsReadingView,"load preserves options and visual mode");File.WriteAllText(Path.Combine(DeskPreferences.TestDirectory,"expected.json"),JsonUtility.ToJson(P));a.Speech.Dispose();log.Add("COMPLETE technical virtual input/callback audit; no physical hearing or human understanding test");File.WriteAllLines(dir+"/results.txt",log);File.WriteAllLines(dir+"/full-results.txt",log);FinalAccessRunner.Exit(log.Any(x=>x.StartsWith("FAIL"))?1:0);
 }
}
public static class FinalAccessRunner {
 public static string DirectoryArg(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-accessAuditDir");return i>=0&&i+1<args.Length?args[i+1]:null;}
 public static void Exit(int code){
#if UNITY_EDITOR
 EditorApplication.Exit(code);
#else
 Application.Quit(code);
#endif
 }
#if UNITY_EDITOR
 public static void Run(){SessionState.SetBool("FinalAccess",true);SessionState.SetString("FinalAccessDir",DirectoryArg()??Path.Combine(Path.GetTempPath(),"DespachoFinal-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
 public static void Build(){string output=Environment.GetEnvironmentVariable("DESPACHO_BUILD_OUTPUT");bool audit=Environment.GetEnvironmentVariable("DESPACHO_BUILD_AUDIT")=="1";var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=output,target=BuildTarget.StandaloneWindows64,options=audit?BuildOptions.Development:BuildOptions.None,extraScriptingDefines=audit?new[]{"DESPACHO_AUDIT"}:new string[0]});File.WriteAllText(Path.ChangeExtension(output,"build.txt"),report.summary.result+" errors="+report.summary.totalErrors);Exit(report.summary.result==BuildResult.Succeeded?0:1);}
#endif
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){
#if UNITY_EDITOR
 if(!SessionState.GetBool("FinalAccess",false))return;DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory=SessionState.GetString("FinalAccessDir","");
#else
 string dir=DirectoryArg();if(dir==null)return;DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory=dir;
#endif
 }
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){
#if UNITY_EDITOR
 if(!SessionState.GetBool("FinalAccess",false))return;SessionState.SetBool("FinalAccess",false);
#else
 if(DirectoryArg()==null)return;
#endif
 new GameObject("FinalAccessProbe").AddComponent<FinalAccessProbe>();}
}
#endif
