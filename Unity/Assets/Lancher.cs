using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lancher : MonoBehaviour {
    public static Lancher Instance;
    public GameObject winRoot;
    public GameObject dialogRoot;
    public GameObject msgRoot;

    [Header ("Version")]
    public string version;
    public string versionUrl;
    public string platform;

    public GameObject windowBase;
    public Stack<MonoBehaviour> windowStack = new Stack<MonoBehaviour> ();

    private void Awake () {
        winRoot.ClearChild ();
        dialogRoot.ClearChild ();
        msgRoot.ClearChild ();

        if (Instance != null && Instance != this) {
            DestroyObject (gameObject);
            return;
        }
        DontDestroyOnLoad (gameObject);
        gameObject.AddComponent<WebManager> ();

        Instance = this;
    }

    private void Start () {
        Config.loadConfig ();
        OpenWindow<MainWindow> ();

        if (!Config.name.valid () || !Config.password.valid ()) {
            OpenDialogWindow<SettingWindow> ();
            MessageLine.Show ("请输出帐号和密码");
        }
    }

    public void OpenWindow<T> ( Callback<T> oninit = null, bool dialog = false ) where T : MonoBehaviour {
        var prefab = this.LoadPrefab ("UI/" + typeof (T).Name);
        if (prefab == null) return;

        var wb = GameObject.Instantiate (Lancher.Instance.windowBase, dialog ? dialogRoot.transform : winRoot.transform);
        wb.name = typeof (T).Name;

        var go = GameObject.Instantiate (prefab, wb.transform);
        T t = go.GetComponent<T> ();
        if (t != null && oninit != null) oninit (t);

        if (!dialog) {
            if (windowStack.Count > 0) HideWindow (windowStack.Peek ());
        }
        windowStack.Push (t);
    }
    public void OpenDialogWindow<T> ( Callback<T> oninit = null ) where T : MonoBehaviour {
        OpenWindow<T> (oninit, true);
    }

    public void FinishWindow ( MonoBehaviour win ) {
        if (windowStack.Count == 0) {
            OpenWindow<MainWindow> ();
        }
        else {
            windowStack.Pop ();
            windowStack.Peek ().transform.parent.gameObject.SetActive (true);
        }
        GameObject.DestroyObject (win.transform.parent.gameObject);
    }
    public void HideWindow ( MonoBehaviour win ) {
        if (windowStack.Count == 0) {
            OpenWindow<MainWindow> ();
        }
        windowStack.Peek ().transform.parent.gameObject.SetActive (false);
    }
}
