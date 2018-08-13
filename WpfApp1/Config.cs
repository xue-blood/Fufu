using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfApp1 {
    public class TimeData {
        public TimeSpan start { get; set; }
        public TimeSpan end { get; set; }
        public TimeType type { get { return _type; } set { _type = value; Config.UpdateConfig (); } }
        private TimeType _type; // 更改设置 刷新

        public string color { get { return _color; } set { _color = value; Config.UpdateConfig (); } }
        private string _color;

        public string desc { get; set; }

        public bool time_in { get; set; }

        public bool invalid { get { return start == TimeSpan.Zero && end == TimeSpan.Zero; } }

        const string str_fmt = @"h\:mm";

        public override string ToString () {
            return string.IsNullOrEmpty (desc) ? start.ToString (str_fmt) + "-" + end.ToString (str_fmt) : desc;
        }

        public bool Parse ( string cfg ) {
            if (!string.IsNullOrEmpty (cfg)) {
                var strs = cfg.Split (',');
                if (strs.Length == 6) {
                    var t = TimeSpan.Zero;
                    TimeSpan.TryParse (strs[0], out t);
                    start = t;
                    TimeSpan.TryParse (strs[1], out t);
                    end = t;
                    type = (TimeType)strs[2].ToInt ();
                    bool b;
                    bool.TryParse (strs[3], out b);
                    time_in = b;
                    color = strs[4];
                    desc = strs[5];

                    return true;
                }
            }
            return false;
        }

        public string UnParse () {
            return start.ToString (str_fmt) + "," + end.ToString (str_fmt) + "," + ((int)type).ToString () + "," + time_in.ToString () + "," + color.ToString () + "," + desc;
        }
    }

    public delegate void ConfigUpdate ();

    public static class Config {
        public static ObservableCollection<TimeData> timeTypes = new ObservableCollection<TimeData> ();

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

        public static void loadConfig ( string _cfg = null ) {
            pauseUpdate = true;
            Config.timeTypes.Clear ();
            var cfg = _cfg == null ? Properties.Settings.Default.TimeTypes.Split ('|') : _cfg.Split ('|');
            foreach (var s in cfg) {
                var d = new TimeData ();
                if (d.Parse (s))
                    Config.timeTypes.Add (d);
            }
            pauseUpdate = false;
            UpdateConfig ();
        }

        public static void saveConfig () {
            var sbd = new StringBuilder ();
            for (int i = 0; i < Config.timeTypes.Count (); i++) {
                sbd.Append (Config.timeTypes[i].UnParse () + "|");
            }
            Properties.Settings.Default.TimeTypes = sbd.Length > 1 ? sbd.ToString (0, sbd.Length - 1) : "";
        }
    }
}
