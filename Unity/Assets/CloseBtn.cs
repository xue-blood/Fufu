using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (Button))]
public class CloseBtn : MonoBehaviour {

    public MonoBehaviour window;
    public bool exitGame = false;
    Button btn;

    private void Awake () {
        btn = GetComponent<Button> ();
    }

    bool exitnow = false;
    private void Start () {
        btn.onClick.AddListener (close);
    }

    private void Update () {
        if (Input.GetKey (KeyCode.Escape)) close ();
    }

    void close () {
        if (window) {
            window.FinishWindow ();
        }
        else if (exitGame) {
            if (exitnow) doclose ();
            exitnow = true;
            MessageLine.Show ("再点一次" + "退出".color ("red"), () => exitnow = false);
        }
    }

    void doclose () {
        if (Application.platform == RuntimePlatform.Android) {
            AndroidJavaObject activity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject> ("currentActivity");
            activity.Call<bool> ("moveTaskToBack", true);
        }
        else {
            Application.Quit ();
        }
    }
}
