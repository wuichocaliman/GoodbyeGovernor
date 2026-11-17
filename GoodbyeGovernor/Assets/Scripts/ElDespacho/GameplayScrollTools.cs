using UnityEngine;using UnityEngine.UI;
namespace ElDespacho {public static class GameplayScrollTools {
 public static void AddBar(ScrollRect scroll){var view=scroll.viewport;var track=Kit.Panel(view,new Color(.3f,.3f,.3f,.8f),new Vector2(1,0),Vector2.one,new Vector2(-10,0),Vector2.zero);track.name="GameplayScrollBar";track.raycastTarget=true;var handle=Kit.Panel(track.transform,Pal.Gold,Vector2.zero,Vector2.one);handle.raycastTarget=true;var bar=track.gameObject.AddComponent<Scrollbar>();bar.handleRect=handle.rectTransform;bar.targetGraphic=handle;bar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=bar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;scroll.content.offsetMax=new Vector2(-18,scroll.content.offsetMax.y);}
}}
