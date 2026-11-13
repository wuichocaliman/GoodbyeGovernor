using UnityEngine;using UnityEngine.UI;
namespace ElDespacho {
 // Small monochrome pictograms; text remains the authoritative explanation.
 [RequireComponent(typeof(CanvasRenderer))] public sealed class AccessSymbol:MaskableGraphic {
  public string symbol="D";
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;
   void Poly(params Vector2[] pts){int n=v.currentVertCount;foreach(var p in pts)v.AddVert(new Vector3(r.xMin+p.x*r.width,r.yMin+p.y*r.height),color,Vector2.zero);for(int i=1;i<pts.Length-1;i++)v.AddTriangle(n,n+i,n+i+1);}
   void Box(float x,float y,float w,float h)=>Poly(new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h));
   switch(symbol){
    case "S":Poly(new Vector2(.15f,.9f),new Vector2(.85f,.9f),new Vector2(.8f,.35f),new Vector2(.5f,.05f),new Vector2(.2f,.35f));break;
    case "=":Box(.45f,.1f,.1f,.8f);Box(.15f,.75f,.7f,.08f);Box(.1f,.35f,.35f,.1f);Box(.55f,.35f,.35f,.1f);Box(.23f,.42f,.06f,.38f);Box(.71f,.42f,.06f,.38f);Box(.2f,.08f,.6f,.08f);break;
    case "$":Box(.1f,.15f,.8f,.18f);Box(.1f,.4f,.8f,.18f);Box(.1f,.65f,.8f,.18f);break;
    case "!":Box(.43f,.35f,.14f,.55f);Box(.43f,.08f,.14f,.15f);break;
    case "+":Box(.42f,.15f,.16f,.7f);Box(.15f,.42f,.7f,.16f);break;
    case "A":Poly(new Vector2(.1f,.8f),new Vector2(.3f,.6f),new Vector2(.5f,.95f),new Vector2(.7f,.6f),new Vector2(.9f,.8f),new Vector2(.8f,.2f),new Vector2(.2f,.2f));break;
    default:Box(.15f,.1f,.1f,.8f);Box(.75f,.1f,.1f,.8f);Box(.15f,.1f,.7f,.08f);Box(.15f,.82f,.7f,.08f);Box(.32f,.62f,.36f,.06f);Box(.32f,.45f,.36f,.06f);Box(.32f,.28f,.25f,.06f);break;
   }
  }
 }
}
