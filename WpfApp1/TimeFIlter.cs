using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1 {
    public class ITimeFilter {
        public virtual void PreFilter ( Log today, Log next ) { } // 预处理
        public virtual void Filter ( Log log ) { } // 后处理
    }

    public class TodayFilter : ITimeFilter {
        public override void Filter ( Log log ) {
            // 过滤今天的，没打卡强制显示
            if (log._date.IsNowDay ()) {
                if (string.IsNullOrEmpty (log.time_in)) {
                    log.type_in = TimeType.Color;
                    log.color_in = "Firebrick";
                    log.re_time_in = true;
                }
                if (string.IsNullOrEmpty (log.time_out)) {
                    log.re_time_out = true;
                    log.type_out = TimeType.Color;
                    log.color_out = "DarkGoldenrod";
                }
            }
        }

        public override void PreFilter ( Log today, Log last ) {
            if (today.records != null) {
                TimeSpan tout;
                TimeSpan.TryParse (Properties.Settings.Default.OutTime, out tout);
                if (today.records.Count > 0 && today.records[0].TotalHours > 0) {
                    if (today.records[0] < tout)
                        today.time_in = today.records[0].ToString (@"hh\:mm");
                    else
                        today.time_out = today.records[0].ToString (@"hh\:mm");
                }
                if (today.records.Count > 1 && today.records[today.records.Count - 1].TotalHours > 0) today.time_out = today.records[today.records.Count - 1].ToString (@"hh\:mm");
            }
        }
    }

    public class TimeFilter : ITimeFilter {

        public override void Filter ( Log log ) {
            if (log.week_code != 0 && log.week_code != 6) {

                TimeSpan ti, to;
                TimeSpan.TryParse (log.time_in, out ti);
                TimeSpan.TryParse (log.time_out, out to);

                bool adj_in = false;
                bool adj_out = false;
                foreach (var d in Config.timeTypes) {
                    if (d.invalid) {
                        if (!log.re_time_in && !adj_in && string.IsNullOrEmpty (log.time_in) && d.time_in) {
                            adj_in = true;
                            log.type_in = d.type;
                            log.color_in = d.color;
                        }
                        if (!log.re_time_out && !adj_out && string.IsNullOrEmpty (log.time_out) && !d.time_in) {
                            adj_out = true;
                            log.type_out = d.type;
                            log.color_out = d.color;
                        }
                    }
                    else {
                        if (!log.re_time_in && !adj_in && d.time_in && ti >= d.start && ti <= d.end) {
                            adj_in = true;
                            log.type_in = d.type;
                            log.color_in = d.color;
                        }
                        if (!log.re_time_out && !adj_out && !d.time_in && to >= d.start && to <= d.end) {
                            adj_out = true;
                            log.type_out = d.type;
                            log.color_out = d.color;
                        }
                    }

                    if (adj_in && adj_out)
                        break;
                }

                if (log.color_in == null) log.color_in = "white";
                if (log.color_out == null) log.color_out = "white";
            }
        }
    }

    public class ReOrderFilter : TimeFilter {

        public override void PreFilter ( Log today, Log next ) {
            if (!Properties.Settings.Default.enableOutTime) return;
            if (today.records == null || today.records.Count == 0 || !today.time_in.valid () || !today.time_out.valid ()) return;

            // 当天时间
            for (int i = today.records.Count - 1; i >= 0; i--) {
                foreach (var tc in Config.timeReorder) {
                    // 加班时间
                    if (today.records[i] > tc.start && today.records[i] < tc.end) {
                        today.re_time_out = true;
                        today.color_out = tc.color;
                        today.type_out = TimeType.Normal;
                        today.time_out = today.time_out + "(加班)";

                        // 调休
                        if (next == null || next.records == null) goto _reorder_end_;
                        for (int j = 0; j < next.records.Count; j++) {
                            if (next.records[j] < tc.renew && next.type_in != TimeType.Normal) {

                                next.re_time_in = true;
                                next.color_in = tc.color;
                                next.type_in = TimeType.Normal;
                                next.time_in = next.records[j].ToString (@"hh\:mm") + "(调休)";
                                break;
                            }
                        }

                        goto _reorder_end_;
                    }
                }
            }

            // 加班到第二天
            if (today.time_out.valid () || next == null || next.records == null) goto _reorder_end_;
            for (int i = 0; i < next.records.Count; i++) {
                var nxt = new TimeSpan (1, next.records[i].Hours, next.records[i].Minutes, next.records[i].Seconds);
                foreach (var tc in Config.timeReorder) {
                    if (nxt > tc.start && nxt < tc.end) {
                        // 下班
                        today.re_time_out = true;
                        today.color_out = tc.color;
                        today.type_out = TimeType.Normal;
                        today.time_out = nxt.ToString (@"hh\:mm") + "(次日)";

                        // 新的上班时间
                        if (i < next.records.Count - 1) {
                            next.type_in = TimeType.Normal;
                            next.time_in = next.records[i + 1].ToString (@"hh\:mm") + "(次日)";
                        }
                        base.Filter (next);

                        // 是否算调休
                        if (next.type_in != TimeType.Normal && next.records[i + 1] < tc.renew) {
                            next.type_in = TimeType.Normal;
                            next.color_in = tc.color;
                            next.re_time_in = true;
                        }

                        goto _reorder_end_;
                    }
                }
            }
            _reorder_end_:
            return;
        }
    }
}
