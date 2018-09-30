using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace WpfApp1 {
    public class MainViewModel : ViewModel {
        public string Name { get { return Config.name; } set { Config.name = value; RaisePropertyChanged (); Fufu.logout (); } }

        public string Password { get { return Config.password; } set { Config.password = value; RaisePropertyChanged (); Fufu.logout (); } }

        public List<Log> Logs { get { return logs; } set { logs = value; RaisePropertyChanged (); } }
        private List<Log> logs = new List<Log> ();

        public int TotalDay { get { return totalDay; } set { totalDay = value; RaisePropertyChanged (); } }
        private int totalDay;

        public int FinishDay { get { return finishDay; } set { finishDay = value; RaisePropertyChanged (); } }
        private int finishDay;

        public float Ratio { get { return ration; } set { ration = value; RaisePropertyChanged (); } }
        private float ration = 100;


        bool inwork = false;

        ITimeFilter[] filters = new ITimeFilter[] {
            new TimeFilter (),
            new TodayFilter(),
            new ReOrderFilter(),
        };

        public async Task getLogAsync ( int y, int m, int dayStart, int dayEnd, bool onlyFirst = false ) {
            if (Logs.Count == 0 || !onlyFirst) {
                getLogAsync (y, m, dayStart, dayEnd);
            }
            else {
                var log = await getLogAsync (Logs[0]._date);
                if (log != null) {
                    Logs.RemoveAt (0);
                    Logs.Insert (0, log);
                    RaisePropertyChanged ("Logs_End");
                }
            }
        }

        public void getMonthLog ( int y, int m ) {
            getLogAsync (y, m);
        }

        async Task getLogAsync ( int year, int month, int dayStart = 1, int dayEnd = 0 ) {
            if (string.IsNullOrEmpty (Name) || string.IsNullOrEmpty (Password))
                goto _end_;

            if (inwork) { return; }

            try {
                inwork = true;
                if (!await Fufu.loginAsync (Name, Password))
                    goto _end_;

                TotalDay = 0;
                FinishDay = 0;
                Logs.Clear ();
                // 开始为0，取整月
                if (dayEnd == 0) dayEnd = DateTime.DaysInMonth (year, month);
                for (int i = dayEnd; i >= dayStart; i--) {
                    var log = await getLogAsync (new DateTime (year, month, i));
                    if (log == null) continue;

                    if (Logs.Count > 0) {
                        var l = Logs[Logs.Count - 1];
                        Logs.RemoveAt (Logs.Count - 1);
                        Logs.Add (l);
                    }
                    Logs.Add (log);
                    RaisePropertyChanged ("Logs");

                    // 统计
                    if (log.week_code != 0 && log.week_code != 6 && !log._date.IsNowDay ()) {
                        TotalDay++;
                        FinishDay += (log.type_in == TimeType.Normal && log.type_out == TimeType.Normal) ? 1 : 0;
                    }
                }
                Ratio = totalDay > 0 ? (float)finishDay / (float)totalDay : 1;
                Ratio *= 100;
            }
            catch (Exception _e) {
                _e.Message.Alert ();
                RaisePropertyChanged ("Login_Failed");
                inwork = false;
            }

            _end_:
            inwork = false;
            RaisePropertyChanged ("Logs_End");
        }

        async Task<Log> getLogAsync ( DateTime dt ) {
            var log = await Fufu.getLogAsync (dt);
            if (log == null) return null;
            // 预处理
            foreach (var f in filters) f.PreFilter (log, Logs.Count > 0 ? Logs[Logs.Count - 1] : null);
            // 后处理
            foreach (var f in filters) { f.Filter (log); }
            return log;
        }

        public MainViewModel () {
            Config.onConfigUpdated += updateLogs;
            Config.onDataClear += ClearAccount;
        }

        void ClearAccount () {
            Name = Properties.Settings.Default.User = "";
            Password = Properties.Settings.Default.Password = "";

            Properties.Settings.Default.Save ();

            Fufu.logout ();
            Logs.Clear ();
        }

        void updateLogs () {
            for (int i = 0; i < Logs.Count; i++) {
                // 移除后重新添加才会刷新
                Log ls = Logs[i]; Logs.RemoveAt (i);
                Log lsn = null;
                if (i > 0) {
                    lsn = Logs[i - 1];
                    Logs.RemoveAt (i - 1);
                }
                ls.re_time_in = false;
                ls.re_time_out = false;

                foreach (var f in filters) f.PreFilter (ls, lsn);
                foreach (var f in filters) f.Filter (ls);
                if (i > 0) Logs.Insert (i - 1, lsn);
                Logs.Insert (i, ls);
            }
        }

        public override void Dispose () {
            base.Dispose ();

            Config.saveConfig ();

            foreach (var f in filters) {
                if (f is IDisposable)
                    (f as IDisposable).Dispose ();
            }

            Fufu.logout ();
        }
    }
}