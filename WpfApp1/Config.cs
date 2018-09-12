using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfApp1 {

    public class IConfig {
        protected void Update () { Config.UpdateConfig (); }
        public virtual bool Parse ( string cfg ) { return false; }
        public virtual string Stringfy () { return ""; }
    }

    public class OutTimeData : IConfig {
        public TimeSpan start { get; set; }
        public TimeSpan end { get; set; }
        public TimeSpan renew { get; set; }
        public string color { get { return _color; } set { _color = value; Update (); } }
        private string _color;

        const string str_fmt = @"d\.h\:mm";
        const string str_fmt2 = @"h\:mm";

        public override string ToString () {
            var s = (start.Days > 0 ? "次日" : "") + start.ToString (str_fmt2) + "-" + (end.Days > 0 ? "次日" : "") + end.ToString (str_fmt2);
            if (s.Length <= 12) s += "\t";
            return s + "\t调休到" + renew.ToString (str_fmt2);
        }

        public override bool Parse ( string cfg ) {
            if (!string.IsNullOrEmpty (cfg)) {
                var strs = cfg.Split (',');
                if (strs.Length == 4) {
                    var t = TimeSpan.Zero;
                    TimeSpan.TryParse (strs[0], out t); start = t;
                    TimeSpan.TryParse (strs[1], out t); end = t;
                    TimeSpan.TryParse (strs[2], out t); renew = t;
                    color = strs[3];
                    return true;
                }
            }
            return false;
        }

        public override string Stringfy () {
            return start.ToString (str_fmt) + "," + end.ToString (str_fmt) + "," + renew.ToString (str_fmt) + "," + color.ToString ();
        }
    }
    public class TimeData : IConfig {
        public TimeSpan start { get; set; }
        public TimeSpan end { get; set; }
        public TimeType type { get { return _type; } set { _type = value; Update (); } }
        private TimeType _type; // 更改设置 刷新

        public string color { get { return _color; } set { _color = value; Update (); } }
        private string _color;

        public string desc { get; set; }

        public bool time_in { get; set; } // 标注上班还是下班

        public bool invalid { get { return start == TimeSpan.Zero && end == TimeSpan.Zero; } }

        const string str_fmt = @"h\:mm";

        public override string ToString () {
            return string.IsNullOrEmpty (desc) ? start.ToString (str_fmt) + "-" + end.ToString (str_fmt) : desc;
        }

        public override bool Parse ( string cfg ) {
            if (!string.IsNullOrEmpty (cfg)) {
                var strs = cfg.Split (',');
                if (strs.Length == 6) {
                    var t = TimeSpan.Zero;
                    TimeSpan.TryParse (strs[0], out t); start = t;
                    TimeSpan.TryParse (strs[1], out t); end = t;
                    type = (TimeType)strs[2].ToInt ();
                    bool b; bool.TryParse (strs[3], out b); time_in = b;
                    color = strs[4];
                    desc = strs[5];

                    return true;
                }
            }
            return false;
        }

        public override string Stringfy () {
            return start.ToString (str_fmt) + "," + end.ToString (str_fmt) + "," + ((int)type).ToString () + "," + time_in.ToString () + "," + color.ToString () + "," + desc;
        }
    }

    public delegate void ConfigUpdate ();

    public static class Config {
        public static ObservableCollection<TimeData> timeTypes = new ObservableCollection<TimeData> ();

        public static ObservableCollection<OutTimeData> timeReorder = new ObservableCollection<OutTimeData> ();

        public static event ConfigUpdate onConfigUpdated;

        public static event ConfigUpdate onDataClear;


        public static bool pauseUpdate = false;

        public static void UpdateConfig () {
            if (onConfigUpdated != null && !pauseUpdate)
                onConfigUpdated ();
        }

        public static void ClearData () {
            if (onDataClear != null)
                onDataClear ();
        }

        public static void loadConfig ( string _cfg = null, string _reorder = null ) {
            pauseUpdate = true;
            Config.timeTypes.Clear ();
            var cfg = _cfg == null ? Properties.Settings.Default.TimeTypes.Split ('|') : _cfg.Split ('|');
            foreach (var s in cfg) {
                var d = new TimeData ();
                if (d.Parse (s)) Config.timeTypes.Add (d);
            }

            timeReorder.Clear ();
            var reorder = _reorder == null ? Properties.Settings.Default.TimeReourde.Split ('|') : _reorder.Split ('|');
            foreach (var s in reorder) {
                var ss = s.Split (',');
                if (ss.Length == 4) {
                    var d = new OutTimeData ();
                    if (d.Parse (s)) timeReorder.Add (d);
                }
            }

            pauseUpdate = false;
            UpdateConfig ();
        }

        public static void saveConfig () {
            var sbd = new StringBuilder ();
            for (int i = 0; i < Config.timeTypes.Count (); i++) {
                sbd.Append (Config.timeTypes[i].Stringfy () + "|");
            }
            Properties.Settings.Default.TimeTypes = sbd.Length > 1 ? sbd.ToString (0, sbd.Length - 1) : "";

            sbd.Clear ();
            foreach (var r in timeReorder) {
                sbd.Append (r.Stringfy () + "|");
            }
            Properties.Settings.Default.TimeReourde = sbd.Length > 1 ? sbd.ToString (0, sbd.Length - 1) : "";
        }
    }
}
