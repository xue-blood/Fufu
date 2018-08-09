using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1 {
    interface ITimeFilter {
        void Filter ( Log log );
    }

    public class TimeFilter : ITimeFilter {

        public void Filter ( Log log ) {

            if (log.week_code != 0 && log.week_code != 6) {

                TimeSpan ti, to;
                TimeSpan.TryParse (log.time_in, out ti);
                TimeSpan.TryParse (log.time_out, out to);

                foreach (var d in Config.timeTypes) {
                    if (d.invalid) {
                        if (string.IsNullOrEmpty (log.time_in) && d.time_in) {
                            log.type_in = d.type;
                        }
                        else if (string.IsNullOrEmpty (log.time_out) && !d.time_in) {
                            log.type_out = d.type;
                        }
                    }
                    else {
                        if (d.time_in && ti >= d.start && ti <= d.end) {
                            log.type_in = d.type;
                        }
                        else if (!d.time_in && to >= d.start && to <= d.end) {
                            log.type_out = d.type;
                        }
                    }
                }
            }
        }
    }
}
