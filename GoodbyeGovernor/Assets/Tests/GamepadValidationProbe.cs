#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using ElDespacho;

/// <summary>
/// Prueba en Play mode del soporte de mando con un mando virtual (no hace falta uno físico):
/// foco, gatillo analógico y digital, anti-repetición, sellos, pausa con Start / B, paso a ratón y parada de motores.
/// Usa carpetas temporales: no toca partidas ni ajustes reales. Resultado en Logs/Gamepad-validation.txt.
/// </summary>
public class GamepadValidationProbe : MonoBehaviour
{
    DespachoGame game;
    Gamepad pad;
    bool previousVibration = true;
    const BindingFlags F = BindingFlags.NonPublic | BindingFlags.Instance;
    readonly List<string> results = new List<string>();
    object Get(string n) => typeof(DespachoGame).GetField(n, F).GetValue(game);
    void Call(string n, params object[] args) => typeof(DespachoGame).GetMethod(n, F).Invoke(game, args);
    DeskDocument Doc => (DeskDocument)Get("deskDocument");
    static float Motor(string n) => (float)typeof(Haptics).GetField(n, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    StampTool Stamp(string n) => game.GetComponentsInChildren<StampTool>().FirstOrDefault(s => s.name == n);
    GameObject Selected => EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

    void Check(bool ok, string label)
    {
        results.Add((ok ? "PASS " : "FAIL ") + label);
        File.WriteAllLines("Logs/Gamepad-validation.txt", results);
        if (!ok) throw new Exception(label);
    }

    void HandleLog(string message, string stack, LogType type)
    {
        if (type != LogType.Exception && type != LogType.Error) return;
        results.Add("ERROR " + message + "\n" + stack);
        File.WriteAllLines("Logs/Gamepad-validation.txt", results);
    }

    void Pad(float l2 = 0, float r2 = 0, Vector2 rightStick = default, params GamepadButton[] buttons)
    {
        var s = new GamepadState { leftTrigger = l2, rightTrigger = r2, rightStick = rightStick };
        foreach (var b in buttons) s = s.WithButton(b);
        InputSystem.QueueStateEvent(pad, s);
    }

    IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }

    IEnumerator Start()
    {
        Application.logMessageReceived += HandleLog;
        string temp = Path.Combine(Application.temporaryCachePath, "GamepadProbe");
        DeskPreferences.TestDirectory = temp;
        DeskSaveStore.TestDirectory = temp;
        yield return null;
        game = FindAnyObjectByType<DespachoGame>();
        Check(game != null, "game boots in Unity Play mode");
        var menus = game.GetComponent<DeskMenus>();
        menus.Resume();
        game.StartFromMenu();
        DeskPreferences.Current.tutorial = false;
        previousVibration = Haptics.Enabled; // PlayerPrefs real: se restaura al terminar
        Haptics.Enabled = true;
        DeskPreferences.Current.vibrationStrength = 1f;
        Call("NewGame"); Call("ShowOffice");
        yield return null;

        pad = InputSystem.AddDevice<Gamepad>("ProbeGamepad");
        Pad(rightStick: new Vector2(0, 0.5f)); yield return Frames(2);
        Pad(); yield return Frames(2);
        Check(GamepadInput.UsingGamepad && GamepadInput.ActivePad == pad, "touching the gamepad makes it the active device");
        Check(Selected != null && Selected.GetComponent<Selectable>() != null, "office has a selected button for the gamepad");
        Check(!new[] { "ALIANZA", "CORTAR", "ATACAR" }.Any(w => Selected.GetComponentInChildren<Text>()?.text.ToUpperInvariant().Contains(w) ?? false), "default focus is never an irreversible decision");

        // Gatillo a medias: el expediente sale y sigue la presión, pero no se abre
        Pad(r2: 0.5f); yield return Frames(30);
        Check(Doc.IsGuided && !Doc.CanDecide, "half R2 pulls the document without opening it");
        Pad(); yield return Frames(3);
        yield return new WaitForSecondsRealtime(0.35f);
        Check(Doc.GuidedAvailable && !Doc.CanDecide, "releasing R2 early returns the document to the tray");

        // Gatillo digital (0 → 1 de golpe): no confirma en el primer frame
        Pad(r2: 1f); yield return Frames(2);
        Check(Doc.IsGuided && !Doc.CanDecide, "a digital full press does not open instantly");
        yield return new WaitForSecondsRealtime(0.45f);
        yield return new WaitForSecondsRealtime(0.35f);
        Check(Doc.CanDecide, "holding R2 to the bottom opens the document");

        // Anti-repetición: el mismo R2 aún pulsado no puede sellar APROBADO
        yield return Frames(20);
        Check(!(bool)Get("cardAnswered") && !(Stamp("StampYes")?.IsGuided ?? false), "the R2 that opened the document cannot stamp until released");
        Pad(); yield return Frames(3);

        // L2 a medias y suelto: el sello baja y vuelve sin decidir
        Pad(l2: 0.5f); yield return Frames(20);
        Check(Stamp("StampNo") != null && Stamp("StampNo").IsGuided, "half L2 lowers the DENEGADO stamp");
        Pad(); yield return Frames(3);
        Check(!(bool)Get("cardAnswered"), "releasing L2 early does not decide");
        yield return new WaitForSecondsRealtime(0.3f);

        // Los dos gatillos a la vez: no se elige ninguno
        Pad(l2: 1f, r2: 1f); yield return Frames(20);
        Check(!(bool)Get("cardAnswered") && !Stamp("StampNo").IsGuided && !Stamp("StampYes").IsGuided, "both triggers together choose nothing");
        Pad(); yield return Frames(3);

        // R2 a fondo: sella APROBADO y el mando vibra
        Pad(r2: 1f);
        float strongest = 0, waited = 0;
        while (!(bool)Get("cardAnswered") && waited < 1f) { yield return null; waited += Time.unscaledDeltaTime; }
        for (int i = 0; i < 6; i++) { yield return null; strongest = Mathf.Max(strongest, Motor("lastLow")); }
        Check((bool)Get("cardAnswered") && (bool)Get("stampYes"), "R2 to the bottom stamps APROBADO");
        Check(strongest > 0.5f, "stamping sends a strong low-motor pulse");
        // Desactivar el ajuste para los motores en el acto y omite los pulsos siguientes
        Haptics.StampHit(); yield return null;
        Haptics.Enabled = false; yield return Frames(2);
        Check(Motor("lastLow") == 0 && Motor("lastHigh") == 0, "turning vibration off stops the motors at once");
        Haptics.StampHit(); Haptics.Tension(1f); yield return Frames(3);
        Check(Motor("lastLow") == 0 && Motor("lastHigh") == 0, "with vibration off new pulses are skipped");
        Haptics.Enabled = true;
        Pad(); yield return Frames(3);
        yield return new WaitForSecondsRealtime(0.4f);
        Check(Selected != null && (Selected.GetComponentInChildren<Text>()?.text ?? "") == "CONTINUAR", "after stamping the focus moves to CONTINUAR");

        // Cambiar de pantalla para los motores y vuelve a elegir foco
        Call("ShowMap"); yield return Frames(3);
        Check(Motor("lastLow") == 0 && Motor("lastHigh") == 0, "changing screen stops the motors");
        Check(Selected != null, "map screen gets a selected button");

        // Ajustes › Controles: botón VIBRACIÓN: SÍ / NO generado por código y guardado en PlayerPrefs
        menus.ShowSettings(3); yield return Frames(2);
        var toggle = menus.GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == "VibrationToggle");
        Check(toggle != null && toggle.GetComponentInChildren<Text>().text == "VIBRACIÓN: SÍ", "settings show VIBRACIÓN: SÍ");
        toggle.onClick.Invoke(); yield return null;
        Check(!Haptics.Enabled && PlayerPrefs.GetInt("eldespacho_vibracion", 1) == 0 && toggle.GetComponentInChildren<Text>().text == "VIBRACIÓN: NO",
            "the toggle turns vibration off and saves it in PlayerPrefs");
        toggle.onClick.Invoke(); yield return null;
        Check(Haptics.Enabled, "the toggle turns vibration back on");
        menus.Resume(); yield return Frames(2);

