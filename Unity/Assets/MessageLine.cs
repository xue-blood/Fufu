using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageLine : MonoBehaviour {
    public Text msg;
    CanvasRenderer render;
    static MessageLine Instance;
    static GameObject prefab;
    Queue<Callback> onEnd = new Queue<Callback> ();

    private void Awake () {
        if (Instance != null && Instance != this) {
            DestroyObject (gameObject);
            return;
        }
        Instance = this;
        render = GetComponent<CanvasRenderer> ();
    }
    public static void Show ( string msg, Callback onend = null ) {
        if (Instance != null) Instance.doShow (msg, onend);
        if (prefab == null) prefab = Resources.Load ("Ctrl/MessageLine") as GameObject;
        Extension.AddPrefab<MessageLine> (null, GameObject.Find ("MsgRoot"), prefab).doShow (msg, onend);
    }

    public static void Init () {
        if (prefab == null) prefab = Resources.Load ("Ctrl/MessageLine") as GameObject;
    }

    bool inshowing = false;
    private void doShow ( string msg, Callback onend ) {
        this.msg.text = msg;
        this.onEnd.Enqueue (onend);
        if (inshowing) return;
        iTween.ValueTo (gameObject, iTween.Hash ("from", 0.5f, "to", 1f, "onupdatetarget", gameObject, "onupdate", "doUpdate", "easetype", iTween.EaseType.linear, "time", 0.25f));
        iTween.ValueTo (gameObject, iTween.Hash ("delay", 1.5f, "from", 1f, "to", 0.2f, "onupdatetarget", gameObject, "onupdate", "doUpdate", "easetype", iTween.EaseType.linear, "oncompletetarget", gameObject, "oncomplete", "doEnd", "time", 0.25f));
        inshowing = true;
    }

    void doUpdate ( float value ) {
        msg.canvasRenderer.SetAlpha (value);
        render.SetAlpha (value);
    }

    void doEnd () {
        msg.canvasRenderer.SetAlpha (0);
        render.SetAlpha (0);
        inshowing = false;
        while (onEnd.Count > 0) {
            var c = onEnd.Dequeue ();
            if (c != null) c.Invoke ();
        }
    }
}
