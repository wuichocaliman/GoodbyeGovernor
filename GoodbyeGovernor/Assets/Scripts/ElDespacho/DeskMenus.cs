using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ElDespacho
{
    // Dedicated overlay: never deletes or reconstructs the live game to pause it.
    public sealed class DeskMenus : MonoBehaviour
    {
        public const string Version="v0.8-prototipo";
        DespachoGame game;
        Transform root, page;
        CanvasGroup gameInput;
        Canvas canvas;
        GameObject pauseLauncher;
        Image brightness;
        Texture2D mainArt, settingsArt;
        bool visible;
#if ENABLE_INPUT_SYSTEM
        UnityEngine.InputSystem.InputAction escapeAction;
        bool escapeRequested;
#endif
        int desiredWidth, desiredHeight;
        bool desiredFullscreen;
        float oldTimeScale=1, autosaveElapsed;
        string pageName, notice="", lastSave="Sin guardados en esta sesión";
        UnityAction back;
        Coroutine fade;
        public bool IsVisible => visible;
        public string PageName => pageName;
        DeskPreferences prefs => DeskPreferences.Current;
        public void Initialize(DespachoGame owner, Transform gameRoot)
        {
            game=owner;
#if ENABLE_INPUT_SYSTEM
            escapeAction=new UnityEngine.InputSystem.InputAction("Pause",UnityEngine.InputSystem.InputActionType.Button,"<Keyboard>/escape");
            escapeAction.performed += _ => escapeRequested=true;
            escapeAction.Enable();
#endif
            gameInput=gameRoot.gameObject.AddComponent<CanvasGroup>();
            var go=new GameObject("PresidentialMenus",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            go.transform.SetParent(transform,false); root=go.transform;
            canvas=go.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=400;
            var scaler=go.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=0.5f;
            mainArt=Resources.Load<Texture2D>("Menus/PresidentialOffice"); settingsArt=Resources.Load<Texture2D>("Menus/GreenLamp");
            notice=DeskPreferences.Load() ?? "";
            prefs.Apply(true);
            desiredWidth=prefs.width; desiredHeight=prefs.height; desiredFullscreen=prefs.fullscreen;
            var shade=new GameObject("Brightness",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
            shade.transform.SetParent(transform,false); var bc=shade.GetComponent<Canvas>(); bc.renderMode=RenderMode.ScreenSpaceOverlay; bc.sortingOrder=800;
            brightness=Kit.Panel(shade.transform,Color.clear,Vector2.zero,Vector2.one); brightness.raycastTarget=false;
            var launch=new GameObject("PauseShortcut",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            launch.transform.SetParent(transform,false); pauseLauncher=launch;
            var lc=launch.GetComponent<Canvas>();lc.renderMode=RenderMode.ScreenSpaceOverlay;lc.sortingOrder=300;
            var ls=launch.GetComponent<CanvasScaler>();ls.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;ls.referenceResolution=new Vector2(1920,1080);ls.matchWidthOrHeight=.5f;
            var pauseButton = Kit.MakeButton(launch.transform,"PAUSA · ESC",16,Pal.WoodMid,Pal.Cream,ShowPause,new Vector2(.89f,.909f),new Vector2(.99f,.936f));
            DeskMenuSkin.Apply(pauseButton, true);
            launch.SetActive(false);
            visible=false;
        }
        void Update()
        {
            bool escape=false;
#if ENABLE_INPUT_SYSTEM
            escape=escapeRequested;
            escapeRequested=false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            escape=Input.GetKeyDown(KeyCode.Escape);
#endif
            if(escape) { if(!visible && game.HasSession) ShowPause(); else if(visible && back!=null) back(); }
            if(!visible && game.HasSession && prefs.autosave)
            {
                autosaveElapsed+=Time.unscaledDeltaTime;
                if(autosaveElapsed>=prefs.autosaveMinutes*60) { autosaveElapsed=0; SaveSlot(0); }
            }
            brightness.color=prefs.brightness<=1 ? new Color(0,0,0,1-prefs.brightness) : new Color(1,1,1,(prefs.brightness-1)*0.6f);
        }
        void LateUpdate()
        {
            pauseLauncher.SetActive(!visible && game.HasSession);
            var tutorial=TutorialManager.Instance;
            if(tutorial!=null) tutorial.SetMenuSuspended(visible || !prefs.tutorial);
        }
        void OnDestroy() {
#if ENABLE_INPUT_SYSTEM
            escapeAction?.Dispose();
#endif
            if(manualSteps!=null)foreach(var step in manualSteps)Destroy(step);
            if(DeskPreferences.Paused) Time.timeScale=oldTimeScale; DeskPreferences.Paused=false;
        }
        void Open()
        {
            if(!visible) {
                foreach(var doc in game.GetComponentsInChildren<DeskDocument>()) doc.CancelDrag();
                foreach(var stamp in game.GetComponentsInChildren<StampTool>()) stamp.CancelDrag();
                oldTimeScale=Time.timeScale;
            }
            visible=true; DeskPreferences.Paused=true; Time.timeScale=0;
            gameInput.interactable=gameInput.blocksRaycasts=false;
            canvas.gameObject.SetActive(true);
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
        }
        public void Resume()
        {
            visible=false; DeskPreferences.Paused=false; Time.timeScale=oldTimeScale;
            gameInput.interactable=gameInput.blocksRaycasts=true;
            canvas.gameObject.SetActive(false); back=null;
        }
        void NewPage(string title, bool settings=false)
        {
            Open(); pageName=title;
            if(fade!=null) StopCoroutine(fade);
            if(page!=null) { page.gameObject.SetActive(false); Destroy(page.gameObject); }
            page=Kit.Panel(root,Pal.Wood,Vector2.zero,Vector2.one,default,default,"Menu_"+title).transform;
            var art=settings?settingsArt:mainArt;
            if(art!=null) {
                var image=new GameObject("OfficeArtwork",typeof(RectTransform),typeof(RawImage)); image.transform.SetParent(page,false);
                var rt=(RectTransform)image.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero;
                image.GetComponent<RawImage>().texture=art;
                // Fill the viewport without deforming the supplied 16:9 artwork.
                // A mask clips the small overflow on other aspect ratios.
                var fit = image.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = art.width / (float)art.height;
                page.gameObject.AddComponent<RectMask2D>();
                image.GetComponent<RawImage>().raycastTarget=false;
            }
            Kit.Panel(page,new Color(0.035f,0.04f,0.03f,prefs.highContrast?0.6f:0.14f),Vector2.zero,Vector2.one);
            Kit.Panel(page,Kit.Hex("#bc9959"),new Vector2(0.035f,0.93f),new Vector2(0.965f,0.933f));
            Label("REPÚBLICA DE AURELIA  /  PRESIDENCIA",19,Pal.Cream,.04f,.94f,.8f,.98f);
            Label(Version,17,Pal.Cream,.8f,.94f,.96f,.98f,TextAnchor.MiddleRight);
            if(!string.IsNullOrEmpty(notice)) Label(notice,18,Pal.Cream,.04f,.015f,.95f,.055f);
            var group=page.gameObject.AddComponent<CanvasGroup>();
            fade=StartCoroutine(Fade(group));
        }
        IEnumerator Fade(CanvasGroup group)
        {
            float duration=prefs.reduceMotion?0:0.16f;
            for(float t=0;t<duration;t+=Time.unscaledDeltaTime) { group.alpha=Mathf.Lerp(0.6f,1,t/duration); yield return null; }
            group.alpha=1;
        }
        Text Label(string text,int size,Color color,float x,float y,float right,float top,TextAnchor anchor=TextAnchor.MiddleLeft)
            => Kit.Label(page,text,Mathf.RoundToInt(size*prefs.menuTextScale),color,anchor,new Vector2(x,y),new Vector2(right,top),default,default,FontStyle.Normal,true);
        void Paper(float x,float y,float right,float top)
        {
            Kit.Panel(page,Kit.WithAlpha(Kit.Hex("#131e1b"), prefs.highContrast ? .96f : pageName=="Principal" ? .76f : .72f),new Vector2(x,y),new Vector2(right,top));
            Kit.Panel(page,Pal.Gold,new Vector2(x,y),new Vector2(x+.002f,top));
        }
        Button Button(string label,UnityAction action,float x,float y,float right,float top,bool enabled=true)
        {
            var b=Kit.MakeButton(page,label,Mathf.RoundToInt(23*prefs.menuTextScale),prefs.highContrast?Color.black:Kit.Hex("#25392f"),Pal.Cream,()=>{Sfx.Play(Sfx.Click);action?.Invoke();},new Vector2(x,y),new Vector2(right,top));
            DeskMenuSkin.Apply(b);
            b.name=label; b.interactable=enabled; return b;
        }
        void Heading(string title,string subtitle)
        {
            Label(title,48,Pal.Gold,.07f,.78f,.91f,.88f);
            Label(subtitle,22,Pal.Cream,.07f,.71f,.92f,.78f);
        }
        public void ShowMain()
        {
            NewPage("Principal"); back=null;
            Paper(.045f,.075f,.395f,.9f);
            Label("EL\nDESPACHO",72,Pal.Gold,.075f,.65f,.37f,.87f);
            Label("EL PODER DE CADA DECISIÓN",17,Pal.Cream,.075f,.60f,.37f,.65f);
            int latest=DeskSaveStore.LatestSlot();
            string[] labels={"CONTINUAR","NUEVA PARTIDA","CARGAR PARTIDA","AJUSTES","CÓMO JUGAR","CRÉDITOS","SALIR"};
            UnityAction[] actions={()=>LoadSlot(latest),()=>ConfirmNew(),()=>ShowSlots(false),()=>ShowSettings(0),()=>ShowHelp(),()=>ShowCredits(),()=>ConfirmExit()};
            for(int i=0;i<labels.Length;i++) Button(labels[i],actions[i],.075f,.52f-i*.067f,.365f,.585f-i*.067f,i!=0||latest>=0);
            Paper(.56f,.105f,.94f,.27f);
            string summary=latest>=0&&DeskSaveStore.TryRead(latest,out var s,out _) ? Describe(s) : "No hay partidas guardadas.\nUn nuevo mandato espera tu firma.";
            Label("ÚLTIMO EXPEDIENTE GUARDADO",18,Pal.Gold,.58f,.22f,.92f,.26f);
            Label(summary,23,Pal.Cream,.58f,.12f,.92f,.22f);
        }
        void ConfirmNew()
        {
            UnityAction start=()=>{ Resume(); autosaveElapsed=0; game.StartFromMenu(); };
            if(DeskSaveStore.LatestSlot()>=0) Confirm("NUEVO MANDATO","Los guardados manuales se conservan. El próximo autoguardado sustituirá al anterior.",start,ShowMain);
            else start();
        }
        public void ShowPause()
        {
            if(!game.HasSession) return;
            NewPage("Pausa"); back=Resume;
            Paper(.05f,.07f,.95f,.9f); Heading("MANDATO EN PAUSA",game.SessionSummary);
            string[] labels={"CONTINUAR","GUARDAR PARTIDA","CARGAR PARTIDA","AJUSTES","CÓMO JUGAR","VOLVER AL MENÚ PRINCIPAL","SALIR DEL JUEGO","GUARDAR Y VOLVER AL MENÚ","GUARDAR Y SALIR"};
            UnityAction[] actions={Resume,()=>ShowSlots(true),()=>ShowSlots(false),()=>ShowSettings(0),ShowHelp,
                ()=>Confirm("VOLVER AL MENÚ","Perderás los cambios posteriores al último guardado.",ReturnMain,ShowPause),ConfirmExit,
                ()=>SaveThen(ReturnMain),()=>SaveThen(Quit)};
            for(int i=0;i<labels.Length;i++) { int col=i<5?0:1, row=i<5?i:i-5; Button(labels[i],actions[i],.08f+col*.45f,.60f-row*.095f,.48f+col*.45f,.675f-row*.095f); }
            Label(lastSave,19,Pal.Cream,.08f,.08f,.91f,.15f);
        }
        void ReturnMain() { Resume(); game.ReturnToTitle(); }
        void ReturnContext() { if(game.HasSession) ShowPause(); else ShowMain(); }
        static string Describe(DespachoGame.SaveData s) => $"Año {s.year} · {new DateTime(s.utcTicks,DateTimeKind.Utc).ToLocalTime():dd/MM/yyyy HH:mm}\nEst. {s.estabilidad} / Seg. {s.seguridad} / Prosp. {s.prosperidad} · {s.cardsThisYear} expedientes";
        public bool SaveSlot(int slot)
        {
            try { DeskSaveStore.Write(slot,game.CaptureSave()); lastSave=$"{(slot==0?"Autoguardado":"Guardado "+slot)} · {DateTime.Now:HH:mm:ss}"; notice=lastSave; return true; }
            catch(Exception e) { notice="No se pudo guardar: "+e.Message; return false; }
        }
        void SaveThen(UnityAction action)
        {
            Confirm("GUARDAR EN SLOT 1","Se sustituirá el contenido del slot 1 antes de continuar.",()=>{if(SaveSlot(1))action();else ShowPause();},ShowPause);
        }
        public void ShowSlots(bool saving)
        {
            NewPage(saving?"Guardar":"Cargar"); back=ReturnContext;
            Paper(.05f,.07f,.95f,.9f); Heading(saving?"ARCHIVAR EL MANDATO":"ARCHIVO DE MANDATOS","Tres expedientes manuales y un autoguardado independiente.");
            for(int i=0;i<=3;i++) {
                int slot=i; bool valid=DeskSaveStore.TryRead(i,out var data,out var error);
                float y=.56f-i*.125f;
                Label((i==0?"AUTOGUARDADO":"SLOT "+i)+"\n"+(valid?Describe(data):error),23,Pal.Cream,.08f,y,.71f,y+.115f);
                Button(saving?"GUARDAR":"CARGAR",()=>{
                    if(saving) { if(valid || error!="Vacío") Confirm("SUSTITUIR GUARDADO","Se reemplazará este slot. La versión anterior quedará como copia .bak.",()=>{SaveSlot(slot);ShowSlots(true);},()=>ShowSlots(true)); else {SaveSlot(slot);ShowSlots(true);} }
                    else if(game.HasSession) Confirm("CARGAR MANDATO","Se descartarán los cambios no guardados de la partida actual.",()=>LoadSlot(slot),()=>ShowSlots(false));
                    else LoadSlot(slot);
                },.75f,y+.015f,.91f,y+.085f,saving?i>0:valid);
            }
            Button("VOLVER",ReturnContext,.75f,.08f,.91f,.14f);
        }
        public void LoadSlot(int slot)
        {
            if(slot<0 || !DeskSaveStore.TryRead(slot,out var data,out var error)) { notice="No se pudo cargar el guardado."; ShowSlots(false); return; }
            Resume(); game.RestoreSave(data); autosaveElapsed=0; lastSave="Cargado · "+new DateTime(data.utcTicks,DateTimeKind.Utc).ToLocalTime().ToString("g");
        }
        void Confirm(string title,string message,UnityAction yes,UnityAction no)
        {
            NewPage("Confirmación"); back=no;
            Paper(.19f,.27f,.81f,.73f);
            Label(title,40,Pal.Gold,.23f,.59f,.77f,.69f); Label(message,27,Pal.Cream,.23f,.43f,.77f,.58f);
            Button("CONFIRMAR",yes,.23f,.32f,.48f,.40f); Button("CANCELAR",no,.52f,.32f,.77f,.40f);
        }
        void ConfirmExit() => Confirm("SALIR DEL DESPACHO",game.HasSession?"Los cambios sin guardar se perderán.":"¿Deseas cerrar El Despacho?",Quit,ReturnContext);
        void Quit()
        {
            Resume();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        TutorialStepData[] manualSteps;
        TutorialStepData[] ManualSteps => manualSteps ?? (manualSteps=TutorialLessons.Create(Resources.LoadAll<TutorialStepData>("Tutorial").OrderBy(s=>s.name).ToArray()));
        void ShowHelp()
        {
            NewPage("Cómo jugar"); back=ReturnContext; Paper(.05f,.07f,.95f,.9f);
            Heading("TUTORIAL PRESIDENCIAL","Julián Paz · Consulta libre: no cambia su campaña ni el progreso de la guía.");
            for(int i=0;i<12;i++) {
                int lesson=i,col=i/6,row=i%6;
                Button((i+1)+". "+TutorialLessons.Titles[i],()=>ShowLesson(lesson),.08f+col*.45f,.60f-row*.075f,.48f+col*.45f,.66f-row*.075f);
            }
            Button("VOLVER",ReturnContext,.74f,.09f,.91f,.15f);
        }
        public void ShowLesson(int lesson,int step=0)
        {
            var pages=ManualSteps.Where(s=>s.lesson==lesson).ToArray();step=Mathf.Clamp(step,0,pages.Length-1);
            NewPage("Consulta");back=ShowHelp;Paper(.08f,.12f,.92f,.9f);
            Heading(TutorialLessons.Titles[lesson],$"Julián Paz · Lección {lesson+1}/12 · Nota {step+1}/{pages.Length} · Solo consulta");
            Label(TutorialLessons.Text(pages[step],game.HasSession?game.TutorialQuestions:3),32,Pal.Cream,.13f,.36f,.87f,.69f);
            Label("Puede revisar estas notas sin realizar acciones. ESC vuelve al índice. En el año 4 hay dos preguntas por visita.",22,Pal.Cream,.13f,.25f,.87f,.34f);
            int previous=step-1,following=step+1;
            Button("ATRÁS",()=>{if(previous>=0)ShowLesson(lesson,previous);else ShowLesson(lesson-1);},.13f,.16f,.34f,.23f,step>0||lesson>0);
            Button("VOLVER",ShowHelp,.40f,.16f,.60f,.23f);
            Button("SIGUIENTE",()=>{if(following<pages.Length)ShowLesson(lesson,following);else ShowLesson(lesson+1);},.66f,.16f,.87f,.23f,step<pages.Length-1||lesson<11);
        }
        void ShowCredits()
        {
            NewPage("Créditos"); back=ShowMain; Paper(.05f,.07f,.95f,.9f);
            Heading("CRÉDITOS","El Despacho · proyecto universitario colaborativo");
            Label("Diseño, narrativa y desarrollo\nEquipo de El Despacho\n\nFondos de menú\nImágenes aportadas por el equipo\n\nPrototipo construido con Unity y uGUI.\nLos nombres individuales y la atribución definitiva de las imágenes\nquedan pendientes de completar por el equipo.",30,Pal.Cream,.09f,.25f,.91f,.67f);
            Button("VOLVER",ShowMain,.73f,.10f,.91f,.17f);
        }
        void Persist()
        {
            try { prefs.Apply(false); prefs.Save(); notice="Ajustes guardados"; }
            catch(Exception e) { notice="No se guardaron los ajustes: "+e.Message; }
        }
        public void ShowSettings(int category)
        {
            NewPage("Ajustes",true); back=()=>{Persist();ReturnContext();};
            Paper(.045f,.07f,.95f,.9f); Heading("AJUSTES DEL DESPACHO","Preferencias personales · Se guardan al cambiar cada opción");
            string[] tabs={"AUDIO","PANTALLA","JUGABILIDAD","CONTROLES","ACCESIBILIDAD"};
            for(int i=0;i<tabs.Length;i++) {int tab=i; Button((category==i?"● ":"")+tabs[i],()=>ShowSettings(tab),.07f,.61f-i*.095f,.30f,.685f-i*.095f);}
            if(category==0) {
                SliderRow("Volumen maestro",prefs.master,0,1,.61f,v=>prefs.master=v);
                SliderRow("Efectos",prefs.effects,0,1,.48f,v=>prefs.effects=v);
                SliderRow("Interfaz",prefs.ui,0,1,.35f,v=>prefs.ui=v);
                Label("Música: este prototipo no incluye pista musical.",22,Pal.Cream,.35f,.23f,.91f,.31f);
            } else if(category==1) {
                ToggleRow("Pantalla completa",desiredFullscreen,.63f,()=>{desiredFullscreen=!desiredFullscreen;ShowSettings(1);});
                Button($"RESOLUCIÓN: {desiredWidth} × {desiredHeight}",()=>{ int[] widths={1280,1600,1920,2560}; int idx=(Array.IndexOf(widths,desiredWidth)+1)%widths.Length; desiredWidth=widths[idx];desiredHeight=desiredWidth*9/16;ShowSettings(1);},.35f,.54f,.91f,.61f);
                ToggleRow("VSync",prefs.vsync,.44f,()=>{prefs.vsync=!prefs.vsync;Persist();ShowSettings(1);});
                Button($"LÍMITE FPS: {prefs.fps}"+(prefs.vsync?" (prioridad VSync)":""),()=>{prefs.fps=prefs.fps==30?60:prefs.fps==60?120:30;Persist();ShowSettings(1);},.35f,.35f,.91f,.42f);
                SliderRow("Brillo",prefs.brightness,.65f,1.15f,.23f,v=>prefs.brightness=v);
                Button("APLICAR PANTALLA",ApplyDisplay,.35f,.14f,.64f,.21f);
                Label(Application.isEditor?"Resolución y pantalla completa se aplican en el ejecutable.":"Se confirmará el cambio durante 15 segundos.",18,Pal.Cream,.66f,.13f,.92f,.22f);
            } else if(category==2) {
                ToggleRow("Mostrar tutorial",prefs.tutorial,.62f,()=>{prefs.tutorial=!prefs.tutorial;Persist();ShowSettings(2);});
                ToggleRow("Autoguardado",prefs.autosave,.51f,()=>{prefs.autosave=!prefs.autosave;Persist();ShowSettings(2);});
                Button($"AUTOGUARDADO CADA {prefs.autosaveMinutes} MIN",()=>{prefs.autosaveMinutes=prefs.autosaveMinutes==1?3:prefs.autosaveMinutes==3?5:1;Persist();ShowSettings(2);},.35f,.40f,.91f,.48f);
                Label("Idioma: español\nTexto instantáneo, como en el juego original.\nLa guía se guarda con cada campaña.\nRepita las lecciones desde Cómo jugar.",24,Pal.Cream,.35f,.22f,.91f,.37f);
            } else if(category==3) {
                Label("Ratón izquierdo: coger, arrastrar y soltar\nESC: abrir pausa / volver\n\nEl papel y los sellos siguen el cursor 1:1.\nNo se altera su precisión con una sensibilidad artificial.",27,Pal.Cream,.35f,.34f,.91f,.68f);
                Button("RESTABLECER AJUSTES",()=>Confirm("RESTABLECER AJUSTES","Las preferencias volverán a sus valores iniciales. No se borrará ningún guardado.",()=>{DeskPreferences.Reset();desiredWidth=prefs.width;desiredHeight=prefs.height;desiredFullscreen=prefs.fullscreen;Persist();ShowSettings(3);},()=>ShowSettings(3)),.35f,.23f,.91f,.31f);
            } else {
                ToggleRow("Reducir movimiento",prefs.reduceMotion,.62f,()=>{prefs.reduceMotion=!prefs.reduceMotion;Persist();ShowSettings(4);});
                ToggleRow("Alto contraste en menús",prefs.highContrast,.51f,()=>{prefs.highContrast=!prefs.highContrast;Persist();ShowSettings(4);});
                Button($"TEXTO DE MENÚS: {Mathf.RoundToInt(prefs.menuTextScale*100)} %",()=>{prefs.menuTextScale=prefs.menuTextScale<1.1f?1.1f:prefs.menuTextScale<1.19f?1.2f:1f;Persist();ShowSettings(4);},.35f,.40f,.91f,.48f);
                Label("Los menús se adaptan a la resolución.\nLa ampliación de texto afecta a estos menús;\nlos expedientes conservan su maquetación actual.",24,Pal.Cream,.35f,.22f,.91f,.36f);
            }
            Button("VOLVER",()=>{Persist();ReturnContext();},.73f,.075f,.91f,.135f);
        }
        void ToggleRow(string label,bool on,float y,UnityAction toggle) => Button(label+"   ["+(on?"SÍ":"NO")+"]",toggle,.35f,y,.91f,y+.08f);
        void SliderRow(string label,float value,float min,float max,float y,Action<float> changed)
        {
            var caption=Label(label+"   "+Mathf.RoundToInt(value*100)+" %",24,Pal.Cream,.35f,y+.035f,.91f,y+.09f);
            var track=Kit.Panel(page,Pal.WoodLight,new Vector2(.36f,y),new Vector2(.90f,y+.027f));
            var handle=Kit.Panel(track.transform,Pal.Gold,Vector2.zero,Vector2.one);
            handle.rectTransform.sizeDelta=new Vector2(20,18);
            var slider=track.gameObject.AddComponent<Slider>(); slider.minValue=min;slider.maxValue=max;
            slider.handleRect=handle.rectTransform; slider.targetGraphic=handle; slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(v=>{changed(v);caption.text=label+"   "+Mathf.RoundToInt(v*100)+" %";Persist();});
        }
        void ApplyDisplay()
        {
            if(Application.isEditor) {notice="Prueba el cambio de resolución en una compilación del juego.";ShowSettings(1);return;}
            int oldWidth=Screen.width,oldHeight=Screen.height; bool oldFull=Screen.fullScreen;
            bool accepted=false;
            prefs.width=desiredWidth;prefs.height=desiredHeight;prefs.fullscreen=desiredFullscreen;
            prefs.Apply(true);
            Confirm("CONFIRMAR PANTALLA","Confirma en 15 segundos o se restaurará la pantalla anterior.",()=>{accepted=true;Persist();ShowSettings(1);},()=>{accepted=true;Revert();});
            StartCoroutine(Timeout());
            void Revert() {prefs.width=desiredWidth=oldWidth;prefs.height=desiredHeight=oldHeight;prefs.fullscreen=desiredFullscreen=oldFull;prefs.Apply(true);Persist();ShowSettings(1);}
            IEnumerator Timeout() {yield return new WaitForSecondsRealtime(15);if(!accepted)Revert();}
        }
    }
}
