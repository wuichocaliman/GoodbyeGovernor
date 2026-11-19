using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 // Contextual presentation inside the existing adviser note. Never changes campaign or preferences.
 public sealed class OfficeAccessGuidance:MonoBehaviour {
  DespachoGame game;ScrollRect scroll;Text body;string original,last;Vector2 originalOffset;Button help,glossary;
  public void Initialize(DespachoGame owner,ScrollRect area){game=owner;scroll=area;body=area.content.GetComponent<Text>();original=body.text;originalOffset=((RectTransform)area.transform).offsetMin;
   help=Kit.MakeButton(transform,"AYUDA",19,Pal.WoodLight,Pal.Cream,()=>{game.GetComponent<DeskMenus>().ShowPause();game.GetComponent<DeskMenus>().ShowLesson(2);},new Vector2(0,0),new Vector2(.49f,0),new Vector2(12,12),new Vector2(-4,66));help.name="GuidedHelp";
   glossary=Kit.MakeButton(transform,"GLOSARIO",19,Pal.WoodLight,Pal.Cream,()=>{AccessController.Instance.OpenCategory("Glosario");},new Vector2(.49f,0),new Vector2(1,0),new Vector2(4,12),new Vector2(-12,66));glossary.name="GuidedGlossary";Refresh();
  }
  void LateUpdate()=>Refresh();
  void Refresh(){if(game==null)return;var p=AccessPreferences.Current;bool guided=p.navigationHelp;bool active=guided||p.simpleDocuments||p.simpleLanguage;
   help.gameObject.SetActive(guided||p.simpleDocuments);glossary.gameObject.SetActive(p.glossary);var rt=(RectTransform)scroll.transform;rt.offsetMin=new Vector2(originalOffset.x,(active||p.glossary)?82:originalOffset.y);
   bool large=p.textScale>=1.5f;var hr=(RectTransform)help.transform;var gr=(RectTransform)glossary.transform;
   hr.anchorMin=Vector2.zero;hr.anchorMax=new Vector2(large?1:.49f,0);hr.offsetMin=new Vector2(12,large?70:12);hr.offsetMax=new Vector2(large?-12:-4,large?126:66);
   gr.anchorMin=new Vector2(large?0:.49f,0);gr.anchorMax=new Vector2(1,0);gr.offsetMin=new Vector2(large?12:4,12);gr.offsetMax=new Vector2(-12,66);
   if(large&&(active||p.glossary))rt.offsetMin=new Vector2(originalOffset.x,142);
   string text=original;
   if(active){string step;
    if(game.TutorialAnswered)step="Decisión aplicada. Revise el resultado. Pulse CONTINUAR para archivar.";
    else if(!game.TutorialHasPaper)step="Mesa despejada. Consulte el MAPA; después podrá cerrar el año.";
    else if(game.TutorialOpen)step=AccessController.Instance.SelectedStamp!=null?"Sello seleccionado. Pulse el papel para confirmar. ESC cancela.":p.simpleDocuments?"Lea la petición. Pulse un sello para ver sus efectos; después pulse el papel para responder.":"Lea la petición. Arrastre un sello hasta el papel para responder.";
    else {var d=game.GetComponentInChildren<DeskDocument>();step=p.simpleDocuments?(d!=null&&d.AccessSelected?"2. Pulse la zona de lectura iluminada. El expediente se abrirá. ESC cancela.":"1. Pulse el expediente de la bandeja derecha. Después pulse la zona de lectura."):"Arrastre el expediente de la bandeja derecha al centro de la mesa. Allí se abrirá.";}
    text=(p.symbols?"[>] ":"")+step;
    if(guided)text+="\n\n"+(p.symbols?"[=] ":"")+"Estabilidad: orden y confianza.\n"+(p.symbols?"[S] ":"")+"Seguridad: defensa.\n"+(p.symbols?"[$] ":"")+"Prosperidad: economía.\nSi uno llega a 0, termina el mandato.";

    text+="\n\nAYUDA: Tutorial Presidencial. F1: controles. El mapa abre las visitas; La Gaceta reúne noticias.";
   }
   if(p.symbols)text+="\n\n"+game.AccessIndicators();
   if(active&&p.keyboard)text+="\nTeclado: TAB o flechas seleccionan. INTRO activa; ESC cancela.";
   if(text!=last){last=text;body.text=text;scroll.verticalNormalizedPosition=1;}
  }
 }
}
