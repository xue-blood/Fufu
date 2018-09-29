using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebManager : MonoBehaviour {

    public static WebManager Instance;

    private void Awake () {
        if (Instance != null && Instance != this) {
            DestroyObject (gameObject);
            return;
        }
        DontDestroyOnLoad (gameObject);
        Instance = this;
    }

    TaskCompletionSource<string> tcs = null;
    string request = "";
    public string Get ( string url ) {
        tcs = new TaskCompletionSource<string> ();
        request = url;
        tcs.Task.Wait ();
        return tcs.Task.Result;
    }

    IEnumerator _get ( string url ) {
        request = "";
        Debug.Log (url);
        var www = UnityWebRequest.Get (url);
        yield return www.Send ();
        var res = www.isDone ? www.downloadHandler.text.Trim () : "";
        Debug.Log (res);
        tcs.SetResult (res);
    }

    private void Update () {
        if (request != "") StartCoroutine (_get (request));
    }
}
