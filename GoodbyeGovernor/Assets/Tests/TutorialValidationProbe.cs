#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using ElDespacho;

public class TutorialValidationProbe : MonoBehaviour
{
    const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    DespachoGame game; DeskMenus menus; TutorialManager guide;
    List<string> lines=new List<string>(); int warnings,errors;
    string PathOut=>"Logs/Tutorial-v08-results.txt";
    object Get(string n)=>typeof(DespachoGame).GetField(n,F).GetValue(game);
    void Set(string n,object v)=>typeof(DespachoGame).GetField(n,F).SetValue(game,v);
    void Call(string n,params object[] a)=>typeof(DespachoGame).GetMethod(n,F).Invoke(game,a);
    void Check(bool ok,string name){lines.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines(PathOut,lines);if(!ok)throw new Exception(name);}
    void Log(string m,string st,LogType t){if(t==LogType.Warning)warnings++;if(t==LogType.Error||t==LogType.Exception){errors++;lines.Add(m+st);File.WriteAllLines(PathOut,lines);EditorApplication.Exit(1);}}
    string State()=>JsonUtility.ToJson(game.CaptureSave());
    string Gameplay(){var s=game.CaptureSave();s.utcTicks=1;s.tutorial=null;return JsonUtility.ToJson(s);}
    IEnumerator Tick(){yield return null;yield return null;}
    Vector2 Point(Transform t)=>RectTransformUtility.WorldToScreenPoint(null,t.position);
    IEnumerator Open(bool valid=true){
        var doc=(DeskDocument)Get("deskDocument");var e=new PointerEventData(EventSystem.current){position=Point(doc.transform),pointerId=-1,button=PointerEventData.InputButton.Left};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
        Check(hits.Count>0 && hits[0].gameObject.GetComponentInParent<DeskDocument>()==doc,"guide leaves document reachable through UI raycast");
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.pointerDownHandler);yield return Tick();
        Check(game.Tutorial.facts.Contains("grabbed"),"pointer down records grab with guide enabled");
        e.position=valid?Point(doc.transform.parent.Find("ReadingZone")):new Vector2(-100,-100);
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.pointerUpHandler);yield return new WaitForSecondsRealtime(.4f);
        Check(doc.CanDecide==valid,valid?"valid event drag opens document":"invalid event drag returns to pile");
    }
    IEnumerator Start(){
        Directory.CreateDirectory("Logs");Application.logMessageReceived+=Log;yield return Tick();yield return Tick();
        game=FindAnyObjectByType<DespachoGame>();menus=game.GetComponent<DeskMenus>();guide=TutorialManager.Instance;
        Check(guide!=null&&guide.Steps.Length==18,"12 lessons / 18 notes available");
        Check(!guide.IsActive,"campaign notes hidden in main menu");
        menus.Resume();game.StartFromMenu();yield return Tick();
        Check(guide.ActiveId=="welcome"&&guide.IsActive,"new game shows Julian welcome");
        Check(guide.Status==TutorialStatus.EnCurso,"pending becomes in progress");
        DeskPreferences.Current.tutorial=false;yield return Tick();Call("ShowOffice");yield return Tick();
        Check(!guide.IsActive&&game.Tutorial.completed.Contains("welcome"),"disabled guide reconciles assume-office action");
        DeskPreferences.Current.tutorial=true;yield return Tick();Check(guide.ActiveId=="indicators","reactivation follows actual screen");
        int est=(int)Get("estabilidad");guide.Next();yield return Tick();Check((int)Get("estabilidad")==est&&guide.ActiveId=="grab","indicator explanation leaves values intact");
        yield return Open(false);Check(guide.ActiveId=="open","failed drag retains recoverable open step");
        menus.ShowPause();yield return Tick();Check(guide.ActiveId=="save"&&guide.IsActive,"pause contextual lesson above menu");
        guide.Next();yield return Tick();Check(guide.ActiveId=="load","pause second note");guide.Next();menus.Resume();yield return Tick();
        yield return Open();Check(guide.ActiveId=="decision","opening advances to actual stamp decision");
        for(int i=0;i<4;i++){
            menus.SaveSlot(i);Check(DeskSaveStore.TryRead(i,out var s,out _)&&s.tutorial.completed.Contains("open"),"tutorial stored in slot "+i);
            game.Tutorial.skipped.Add("decision");menus.LoadSlot(i);yield return Tick();
            Check(guide.ActiveId=="decision"&&!game.Tutorial.skipped.Contains("decision"),"exact pending decision restored from slot "+i);
        }
        string before=Gameplay(),progress=JsonUtility.ToJson(game.Tutorial);
        for(int i=0;i<12;i++){menus.ShowLesson(i);yield return Tick();}
        Check(Gameplay()==before&&JsonUtility.ToJson(game.Tutorial)==progress,"consult all lessons without changing campaign or tutorial");
        menus.Resume();yield return Tick();guide.SkipLesson();yield return Tick();
        Check(game.Tutorial.skipped.Contains("decision")&&!game.Tutorial.completed.Contains("decision"),"skipped lesson distinct from completed");
        Call("StampCard",true,Vector2.zero,"APROBADO");yield return Tick();
        Check((bool)Get("cardAnswered")&&guide.ActiveId=="result","real card decision resolves normally after skip");
        var applied=Gameplay();Call("StampCard",true,Vector2.zero,"APROBADO");Check(Gameplay()==applied,"duplicate stamp rejected");
        yield return new WaitForSecondsRealtime(.4f);Call("ArchiveCard");yield return new WaitForSecondsRealtime(.4f);
        Check(game.Tutorial.completed.Contains("result"),"continue action completes result lesson");
        Call("ShowMap");yield return Tick();Check(guide.ActiveId=="map","first map contextual lesson");guide.SkipLesson();
        var country=((List<Country>)Get("countries")).First();Call("StartVisit",country);yield return Tick();
        Check(guide.ActiveId=="questions"&&country.questionsAsked==0,"first visit explains questions without consuming them");
        Call("Ask",country.questions[0]);yield return Tick();Check(country.questionsAsked==1&&guide.ActiveId=="dossier","real question unlocks dossier guidance");
        guide.Next();yield return Tick();
        for(int tab=1;tab<4;tab++){Set("docTab",tab);Call("ShowVisit");yield return Tick();Check(guide.ActiveId==new[]{"","intel","trade","region"}[tab],"document tab lesson "+tab);guide.Next();yield return Tick();}
        Check(country.questionsAsked==1,"tab guidance consumes no questions");
        foreach(var q in country.questions)Call("Ask",q);Check(country.questionsAsked==3,"existing three-question limit preserved");
        var unchanged=Gameplay();Call("GuidedDiplomacy",Decision.Alianza);yield return Tick();Check(Gameplay()==unchanged&&guide.ActiveId=="relations","first diplomacy click explains before executing");
        guide.Next();Call("GuidedDiplomacy",Decision.Alianza);yield return Tick();Check(country.decision==Decision.Alianza,"player repeats click to decide exactly once");
        var cards=(List<Card>)Get("cards");var chain=cards.First(c=>c.id=="huelga");Set("currentCard",chain);Set("cardAnswered",false);Set("documentOpened",true);Call("ShowOffice");yield return Tick();
        Call("StampCard",false,Vector2.zero,"DENEGADO");yield return new WaitForSecondsRealtime(.4f);Call("ArchiveCard");yield return new WaitForSecondsRealtime(.4f);
        Check(((Card)Get("currentCard")).id=="huelga_general"&&guide.ActiveId=="chain","real authored chain activates lesson without future reveal");guide.Next();
        // Arrange end-year eligibility, then exercise the unchanged closing operation.
        foreach(var c in (List<Country>)Get("countries"))if(c.year==1)c.decision=Decision.Alianza;
        Set("currentCard",null);Set("cardsThisYear",5);((List<string>)Get("cardQueue")).Clear();Call("ShowOffice");yield return Tick();Check(guide.ActiveId=="year","year lesson uses actual eligibility");
        guide.Next();Call("EndYear");yield return Tick();Check((int)Get("year")==2,"existing year advancement executes");
        var old=game.CaptureSave();old.tutorial=null;game.RestoreSave(old);yield return Tick();Check(game.Tutorial!=null&&game.Tutorial.completed.Contains("welcome"),"v07 slot without tutorial loads and reconciles");
        DeskPreferences.Current.reduceMotion=true;menus.Resume();game.StartFromMenu();Call("ShowOffice");yield return Tick();guide.SkipLesson();yield return Tick();yield return Open();
        Check(guide.ActiveId=="decision","reduced motion preserves guide transitions");
        guide.CompleteTutorial();yield return Tick();Check(guide.Status!=TutorialStatus.Completado&&!guide.IsActive,"omit remaining guide does not mark every note completed");
        menus.Resume();game.StartFromMenu();yield return Tick();Check(guide.ActiveId=="welcome","new campaign resets prior omission");
        guide.CompleteTutorial();yield return Tick();Check(guide.Status==TutorialStatus.Omitido,"fully omitted has explicit omitted status");
        game.Tutorial.skipped.Clear();foreach(var s in guide.Steps)game.Tutorial.Complete(s.persistentId);Check(guide.Status==TutorialStatus.Completado,"completed state distinct from omitted");
        Set("year",4);Check(game.TutorialQuestions==2,"year four uses two questions in explanations");
        Check(errors==0&&warnings==0,"zero runtime errors and warnings");lines.Add("COMPLETE "+lines.Count(l=>l.StartsWith("PASS "))+" PASS - automated Unity Play Mode, no physical input");File.WriteAllLines(PathOut,lines);EditorApplication.Exit(0);
    }
}
public static class TutorialValidationRunner
{
    public static void Run(){SessionState.SetBool("TutorialTests08",true);SessionState.SetString("TutorialDir08",System.IO.Path.Combine(System.IO.Path.GetTempPath(),"Despacho08-"+Guid.NewGuid().ToString("N")));EditorApplication.EnterPlaymode();}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Before(){if(!SessionState.GetBool("TutorialTests08",false))return;DeskSaveStore.TestDirectory=SessionState.GetString("TutorialDir08","");DeskPreferences.TestDirectory=DeskSaveStore.TestDirectory;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void After(){if(!SessionState.GetBool("TutorialTests08",false))return;SessionState.SetBool("TutorialTests08",false);new GameObject("TutorialValidation").AddComponent<TutorialValidationProbe>();}
}
#endif
