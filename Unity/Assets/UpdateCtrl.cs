using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;

public class UpdateCtrl : OpenWeb {
    public Text text;
    public string testUrl;

    // Use this for initialization
    void Start () {
        StartCoroutine (getVersion ());
        GetComponent<Button> ().onClick.AddListener (() => StartCoroutine (getVersion ()));
    }

    IEnumerator getVersion () {
        if (Lancher.Instance == null) yield break;
        var www = UnityWebRequest.Get (Lancher.Instance.versionUrl);
        yield return www.Send ();
        if (www.isError) yield break;
        string raw = www.downloadHandler.text;
        if (!raw.valid ()) yield break;
        Debug.Log (raw);

        var json = MiniJSON.Json.Deserialize (raw) as Dictionary<string, object>;
        if (json.ContainsKey (Lancher.Instance.platform)) {
            var plat = json[Lancher.Instance.platform] as Dictionary<string, object>;

            // 版本
            if (plat.ContainsKey ("version")) {
                var ver = plat["version"] as string;
                Debug.Log ("version : " + ver.color ("red"));
                if (ver != Lancher.Instance.version) {

                    // url
                    if (plat.ContainsKey ("url")) {
                        var urls = plat["url"] as List<object>;
                        if (urls != null) {
                            for (int i = 0; i < urls.Count; i++) {
                                var hww = UnityWebRequest.Head (urls[i] as string);
                                yield return hww.Send ();
                                if (!hww.isDone || hww.responseCode == 404) {
                                    Debug.Log ("Url no existed:" + urls[i].ToString ().color ("red"));
                                    continue;
                                }

                                base.url = urls[i] as string;
                                text.text = "更新\n" + ver.color ("red");
                                MessageLine.Show ("有新版本: " + ver.color ("cayn"));
                                Debug.Log ("new version url : " + base.url.color ("red"));
                                break;
                            }
                        }
                    }
                }
            }


        }
    }


    [ContextMenu ("Test URL")]
    void test () {
        var hww = UnityWebRequest.Head (testUrl as string);
        hww.Send ();
        if (!hww.isDone) return;
    }
}
