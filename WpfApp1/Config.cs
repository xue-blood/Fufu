using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1 {
    public class TimeData {
        public TimeSpan start { get; set; }
        public TimeSpan end { get; set; }
        public TimeType type { get; set; }
        public bool time_in { get; set; }

        public bool invalid { get { return start == TimeSpan.Zero && end == TimeSpan.Zero; } }

        const string str_fmt = @"h\:mm";

        public override string ToString () {
            if (invalid)
                return time_in ? "上班未打卡" : "下班未打卡";
            else
                return start.ToString (str_fmt) + "-" + end.ToString (str_fmt);
        }

        public bool Parse ( string cfg ) {
            if (!string.IsNullOrEmpty (cfg)) {
                var strs = cfg.Split (',');
                if (strs.Length == 4) {
                    var t = TimeSpan.Zero;
                    TimeSpan.TryParse (strs[0], out t);
                    start = t;
                    TimeSpan.TryParse (strs[1], out t);
                    end = t;
                    type = (TimeType)strs[2].ToInt ();
                    bool b;
                    bool.TryParse (strs[3], out b);
                    time_in = b;

                    return true;
                }
            }
            return false;
        }

        public string UnParse () {
            return start.ToString (str_fmt) + "," + end.ToString (str_fmt) + ","+ ((int)type).ToString () + ","+ time_in.ToString ();
        }
    }

    public static class Config {
        public static ObservableCollection<TimeData> timeTypes = new ObservableCollection<TimeData> ();

    }
}
