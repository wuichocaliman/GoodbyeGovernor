using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElDespacho
{
    // Extends the existing spotlight overlay; game rules stay in DespachoGame.
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }
        internal const string DefaultPrefsKey="tutorial_done_v1", ResourcesFolder="Tutorial";
        [SerializeField] TutorialStepData[] steps;
        DespachoGame game; DeskMenus menus;
        int current=-1; bool ready;
        RectTransform overlay, target, bubble; Transform gameCanvas;
        readonly Image[] blockers=new Image[4], frame=new Image[4];
        Text header, body, hint; Button next; CanvasGroup noteAlpha; float noteShownAt;
        const float FrameThickness=3;
        public bool IsActive=>current>=0 && overlay!=null && overlay.GetComponent<Canvas>().enabled;
        public string ActiveId=>current>=0?steps[current].persistentId:"";
        public TutorialStepData[] Steps=>steps;
        TutorialProgress Progress=>game.Tutorial;
        public TutorialStatus Status=>Progress.Status(steps);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() {
            if(!DespachoGame.AutoBoot || FindAnyObjectByType<TutorialManager>()!=null)return;
            var owner=FindAnyObjectByType<DespachoGame>();
            (owner!=null?owner.gameObject:new GameObject("TutorialManager")).AddComponent<TutorialManager>();
        }
        void Awake() {if(Instance!=null && Instance!=this){Destroy(this);return;}Instance=this;}
        void OnDestroy() {if(Instance==this)Instance=null;DeskDocument.Grabbed-=OnDocumentGrabbed;if(overlay!=null)Destroy(overlay.gameObject);if(steps!=null)foreach(var s in steps)Destroy(s);}
        IEnumerator Start() {
            while(gameCanvas==null) {
                game=FindAnyObjectByType<DespachoGame>();
                var c=game!=null?game.GetComponentInChildren<Canvas>():null;
                if(c!=null)gameCanvas=c.transform;else yield return null;
            }
            menus=game.GetComponent<DeskMenus>();
            var legacy=steps!=null && steps.Length>0?steps:Resources.LoadAll<TutorialStepData>(ResourcesFolder).OrderBy(s=>s.name).ToArray();
            if(legacy.Length==0)legacy=TutorialStepData.CreateDefaults();
            steps=TutorialLessons.Create(legacy);
            BuildOverlay();DeskDocument.Grabbed+=OnDocumentGrabbed;ready=true;
        }
        public void SetMenuSuspended(bool suspended) { /* Visibility is derived from context in LateUpdate, including the pause lesson. */ }
        void OnDocumentGrabbed()=>TryAdvance(TutorialAdvanceCondition.GrabDocument);
        public void TryAdvance(TutorialAdvanceCondition trigger) {
            if(game==null || !game.HasSession)return;
            string fact=trigger==TutorialAdvanceCondition.PressAssumeOffice?"assumed":trigger==TutorialAdvanceCondition.GrabDocument?"grabbed":trigger==TutorialAdvanceCondition.DragDocumentToTray?"opened":trigger==TutorialAdvanceCondition.PressYesOrNo?"answered":trigger==TutorialAdvanceCondition.PressContinue?"archived":"";
            if(fact!="")Progress.Observe(fact);
        }
        bool Eligible(TutorialStepData s) {
            string context=menus.IsVisible?menus.PageName:game.ScreenName;
            if(s.context!=context)return false;
            switch(s.persistentId) {
                case "grab":case "open":return game.TutorialHasPaper && !game.TutorialOpen;
                case "decision":return game.TutorialOpen && !game.TutorialAnswered;
                case "result":return game.TutorialAnswered;
                case "dossier":return game.TutorialTab==0;
                case "intel":return game.TutorialTab==1;
                case "trade":return game.TutorialTab==2;
                case "region":return game.TutorialTab==3;
                case "chain":return game.TutorialChain && game.TutorialHasPaper && !game.TutorialAnswered;
                case "year":return game.TutorialReadyYear;
                default:return true;
            }
        }
        void LateUpdate() {
            if(!ready)return;
            game.ReconcileTutorial();
            foreach(var s in steps)if(!string.IsNullOrEmpty(s.actionFact) && Progress.facts.Contains(s.actionFact))Progress.Complete(s.persistentId);
            if(Progress.facts.Contains("diplomacy"))Progress.Complete("relations");
            if(Progress.facts.Contains("year"))Progress.Complete("year");
            var canvas=overlay.GetComponent<Canvas>();
            canvas.enabled=game.HasSession && DeskPreferences.Current.tutorial && (!menus.IsVisible || menus.PageName=="Pausa");
            if(!canvas.enabled){current=-1;return;}
            int index=System.Array.FindIndex(steps,s=>s.persistentId=="relations" && Progress.facts.Contains("decisionHelp") && !Progress.Resolved(s.persistentId) && Eligible(s));
            if(index<0)index=System.Array.FindIndex(steps,s=>!Progress.Resolved(s.persistentId) && Eligible(s));
            if(index<0){current=-1;canvas.enabled=false;return;}
            if(index!=current || Progress.active!=steps[index].persistentId)ShowStep(index);
            body.text=TutorialLessons.Text(steps[index],game.TutorialQuestions);
            Canvas.ForceUpdateCanvases();Layout();
            noteAlpha.alpha=(DeskPreferences.Current.reduceMotion || AccessPreferences.Current.reduceFlashes)?1:Mathf.Lerp(.65f,1,(Time.unscaledTime-noteShownAt)/.18f);
            float alpha=(DeskPreferences.Current.reduceMotion || AccessPreferences.Current.reduceFlashes)?1:.7f+.3f*Mathf.Abs(Mathf.Sin(Time.unscaledTime*2));
            foreach(var f in frame)f.color=Kit.WithAlpha(Kit.Hex("#C6A24F"),alpha);
        }
        public bool ExplainDiplomacy() {
            if(!ready || !DeskPreferences.Current.tutorial || Progress.Resolved("relations"))return false;
            Progress.Observe("decisionHelp");Progress.active="relations";ShowStep(System.Array.FindIndex(steps,s=>s.persistentId=="relations"));return true;
        }
        public void Next() {
            if(current<0 || !string.IsNullOrEmpty(steps[current].actionFact))return;
            Progress.Complete(steps[current].persistentId);current=-1;
        }
        public void SkipLesson() {
            if(current<0)return;
            int lesson=steps[current].lesson;
            foreach(var s in steps.Where(s=>s.lesson==lesson))if(!Progress.Resolved(s.persistentId))Progress.skipped.Add(s.persistentId);
            Progress.active="";current=-1;
        }
        // Compatibility API: omission is not completion and never writes a global PlayerPrefs flag.
        public void CompleteTutorial() {
            foreach(var s in steps)if(!Progress.Resolved(s.persistentId))Progress.skipped.Add(s.persistentId);
            Progress.active="";current=-1;
        }
        void ShowStep(int index) {
            current=index;target=null;noteShownAt=Time.unscaledTime;var s=steps[index];Progress.active=s.persistentId;
            var group=steps.Where(x=>x.lesson==s.lesson).ToArray();
            header.text=$"JULIÁN PAZ · JEFE DE GABINETE\n{s.lesson+1}/12 · {TutorialLessons.Titles[s.lesson]} · {System.Array.IndexOf(group,s)+1}/{group.Length}";
            body.text=TutorialLessons.Text(s,game.TutorialQuestions);
            hint.text=string.IsNullOrEmpty(s.actionFact)?"":"Realice la acción o salte esta lección.";
            next.interactable=string.IsNullOrEmpty(s.actionFact);
        }
        void BuildOverlay() {
            var go=new GameObject("TutorialCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.transform.SetParent(transform,false);
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=500;canvas.enabled=false;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            overlay=(RectTransform)go.transform;
            for(int i=0;i<4;i++){blockers[i]=Kit.Panel(overlay,Color.clear,Vector2.zero,Vector2.zero);blockers[i].raycastTarget=false;frame[i]=Kit.Panel(overlay,Kit.Hex("#C6A24F"),Vector2.zero,Vector2.zero);frame[i].raycastTarget=false;}
            var paper=Kit.Panel(overlay,Kit.Hex("#F3E7CF"),Vector2.zero,Vector2.zero,default,default,"AdvisorNote");paper.raycastTarget=true;
            bubble=paper.rectTransform;bubble.pivot=Vector2.zero;noteAlpha=paper.gameObject.AddComponent<CanvasGroup>();
            var outline=paper.gameObject.AddComponent<Outline>();outline.effectColor=Kit.Hex("#C6A24F");outline.effectDistance=new Vector2(2,-2);
            header=Kit.Label(bubble,"",17,Kit.Hex("#7A1F2A"),TextAnchor.UpperLeft,Vector2.zero,Vector2.one,new Vector2(16,0),new Vector2(-16,-10),FontStyle.Bold);
            body=Kit.Label(bubble,"",22,Kit.Hex("#2B2B2B"),TextAnchor.UpperLeft,Vector2.zero,Vector2.one,new Vector2(16,78),new Vector2(-16,-62),FontStyle.Normal,true);
            hint=Kit.Label(bubble,"",14,Pal.InkSoft,TextAnchor.LowerLeft,Vector2.zero,Vector2.one,new Vector2(16,52),new Vector2(-16,-10));
            Kit.MakeButton(bubble,"CONSULTAR",15,Kit.Hex("#7A4E2E"),Pal.Cream,()=>menus.ShowLesson(steps[current].lesson),new Vector2(0,0),new Vector2(.33f,0),new Vector2(12,12),new Vector2(-4,44));
            next=Kit.MakeButton(bubble,"SIGUIENTE",15,Kit.Hex("#2F4B3A"),Pal.Cream,Next,new Vector2(.33f,0),new Vector2(.67f,0),new Vector2(4,12),new Vector2(-4,44));
            Kit.MakeButton(bubble,"SALTAR",15,Kit.Hex("#7A1F2A"),Pal.Cream,SkipLesson,new Vector2(.67f,0),new Vector2(1,0),new Vector2(4,12),new Vector2(-12,44));
        }
        void Layout()
        {
            if (!IsActive) return;
            var step = steps[current];
            var size = overlay.rect.size;
            bool hasHole = TryGetHole(step, size, out var hole);

            if (!hasHole)
            {
                Place(blockers[0], new Rect(Vector2.zero, size));
                for (int i = 1; i < blockers.Length; i++) Place(blockers[i], Rect.zero);
                foreach (var f in frame) Place(f, Rect.zero);
            }
            else
            {
                Place(blockers[0], Rect.MinMaxRect(0, hole.yMax, size.x, size.y));           // arriba
                Place(blockers[1], Rect.MinMaxRect(0, 0, size.x, hole.yMin));                // abajo
                Place(blockers[2], Rect.MinMaxRect(0, hole.yMin, hole.xMin, hole.yMax));     // izquierda
                Place(blockers[3], Rect.MinMaxRect(hole.xMax, hole.yMin, size.x, hole.yMax)); // derecha
                const float k = FrameThickness;
                Place(frame[0], Rect.MinMaxRect(hole.xMin - k, hole.yMax, hole.xMax + k, hole.yMax + k));
                Place(frame[1], Rect.MinMaxRect(hole.xMin - k, hole.yMin - k, hole.xMax + k, hole.yMin));
                Place(frame[2], Rect.MinMaxRect(hole.xMin - k, hole.yMin, hole.xMin, hole.yMax));
                Place(frame[3], Rect.MinMaxRect(hole.xMax, hole.yMin, hole.xMax + k, hole.yMax));
            }

            // Si un paso pide interactuar pero no encuentra qué destacar, no bloqueamos nada:
            // mejor un tutorial sin foco que un jugador atascado.
            bool block = false;
            foreach (var b in blockers) b.raycastTarget = block;

            PlaceBubble(hasHole, hole, size);
        }

        bool TryGetHole(TutorialStepData step, Vector2 size, out Rect hole)
        {
            hole = default;
            if (!string.IsNullOrEmpty(step.targetName))
            {
                if (target == null || !target.gameObject.activeInHierarchy) target = FindTarget(step.targetName);
                if (target != null)
                {
                    var corners = new Vector3[4];
                    target.GetWorldCorners(corners); // en un Canvas Overlay, mundo = píxeles de pantalla
                    var min = new Vector2(float.MaxValue, float.MaxValue);
                    var max = new Vector2(float.MinValue, float.MinValue);
                    foreach (var c in corners)
                    {
                        var p = ToOverlay(c);
                        min = Vector2.Min(min, p);
                        max = Vector2.Max(max, p);
                    }
                    float pad = step.targetPadding;
                    hole = Rect.MinMaxRect(min.x - pad, min.y - pad, max.x + pad, max.y + pad);
                    hole = Clamp(hole, size);
                    return true;
                }
            }
            if (step.spotlightAnchor.x >= 0f && step.spotlightAnchor.y >= 0f)
            {
                var center = Vector2.Scale(step.spotlightAnchor, size);
                float r = step.spotlightRadius;
                hole = Clamp(new Rect(center.x - r, center.y - r, 2 * r, 2 * r), size);
                return true;
            }
            return false;
        }

        RectTransform FindTarget(string name)
        {
            if (gameCanvas == null) return null;
            foreach (var rt in gameCanvas.GetComponentsInChildren<RectTransform>())
                if (rt.name == name) return rt;
            return null;
        }

        Vector2 ToOverlay(Vector3 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, screenPoint, null, out var local);
            return local + overlay.rect.size * 0.5f; // de centro a esquina inferior izquierda
        }

        /// <summary>Burbuja al lado del foco donde quepa (derecha, izquierda, abajo, arriba), sin tapar el HUD.</summary>
        void PlaceBubble(bool hasHole, Rect hole, Vector2 size)
        {
            // Reserve the central paper, diplomatic tabs/questions and all decision buttons.
            Vector2 b = new Vector2(Mathf.Min(450, size.x * .235f), 300);
            Vector2 pos = new Vector2(20, size.y * .15f);
            if(game.ScreenName=="Introducción") pos.y=size.y*.38f;
            if(game.ScreenName=="Visita") { b.x=size.x*.44f; b.y=250;pos=new Vector2(size.x*.03f,size.y*.38f); }
            if(game.ScreenName=="Mapa") { b.x=size.x*.18f;b.y=380;pos=new Vector2(size.x*.80f,size.y*.18f); }
            if(menus.IsVisible) { b=new Vector2(size.x*.42f,205);pos=new Vector2(size.x*.54f,size.y*.045f); }
            bubble.sizeDelta=b;
            bubble.anchorMin=bubble.anchorMax=Vector2.zero;
            bubble.anchoredPosition=pos;
            bubble.SetAsLastSibling();
        }

        static void Place(Graphic g, Rect r)
        {
            var rt = g.rectTransform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            rt.anchoredPosition = r.position;
            rt.sizeDelta = new Vector2(Mathf.Max(0, r.width), Mathf.Max(0, r.height));
        }

        static Rect Clamp(Rect r, Vector2 size) =>
            Rect.MinMaxRect(Mathf.Max(0, r.xMin), Mathf.Max(0, r.yMin), Mathf.Min(size.x, r.xMax), Mathf.Min(size.y, r.yMax));
    }

}


