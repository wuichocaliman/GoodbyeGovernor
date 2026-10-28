#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ElDespacho;

public class DeskValidationProbe : MonoBehaviour
{
    DespachoGame game;
    const BindingFlags F = BindingFlags.NonPublic | BindingFlags.Instance;
    readonly List<string> results = new List<string>();
    object Get(string n) => typeof(DespachoGame).GetField(n,F).GetValue(game);
    void Set(string n, object value) => typeof(DespachoGame).GetField(n,F).SetValue(game,value);
    void Call(string n, params object[] args) => typeof(DespachoGame).GetMethod(n,F).Invoke(game,args);
    DeskDocument Doc => (DeskDocument)Get("deskDocument");
    void Check(bool ok, string label)
    {
        results.Add((ok ? "PASS " : "FAIL ") + label);
        File.WriteAllLines("Logs/DeskDocument-validation.txt", results);
        if (!ok) throw new Exception(label);
    }
    Vector2 ScreenPoint(Transform t) => RectTransformUtility.WorldToScreenPoint(null,t.position);
    PointerEventData Pointer(Vector2 p) => new PointerEventData(EventSystem.current) { position=p, pointerId=-1, button=PointerEventData.InputButton.Left };
    void Drag(DeskDocument doc, Vector2 target)
    {
        var e=Pointer(ScreenPoint(doc.transform));
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.beginDragHandler);
        e.position=target;
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.dragHandler);
        Check(Vector2.Distance(ScreenPoint(doc.transform),target)<1,"document follows cursor in canvas coordinates");
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.endDragHandler);
        ExecuteEvents.Execute(doc.gameObject,e,ExecuteEvents.pointerUpHandler);
    }
    IEnumerator Open()
    {
        var d=Doc;
        var tray=d.transform.parent.Find("ReadingZone");
        var down = Pointer(ScreenPoint(d.transform));
        ExecuteEvents.Execute(d.gameObject,down,ExecuteEvents.pointerDownHandler);
        down.position = ScreenPoint(tray);
        ExecuteEvents.Execute(d.gameObject,down,ExecuteEvents.dragHandler);
        Check(tray.GetComponentInChildren<Text>().text.Contains("SUELTA"), "reading zone highlights during drag");
        ExecuteEvents.Execute(d.gameObject,down,ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(d.gameObject,down,ExecuteEvents.endDragHandler);
        Check(!d.CanDecide,"opening animation blocks decisions");
        yield return new WaitForSecondsRealtime(0.3f);
        Check(d.CanDecide && (bool)Get("documentOpened"),"valid drop opens exactly once");
    }
    void HandleLog(string message, string stack, LogType type)
    {
        if (type != LogType.Exception && type != LogType.Error) return;
        results.Add("ERROR " + message + "\n" + stack);
        File.WriteAllLines("Logs/DeskDocument-validation.txt", results);
        UnityEditor.EditorApplication.isPaused = true;
        StopAllCoroutines();
    }
    void OnDestroy() => Application.logMessageReceived -= HandleLog;
    IEnumerator Start()
    {
        Application.logMessageReceived += HandleLog;
        yield return null;
        game=FindAnyObjectByType<DespachoGame>();
        Check(game!=null,"game boots in Unity Play mode");
        Call("NewGame"); Call("ShowOffice");
        yield return null;
        Check(!Doc.CanDecide && !Doc.transform.Find("OpenedContent").gameObject.activeSelf,"fresh card is closed and decisions hidden");
        int start=(int)Get("estabilidad");
        Call("AnswerCard",true);
        Check((int)Get("estabilidad")==start && !(bool)Get("cardAnswered"),"answer before opening has no effect");
        var initial=((RectTransform)Doc.transform).anchoredPosition;
        Drag(Doc,new Vector2(-100,-100));
        yield return new WaitForSecondsRealtime(0.3f);
        Check(!Doc.CanDecide && Vector2.Distance(initial,((RectTransform)Doc.transform).anchoredPosition)<1,"invalid drop returns to pile");
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(Pointer(ScreenPoint(Doc.transform)), hits);
        Check(hits.Any(x => x.gameObject == Doc.gameObject), "closed document hit by GraphicRaycaster");
        yield return Open();
        Call("ShowMap"); yield return null;
        Call("ShowOffice"); yield return null;
        Check(Doc.CanDecide,"map round-trip preserves opened document");
        // Use an existing chain and its unchanged authored effect.
        Call("NewGame");
        var cards=(List<Card>)Get("cards");
        var chain=cards.First(c=>c.id=="huelga");
        Set("currentCard",chain); Set("cardsThisYear",1);
        ((HashSet<string>)Get("usedCards")).Add(chain.id);
        Call("ShowOffice"); yield return null;
        yield return Open();
        int a=(int)Get("estabilidad"), b=(int)Get("seguridad"), c0=(int)Get("prosperidad");
        Call("AnswerCard",false); Call("AnswerCard",false);
        Check((int)Get("estabilidad")==Mathf.Clamp(a+chain.no.estabilidad,0,100) &&
            (int)Get("seguridad")==Mathf.Clamp(b+chain.no.seguridad,0,100) &&
            (int)Get("prosperidad")==Mathf.Clamp(c0+chain.no.prosperidad,0,100),"original three effects applied exactly once despite double answer");
        Call("ArchiveCard");
        Check(Doc.CanDecide && (bool)Get("cardAnswered"), "second click cannot skip freshly shown result");
        yield return new WaitForSecondsRealtime(0.35f);
        Call("ArchiveCard"); Call("ArchiveCard");
        Check(!Doc.CanDecide,"archive animation blocks repeated continue");
        yield return new WaitForSecondsRealtime(0.3f);
        Check(((Card)Get("currentCard")).id==chain.noNext && !Doc.CanDecide,"chain delivered as new closed document");
        for(int i=0;i<3;i++)
        {
            yield return Open();
            Call("AnswerCard",true);
            yield return new WaitForSecondsRealtime(0.35f);
            Call("ArchiveCard");
            yield return new WaitForSecondsRealtime(0.3f);
        }
        Check((int)Get("cardsThisYear")==5,"successive decisions advance once each");
        Call("NewGame");
        var special=((List<Card>)Get("cards")).First(x=>x.id=="filtracion");
        Set("currentCard",special); Call("ShowOffice"); yield return null;
        yield return Open();
        var labels=Doc.GetComponentsInChildren<Button>().Select(x=>x.GetComponentInChildren<Text>().text).ToList();
        Check(labels.Contains(special.yesLabel)&&labels.Contains(special.noLabel),"custom answer labels retained");
        Call("ShowMap"); yield return null;
        Check(FindObjectsByType<Button>(FindObjectsSortMode.None).Length>0,"map remains accessible");
        results.Add("COMPLETE: automated Play-mode event tests; no physical mouse or visual sign-off.");
        File.WriteAllLines("Logs/DeskDocument-validation.txt",results);
        Debug.Log("EL_DESPACHO_VALIDATION_COMPLETE " + results.Count + " checks/log entries");
        Call("ShowOffice");
        Destroy(gameObject);
    }
}

public static class DeskDocumentValidationMenu
{
    [UnityEditor.MenuItem("Tools/El Despacho/Validate document flow (resets Play session)")]
    public static void Run()
    {
        if (!UnityEditor.EditorApplication.isPlaying) { Debug.LogWarning("Enter Play mode before running document validation."); return; }
        UnityEditor.EditorApplication.isPaused = false;
        if (UnityEngine.Object.FindAnyObjectByType<DeskValidationProbe>() != null) return;
        new GameObject("Desk document validation").AddComponent<DeskValidationProbe>();
    }
}

#endif

