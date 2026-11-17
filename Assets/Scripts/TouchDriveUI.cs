using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchDriveUI : MonoBehaviour
{
    DriftCarController car;
    Text speedText, scoreText;
    float score;

    void Start()
    {
        car = FindObjectOfType<DriftCarController>();
        BuildUI();
    }

    void Update()
    {
        if (!car) return;
        if (car.DriftAmount > 0.15f && car.SpeedKmh > 15f)
            score += car.DriftAmount * car.SpeedKmh * Time.deltaTime * 2f;
        if (speedText) speedText.text = Mathf.RoundToInt(car.SpeedKmh) + " KM/H";
        if (scoreText) scoreText.text = "DRIFT  " + Mathf.RoundToInt(score).ToString("000000");
    }

    void BuildUI()
    {
        var canvasGO = new GameObject("Mobile HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920,1080);
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        speedText = MakeText(canvas.transform, "0 KM/H", new Vector2(0.84f,0.9f), 34, TextAnchor.MiddleRight);
        scoreText = MakeText(canvas.transform, "DRIFT  000000", new Vector2(0.83f,0.82f), 24, TextAnchor.MiddleRight);

        MakeHoldButton(canvas.transform, "◀", new Vector2(0.10f,0.18f), new Vector2(150,150), v => car.steerInput = v, -1f);
        MakeHoldButton(canvas.transform, "▶", new Vector2(0.27f,0.18f), new Vector2(150,150), v => car.steerInput = v, 1f);
        MakeHoldButton(canvas.transform, "GAS", new Vector2(0.88f,0.20f), new Vector2(170,150), v => car.throttleInput = v, 1f);
        MakeHoldButton(canvas.transform, "BRAKE", new Vector2(0.72f,0.12f), new Vector2(140,120), v => car.brakeInput = v, 1f);
        MakeHandbrake(canvas.transform);
    }

    Text MakeText(Transform parent, string value, Vector2 anchor, int size, TextAnchor align)
    {
        var go = new GameObject("HUD Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>(); rt.anchorMin=rt.anchorMax=anchor; rt.sizeDelta=new Vector2(500,70);
        var t=go.GetComponent<Text>(); t.text=value; t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize=size; t.alignment=align; t.color=Color.white; t.fontStyle=FontStyle.Bold;
        return t;
    }

    void MakeHoldButton(Transform parent, string label, Vector2 anchor, Vector2 size, System.Action<float> set, float value)
    {
        var go = ButtonBase(parent,label,anchor,size);
        var trigger=go.AddComponent<EventTrigger>();
        AddEvent(trigger,EventTriggerType.PointerDown,_=>set(value));
        AddEvent(trigger,EventTriggerType.PointerUp,_=>set(0));
        AddEvent(trigger,EventTriggerType.PointerExit,_=>set(0));
    }

    void MakeHandbrake(Transform parent)
    {
        var go=ButtonBase(parent,"HANDBRAKE",new Vector2(0.53f,0.12f),new Vector2(180,110));
        var trigger=go.AddComponent<EventTrigger>();
        AddEvent(trigger,EventTriggerType.PointerDown,_=>car.handbrake=true);
        AddEvent(trigger,EventTriggerType.PointerUp,_=>car.handbrake=false);
        AddEvent(trigger,EventTriggerType.PointerExit,_=>car.handbrake=false);
    }

    GameObject ButtonBase(Transform parent,string label,Vector2 anchor,Vector2 size)
    {
        var go=new GameObject(label,typeof(RectTransform),typeof(Image));
        go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>(); rt.anchorMin=rt.anchorMax=anchor; rt.sizeDelta=size;
        var img=go.GetComponent<Image>(); img.color=new Color(0.05f,0.12f,0.18f,0.68f);
        var textGO=new GameObject("Label",typeof(RectTransform),typeof(Text)); textGO.transform.SetParent(go.transform,false);
        var txt=textGO.GetComponent<Text>(); txt.text=label; txt.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize=26; txt.fontStyle=FontStyle.Bold; txt.alignment=TextAnchor.MiddleCenter; txt.color=Color.white;
        var tr=textGO.GetComponent<RectTransform>(); tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one; tr.offsetMin=tr.offsetMax=Vector2.zero;
        return go;
    }

    void AddEvent(EventTrigger trigger,EventTriggerType type,UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        var entry=new EventTrigger.Entry(); entry.eventID=type; entry.callback.AddListener(action); trigger.triggers.Add(entry);
    }
}
