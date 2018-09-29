using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent (typeof (Button))]
public class OpenWeb : MonoBehaviour {
    public string url;

    private void Awake () {
        GetComponent<Button> ().onClick.AddListener (() => StartCoroutine (openUrl (url)));
    }

    IEnumerator openUrl ( string url ) {
        if (string.IsNullOrEmpty (url)) yield break;

        // test url
        var hww = UnityWebRequest.Head (url);
        yield return hww.Send ();
        if (!hww.isDone || hww.responseCode == 404) yield break;

        Debug.Log ("Open Url ".color ("red") + url.color ("cyan"));
        Application.OpenURL (url);
    }
}
