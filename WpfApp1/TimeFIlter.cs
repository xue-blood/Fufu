using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1 {
    interface ITimeFilter {
        void Filter ( Log log );
    }

    public class TodayFilter : ITimeFilter {
        public void Filter ( Log log ) {
            // 过滤今天的，没打卡强制显示
            if (log.time.IsNowDay ()) {
                if (string.IsNullOrEmpty (log.time_in)) {
                    log.type_in = TimeType.Color;
                    log.color_in = "Firebrick";
                }
                if (string.IsNullOrEmpty (log.time_out)) {
                    log.type_out = TimeType.Color;
                    log.color_in = "DarkGoldenrod";
                }
            }
        }
    }

    public class TimeFilter : ITimeFilter {

        public void Filter ( Log log ) {
            if (log.time.IsNowDay ())
                return;

            if (log.week_code != 0 && log.week_code != 6) {

                TimeSpan ti, to;
                TimeSpan.TryParse (log.time_in, out ti);
                TimeSpan.TryParse (log.time_out, out to);

                log.type_in = TimeType.Normal;
                log.type_out = TimeType.Normal;
                bool adj_in = false;
                bool adj_out = false;
                foreach (var d in Config.timeTypes) {
                    if (d.invalid) {
                        if (!adj_in && string.IsNullOrEmpty (log.time_in) && d.time_in) {
                            adj_in = true;
                            log.type_in = d.type;
                            log.color_in = d.color;
                        }
                        if (!adj_out && string.IsNullOrEmpty (log.time_out) && !d.time_in) {
                            adj_out = true;
                            log.type_out = d.type;
                            log.color_out = d.color;
                        }
                    }
                    else {
                        if (!adj_in && d.time_in && ti >= d.start && ti <= d.end) {
                            adj_in = true;
                            log.type_in = d.type;
                            log.color_in = d.color;
                        }
                        if (!adj_out && !d.time_in && to >= d.start && to <= d.end) {
                            adj_out = true;
                            log.type_out = d.type;
                            log.color_out = d.color;
                        }
                    }

                    if (adj_in && adj_out)
                        break;
                }
            }
        }
    }
}
