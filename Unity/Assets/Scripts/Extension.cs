using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniJSON;
using UnityEngine;

public delegate void Callback ();
public delegate void Callback<T> ( T t );
public delegate void Callback<T1, T2> ( T1 t1, T2 t2 );


public static class Extension {

    public static Task<Dictionary<string, object>> GetJsonAsync ( this string url, IEnumerable<KeyValuePair<string, string>> keys ) {
        var _url = url + keys.ToUrl ();
        Dictionary<string, object> json = null;
        var t = new Task<Dictionary<string, object>> (() => {
            var raw = WebManager.Instance.Get (_url);
            if (!string.IsNullOrEmpty (raw)) {
                raw = raw.Substring (raw.IndexOf ('{'), raw.LastIndexOf ('}') - raw.IndexOf ('{') + 1);
                json = (Dictionary<string, object>)Json.Deserialize (raw);
            }
            return json;
        });
        t.Start ();
        return t;
    }

    public static string ToUrl ( this IEnumerable<KeyValuePair<string, string>> keys ) {
        string url = "";
        foreach (var key in keys) {
            url += (url.Length == 0 ? "?" : "&") + key.Key + "=" + key.Value;
        }
        return url;
    }


    public static T Get<T> ( this Dictionary<string, object> dic, string key ) {
        if (!dic.ContainsKey (key))
            return default (T);
        if (dic[key].GetType () != typeof (T)) {
            Debug.LogError ("类型错误： " + key + typeof (T).ToString ());
            return default (T);
        }
        return (T)dic[key];
    }
    public static double ToUnixTimestamp ( this DateTime dateTime ) {
        return (TimeZoneInfo.ConvertTimeToUtc (dateTime) -
               new DateTime (1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
    }


    public static int ToInt ( this string s ) {
        int r = 0;
        int.TryParse (s, out r);
        return r;
    }

    public static bool valid ( this string s ) { return !String.IsNullOrEmpty (s); }
    public static bool IsNowDay ( this DateTime date ) { return date.IsSameDay (DateTime.Now); }
    public static bool IsNowMonth ( this DateTime date ) { return date.IsSameMonth (DateTime.Now); }
    public static bool IsSameDay ( this DateTime date, DateTime other ) { return date.Year == other.Year && date.DayOfYear == other.DayOfYear; }
    public static bool IsSameMonth ( this DateTime date, DateTime other ) { return date.Year == other.Year && date.Month == other.Month; }

    public static string ToString ( this TimeSpan ts, string fmt ) {
        return ts.ToString ();
    }

    public static void Alert ( this string msg ) {
        Debug.LogError (msg);
    }
    /// <summary>
    /// 获取最后一个字符串
    /// </summary>
    /// <param name="strs"></param>
    /// <returns></returns>
    public static string last ( this string[] strs ) {
        for (int i = strs.Length - 1; i > 0; i--)
            if (strs[i].valid ())
                return strs[i];
        return strs[0];
    }

    public static T AddPrefab<T> ( this MonoBehaviour mono, GameObject parent, GameObject prefab ) where T : Component {
        if (prefab == null) return default (T);
        var type = typeof (T).Name;
        var go = GameObject.Instantiate (prefab, parent.transform);
        return (T)go.GetComponent (type);
    }

    public static GameObject LoadPrefab ( this MonoBehaviour mono, string path ) {
        var prefab = Resources.Load (path) as GameObject;
        if (prefab == null) prefab = Resources.Load (path + "/" + path.Split ('/').last ()) as GameObject;
        if (prefab == null) Alert ("加载 Prefab 失败：" + path);
        return prefab;
    }

    public static void ClearChild ( this GameObject obj, params GameObject[] withOut ) {
        int count = obj.transform.childCount;
        for (int i = 0; i < count; i++) {

            if (obj.transform.GetChild (i) == null)
                continue;

            GameObject child = obj.transform.GetChild (i).gameObject;
            for (int j = 0; withOut != null && j < withOut.Length; j++) {
                if (child == withOut[j])
                    continue;
            }
            GameObject.Destroy (child);
        }
    }

    public static void UpdateGrid<T> ( this MonoBehaviour mono,
        ref List<T> list, int count, GameObject grid, GameObject prefab, Action<int> each )
        where T : MonoBehaviour {
        if (each == null || grid == null || prefab == null) { return; }
        if (list == null) { list = new List<T> (); }
        int max = Mathf.Max (list.Count, count);
        for (int i = 0; i < max; i++) {
            if (i >= count) { list[i].gameObject.SetActive (false); continue; }// 隐藏多余的
            if (i >= list.Count) { list.Add (AddPrefab<T> (mono, grid.gameObject, prefab)); }
            list[i].gameObject.SetActive (true);
            each (i);
        }
    }

    public static void OpenWindow<T> ( this MonoBehaviour mono, Callback<T> oninit = null, bool dialog = false ) where T : MonoBehaviour {
        if (Lancher.Instance != null) Lancher.Instance.OpenWindow (oninit, dialog);
    }
    public static void OpenDialogWindow<T> ( this MonoBehaviour mono, Callback<T> oninit = null ) where T : MonoBehaviour {
        if (Lancher.Instance != null) Lancher.Instance.OpenWindow (oninit, true);
    }

    public static void FinishWindow ( this MonoBehaviour mono ) {
        if (Lancher.Instance != null) Lancher.Instance.FinishWindow (mono);

    }
    public static void HideWindow ( this MonoBehaviour mono ) {
        if (Lancher.Instance != null) Lancher.Instance.HideWindow (mono);
    }

    [System.Diagnostics.DebuggerHidden]
    public static string format ( this string str, params object[] param ) { return (param != null && param.Length > 0) ? string.Format (str, param) : str; }
    [System.Diagnostics.DebuggerHidden]
    public static string color ( this string val, string color, params string[] args ) { return string.Format ("<color={0}>{1}</color>", color, val.format (args)); }
}