        // Start abre la pausa; el foco pasa al menú; B vuelve
        Pad(buttons: GamepadButton.Start); yield return Frames(2); Pad(); yield return Frames(3);
        Check(menus.IsVisible, "Start opens the pause menu");
        Check(Selected != null && Selected.transform.IsChildOf(menus.transform), "pause menu gets the gamepad focus");
        Pad(buttons: GamepadButton.East); yield return Frames(2); Pad(); yield return Frames(3);
        Check(!menus.IsVisible, "B closes the pause menu");

        // Pasar al ratón: el mando deja de mandar, sin foco ni vibración
        if (Mouse.current != null)
        {
            InputSystem.QueueDeltaStateEvent(Mouse.current.delta, new Vector2(40, 0));
            yield return Frames(3);
            Check(!GamepadInput.UsingGamepad && Selected == null && Motor("lastLow") == 0 && Motor("lastHigh") == 0,
                "moving the mouse hands control back: no focus, motors stopped");
        }

        // Desconectar el mando a mitad de una acción
        Call("ShowOffice"); yield return Frames(2);
        Pad(rightStick: new Vector2(0, 0.5f)); yield return Frames(2);
        InputSystem.RemoveDevice(pad); pad = null; yield return Frames(2);
        Check(!GamepadInput.UsingGamepad && GamepadInput.ActivePad == null, "disconnecting the gamepad clears it safely");

        results.Add("COMPLETE: virtual gamepad in Play mode; physical rumble feel not verified.");
        File.WriteAllLines("Logs/Gamepad-validation.txt", results);
        Debug.Log("EL_DESPACHO_GAMEPAD_VALIDATION_COMPLETE " + results.Count);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
        if (pad != null) InputSystem.RemoveDevice(pad);
        Haptics.Enabled = previousVibration;
    }
}

public static class GamepadValidationMenu
{
    [UnityEditor.MenuItem("Tools/El Despacho/Validate gamepad (virtual pad, temp saves)")]
    public static void Run()
    {
        if (!UnityEditor.EditorApplication.isPlaying) { Debug.LogWarning("Enter Play mode before running gamepad validation."); return; }
        UnityEditor.EditorApplication.isPaused = false;
        if (UnityEngine.Object.FindAnyObjectByType<GamepadValidationProbe>() != null) return;
        new GameObject("Gamepad validation").AddComponent<GamepadValidationProbe>();
    }
}
#endif
