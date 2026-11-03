#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ElDespacho;

// Editor-only tests use a dedicated directory, never the player's save slots or preferences.
public sealed class MenuValidationProbe : MonoBehaviour
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    DespachoGame game; DeskMenus menus;
    readonly List<string> lines=new List<string>();
    string output;
    int errors,warnings;
    object Get(string name)=>typeof(DespachoGame).GetField(name,F).GetValue(game);
    void Set(string name,object value)=>typeof(DespachoGame).GetField(name,F).SetValue(game,value);
    void Call(string name,params object[] args)=>typeof(DespachoGame).GetMethod(name,F).Invoke(game,args);
    void Check(bool ok,string label) { lines.Add((ok?"PASS ":"FAIL ")+label); Flush(); if(!ok)throw new Exception(label); }
    void Flush()=>File.WriteAllLines(Path.Combine(output,"results.txt"),lines);
    void Log(string message,string stack,LogType type)
    {
        if(type==LogType.Warning) {warnings++;lines.Add("WARNING "+message);Flush();}
        if(type==LogType.Error || type==LogType.Exception) {errors++;lines.Add("ERROR "+message+"\n"+stack);Flush(); EditorApplication.Exit(1);}
    }
    Button FindButton(string label)=>menus.GetComponentsInChildren<Button>().First(b=>b.name==label);
    void Click(string label) {var b=FindButton(label);Check(b.interactable,"button enabled: "+label);b.onClick.Invoke();}
    PointerEventData Pointer(Vector2 point)=>new PointerEventData(EventSystem.current){position=point,pointerId=-1,button=PointerEventData.InputButton.Left};
    Vector2 Point(Transform t)=>RectTransformUtility.WorldToScreenPoint(null,t.position);
    IEnumerator OpenDocument(bool pause)
    {
        var doc=(DeskDocument)Get("deskDocument");var e=Pointer(Point(doc.transform));
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.pointerDownHandler);
        e.position=Point(doc.transform.parent.Find("ReadingZone"));
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.pointerUpHandler);
        if(pause) {menus.ShowPause();yield return new WaitForSecondsRealtime(.35f);Check(!doc.CanDecide,"opening freezes while paused");menus.Resume();}
        yield return new WaitForSecondsRealtime(.4f);Check(doc.CanDecide,"document opens after drop/resume");
    }
    IEnumerator Start()
    {
        output=MenuValidationRunner.Output;Directory.CreateDirectory(output);
        Application.logMessageReceived+=Log;
        yield return null; yield return null;
        game=FindAnyObjectByType<DespachoGame>();menus=game.GetComponent<DeskMenus>();
        Check(game!=null&&menus.PageName=="Principal","main menu boots in actual Unity Play Mode");
        Check(Resources.Load<Texture2D>("Menus/PresidentialOffice")!=null && Resources.Load<Texture2D>("Menus/GreenLamp")!=null,"both supplied backgrounds imported");
        MenuSkinValidation.Check(menus);
        Check(!FindButton("CONTINUAR").interactable,"Continue disabled without saves");
        Click("CARGAR PARTIDA"); Check(menus.GetComponentsInChildren<Button>().Count(b=>b.name=="CARGAR"&&!b.interactable)==4,"four empty load slots disabled");
        Click("VOLVER");Click("AJUSTES");
        Check(menus.PageName=="Ajustes","settings navigation");
        for(int i=0;i<5;i++){menus.ShowSettings(i);yield return null;Check(menus.GetComponentsInChildren<Button>().Any(b=>b.name=="VOLVER"),"settings category "+i);}
        var prefs=DeskPreferences.Current;prefs.master=.37f;prefs.ui=.28f;prefs.reduceMotion=true;prefs.autosave=false;prefs.Save();
        DeskPreferences.Load();Check(Mathf.Approximately(DeskPreferences.Current.master,.37f)&&DeskPreferences.Current.reduceMotion&&!DeskPreferences.Current.autosave,"preferences persisted through disk reload");
        DeskPreferences.Current.reduceMotion=false;DeskPreferences.Current.tutorial=false;
        menus.ShowMain();Click("CÓMO JUGAR");Check(menus.PageName=="Cómo jugar","help opens");Click("VOLVER");Click("CRÉDITOS");Check(menus.PageName=="Créditos","credits open");Click("VOLVER");
        Click("NUEVA PARTIDA");yield return null;Check(game.HasSession&&!menus.IsVisible,"new game starts existing introduction");
        game.GetComponentsInChildren<Button>().First(b=>b.name=="AssumeButton").onClick.Invoke();yield return null;
        Check(game.ScreenName=="Despacho","assume office keeps established flow");
        var doc=(DeskDocument)Get("deskDocument");var pointer=Pointer(Point(doc.transform));
        ExecuteEvents.Execute(doc.gameObject,pointer,ExecuteEvents.pointerDownHandler);
        menus.ShowPause();ExecuteEvents.Execute(doc.gameObject,pointer,ExecuteEvents.pointerUpHandler);
        menus.Resume();yield return new WaitForSecondsRealtime(.3f);
        Check(!doc.CanDecide,"pausing during drag returns paper without deciding");
#if ENABLE_INPUT_SYSTEM
        // A batch editor has no focused Game view. Use transient test settings only.
        var inputSettings=Instantiate(UnityEngine.InputSystem.InputSystem.settings);
        inputSettings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        inputSettings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.InputSystem.InputSystem.settings=inputSettings;
        var keyboard=UnityEngine.InputSystem.Keyboard.current ?? UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Escape));
        yield return null; yield return null;
        Check(menus.IsVisible,"Escape through Input System opens pause");
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
        yield return null;menus.Resume();
