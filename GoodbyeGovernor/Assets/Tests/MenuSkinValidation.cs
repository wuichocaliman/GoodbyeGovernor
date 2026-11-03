#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using ElDespacho;

public static class MenuSkinValidation
{
    public static void Check(DeskMenus menus)
    {
        var results=new List<string>();
        void Assert(bool condition,string message)
        {
            results.Add((condition?"PASS ":"FAIL ")+message);
            Directory.CreateDirectory("Logs/MenuVisual-v071");
            File.WriteAllLines("Logs/MenuVisual-v071/results.txt",results);
            if(!condition)throw new Exception(message);
        }
        foreach(var name in new[]{"PresidentialOffice","GreenLamp","button_default","button_hover","button_pressed","button_disabled"})
        {
            var asset=Resources.Load<Texture2D>("Menus/"+name);
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Menus/"+name+".png");
            Assert(asset!=null&&!importer.mipmapEnabled&&importer.npotScale==TextureImporterNPOTScale.None&&importer.textureCompression==TextureImporterCompression.Uncompressed,"UI import: "+name);
            Assert(asset.width==(name.StartsWith("button")?2172:1672),"native width retained: "+name);
        }
        Canvas.ForceUpdateCanvases();
        var art=menus.GetComponentsInChildren<RawImage>().Single();
        Assert(Mathf.Abs(art.rectTransform.rect.width/art.rectTransform.rect.height-1672f/941f)<.002f,"background preserves original aspect ratio");
        Assert(art.uvRect==new Rect(0,0,1,1),"no obsolete white-border crop");
        var button=menus.GetComponentsInChildren<Button>().First(b=>b.name=="CARGAR PARTIDA");
        var image=button.GetComponent<Image>();var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=-1};
        EventSystem.current.SetSelectedGameObject(null);
        Assert(button.transition==Selectable.Transition.SpriteSwap&&image.type==Image.Type.Sliced,"sprite swap and nine-slice configured");
        Assert(image.sprite.name=="button_default" && image.sprite.texture==Resources.Load<Texture2D>("Menus/button_default"),"normal sprite");
        button.OnPointerEnter(pointer);
        Assert(image.overrideSprite.name=="button_hover" && image.overrideSprite.texture==Resources.Load<Texture2D>("Menus/button_hover"),"hover sprite");
        button.OnPointerDown(pointer);
        Assert(image.overrideSprite.name=="button_pressed" && image.overrideSprite.texture==Resources.Load<Texture2D>("Menus/button_pressed"),"pressed sprite");
        button.OnPointerUp(pointer);button.OnPointerExit(pointer);EventSystem.current.SetSelectedGameObject(null);
        button.interactable=false;
        Assert(image.overrideSprite.name=="button_disabled" && image.overrideSprite.texture==Resources.Load<Texture2D>("Menus/button_disabled"),"disabled sprite");
        button.interactable=true;button.OnPointerExit(pointer);
        var disabled=menus.GetComponentsInChildren<Button>().First(b=>b.name=="CONTINUAR");
        Assert(!disabled.interactable&&disabled.GetComponent<Image>().overrideSprite.name=="button_disabled" && disabled.GetComponent<Image>().overrideSprite.texture==Resources.Load<Texture2D>("Menus/button_disabled"),"Continue remains disabled without saves");
        Assert(button.GetComponentInChildren<Text>().rectTransform.offsetMin.x>=50,"text inset from decorative edges");
        results.Add("COMPLETE "+results.Count+" visual/import assertions");File.WriteAllLines("Logs/MenuVisual-v071/results.txt",results);
    }
}
#endif
