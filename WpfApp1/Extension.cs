using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MiniJSON;
using System.IO;
using System.Windows;

namespace WpfApp1 {
    public static class Extension {

        public static async Task<Dictionary<string, object>> PostJsonAsync ( this HttpClient web, string url, IEnumerable<KeyValuePair<string, string>> keys ) {
            Debug.Print(url + keys.ToUrl());
            Dictionary<string, object> json = null;
            var res = await web.PostAsync (url, new FormUrlEncodedContent (keys));
            using (var reader = new StreamReader (await res.Content.ReadAsStreamAsync ())) {
                var raw = await reader.ReadToEndAsync ();
                Debug.Print (raw);
                if (!string.IsNullOrEmpty (raw)) {
                    raw = raw.Substring (raw.IndexOf ('{'), raw.LastIndexOf ('}') - raw.IndexOf ('{') + 1);
                    json = (Dictionary<string, object>)Json.Deserialize (raw);
                }
            }
            return json;
        }

        public static async Task<Dictionary<string, object>> GetJsonAsync ( this HttpClient web, string url, IEnumerable<KeyValuePair<string, string>> keys ) {
            Dictionary<string, object> json = null;

            Debug.Print(url + keys.ToUrl());
            var res = await web.GetAsync (url + keys.ToUrl ());
            using (var reader = new StreamReader (await res.Content.ReadAsStreamAsync ())) {
                var raw = await reader.ReadToEndAsync ();
                Debug.Print (raw);
                if (!string.IsNullOrEmpty (raw)) {
                    raw = raw.Substring (raw.IndexOf ('{'), raw.LastIndexOf ('}') - raw.IndexOf ('{') + 1);
                    json = (Dictionary<string, object>)Json.Deserialize (raw);
                }
            }
            return json;
        }


        public static string ToUrl ( this IEnumerable<KeyValuePair<string, string>> keys ) {
            string url = "";
            foreach (var key in keys) {
                url += (url.Length == 0 ? "?" : "&") + key.Key + "=" +key.Value;
            }
            return url;
        }


        public static T Get<T> ( this Dictionary<string, object> dic, string key ) {
            if (!dic.ContainsKey (key))
                return default (T);
            if (dic[key].GetType () != typeof (T)) {
                MessageBox.Show ("类型错误： "+ key + typeof (T).ToString ());
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

        public static bool IsSameDay ( this DateTime date, DateTime other ) { return date.Year == other.Year && date.DayOfYear == other.DayOfYear; }
        public static bool IsNowDay ( this DateTime date ) { return date.IsSameDay (DateTime.Now); }
    }
}
