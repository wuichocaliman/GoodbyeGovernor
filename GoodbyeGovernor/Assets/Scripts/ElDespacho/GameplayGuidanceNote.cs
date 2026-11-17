using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ElDespacho {
 public sealed class GameplayGuidanceNote:MonoBehaviour {
  Text text;string original;
  void Awake(){text=GetComponent<Text>();original=text.text;}
  void LateUpdate()=>Apply();
  public void Apply(){if(text==null)return;bool map=original=="Elige un país con el ratón.";text.text=AccessPreferences.Current.navigationHelp?(map?"HERRAMIENTAS: Pulse un país disponible. ! y + son sospechas, no certezas.":"HERRAMIENTAS: Cada pregunta consume una de las disponibles.\nDOSSIER: datos públicos. INTELIGENCIA: compare fuente y fecha; puede contener errores. COMERCIO: recursos.\nAlianza crea vínculos. Cortar lazos aplica la acción del botón. Usted decide.\n"+original):original;}
 }
}