#endif
        yield return OpenDocument(true);
        menus.ShowPause();Check(Time.timeScale==0&&DeskPreferences.Paused,"pause freezes time and presentation");
        Click("GUARDAR PARTIDA");menus.SaveSlot(1);Check(DeskSaveStore.TryRead(1,out var saved,out _)&&saved.documentOpened,"manual slot stores open document");
        menus.ShowPause();Click("CONTINUAR");Check(!DeskPreferences.Paused&&Time.timeScale==1,"resume restores previous time scale");
        // Chained card, result and seal survive reload without applying effects twice.
        var cards=(List<Card>)Get("cards");var chain=cards.First(c=>c.id=="huelga");
        Set("currentCard",chain);Set("cardAnswered",false);Set("documentOpened",true);Set("cardsThisYear",1);
        ((HashSet<string>)Get("usedCards")).Add(chain.id);Call("ShowOffice");yield return null;
        Call("StampCard",false,Vector2.zero,"DENEGADO");yield return null;
        Check((bool)Get("cardAnswered")&&((List<string>)Get("cardQueue")).Contains("huelga_general"),"authored chained card queues normally");
        var state=game.CaptureSave();menus.SaveSlot(2);Set("estabilidad",99);
        menus.ShowPause();Click("VOLVER AL MENÚ PRINCIPAL");Click("CANCELAR");Check(menus.PageName=="Pausa","return confirmation cancels safely");
        Click("VOLVER AL MENÚ PRINCIPAL");Click("CONFIRMAR");Check(menus.PageName=="Principal"&&!game.HasSession,"return to main menu");
        Check(FindButton("CONTINUAR").interactable,"Continue enabled with valid save");
        Click("CONTINUAR");yield return null;
        Check((int)Get("estabilidad")==state.estabilidad&&(bool)Get("cardAnswered")&&(bool)Get("stampPlaced"),"Continue restores result and stamp without duplicate effects");
        yield return new WaitForSecondsRealtime(.35f);Call("ArchiveCard");yield return new WaitForSecondsRealtime(.3f);
        Check(((Card)Get("currentCard")).id=="huelga_general"&&!(bool)Get("documentOpened"),"next chained document closed after load");
        // Save in a visit after asking a question: do not invoke StartVisit on restore.
        var country=((List<Country>)Get("countries")).First();Call("StartVisit",country);Call("Ask",country.questions[0]);
        yield return null;
        state=game.CaptureSave();menus.SaveSlot(3);Call("ShowOffice");menus.LoadSlot(3);yield return null;
        country=(Country)Get("current");
        Check(game.ScreenName=="Visita"&&country.questions[0].asked&&country.questionsAsked==1,"visit reload preserves questions and dialogue");
        // Pending effects, drains, flags and diplomatic state serialize by stable ID.
        Call("Decide",Decision.Corte);yield return null;state=game.CaptureSave();
        menus.SaveSlot(3);menus.LoadSlot(3);yield return null;
        var after=game.CaptureSave();
        Check(after.estabilidad==state.estabilidad&&after.seguridad==state.seguridad&&after.prosperidad==state.prosperidad,"diplomatic consequences never reapply on load");
        Check(JsonUtility.ToJson(new Lists{pending=state.pending,drains=state.drains,countries=state.countries})==JsonUtility.ToJson(new Lists{pending=after.pending,drains=after.drains,countries=after.countries}),"pending events drains and all country states round-trip");
        // Corruption cannot overwrite the active session.
        File.WriteAllText(Path.Combine(DeskSaveStore.DirectoryPath,"slot-3.json"),"corrupt");
        Check(!DeskSaveStore.TryRead(3,out _,out _),"corrupt slot rejected");
        Check(game.CaptureSave().estabilidad==after.estabilidad,"corrupt read leaves active game untouched");
        menus.SaveSlot(1);menus.SaveSlot(1);Check(File.Exists(Path.Combine(DeskSaveStore.DirectoryPath,"slot-1.json.bak")),"atomic overwrite retains previous backup");
        Check(menus.SaveSlot(0)&&DeskSaveStore.TryRead(0,out _,out _),"dedicated autosave slot round-trip");
        // Auto timer runs only while playing; force elapsed time, not a long wait.
        menus.Resume();DeskPreferences.Current.autosave=true;
        typeof(DeskMenus).GetField("autosaveElapsed",F).SetValue(menus,1000f);yield return null;yield return null;
        Check((float)typeof(DeskMenus).GetField("autosaveElapsed",F).GetValue(menus)<1,"autoguardado timer fires during play");
        menus.ShowPause();typeof(DeskMenus).GetField("autosaveElapsed",F).SetValue(menus,10f);yield return new WaitForSecondsRealtime(.2f);
        Check((float)typeof(DeskMenus).GetField("autosaveElapsed",F).GetValue(menus)==10,"autoguardado timer stops in pause");
        menus.Resume();
        var randomState=game.CaptureSave();Call("DrawCard");var expected=((Card)Get("currentCard"))?.id;
        game.RestoreSave(randomState);Call("DrawCard");
        Check(((Card)Get("currentCard"))?.id==expected,"random draw sequence restored exactly");
        Set("year",4);Set("mandateFinished",true);var finalState=game.CaptureSave();game.RestoreSave(finalState);
        Check(game.ScreenName=="Fin"&&(int)Get("year")==4,"finished mandate loads its ending without replaying year effects");
        Set("estabilidad",0);var defeated=game.CaptureSave();game.RestoreSave(defeated);
        Check(game.ScreenName=="Fin"&&(int)Get("estabilidad")==0,"defeat remains defeat after reload");
        // Run the existing document regression suite on this same final integration.
        File.WriteAllText("Logs/DeskDocument-validation.txt","");
        var regression=new GameObject("Existing document regression").AddComponent<DeskValidationProbe>();
        float timeout=Time.realtimeSinceStartup+25;
        while(regression!=null && Time.realtimeSinceStartup<timeout)yield return null;
        string documentResults=File.ReadAllText("Logs/DeskDocument-validation.txt");
        Check(documentResults.Contains("COMPLETE:")&&!documentResults.Contains("FAIL ")&&!documentResults.Contains("ERROR "),"existing document regression suite completes");
        Check(errors==0,"zero runtime errors");Check(warnings==0,"zero runtime warnings");
        lines.Add("COMPLETE "+lines.Count(l=>l.StartsWith("PASS "))+" PASS");Flush();
        Application.logMessageReceived-=Log;
        EditorApplication.Exit(0);
    }
    [Serializable] class Lists {public DespachoGame.PendingState[] pending;public DespachoGame.DrainState[] drains;public DespachoGame.CountryState[] countries;}
}
public static class MenuValidationRunner
{
    public static string Output=>Path.GetFullPath("Logs/MenuValidation-v07");
    public static void Run()
    {
        SessionState.SetBool("DeskMenusRunTests",true);
        SessionState.SetString("DeskMenusTestDir",Path.Combine(Path.GetTempPath(),"ElDespacho-v07-tests-"+Guid.NewGuid().ToString("N")));
        EditorApplication.EnterPlaymode();
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Before()
    {
        if(!SessionState.GetBool("DeskMenusRunTests",false))return;
        DeskSaveStore.TestDirectory=SessionState.GetString("DeskMenusTestDir","");
        DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void After()
    {
        if(!SessionState.GetBool("DeskMenusRunTests",false))return;
        SessionState.SetBool("DeskMenusRunTests",false);
        new GameObject("MenuValidationProbe").AddComponent<MenuValidationProbe>();
    }
}
#endif
