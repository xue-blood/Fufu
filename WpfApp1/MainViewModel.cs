using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1 {
    public class MainViewModel : ViewModel, IDisposable {
        public string Name { get { return name; } set { name = value; RaisePropertyChanged (); } }
        private string name;

        public string Password { get { return password; } set { password = value; RaisePropertyChanged (); } }
        private string password;

        public ObservableCollection<Log> Logs { get { return logs; } set { logs = value; RaisePropertyChanged (); } }
        private ObservableCollection<Log> logs = new ObservableCollection<Log> ();

        public Command GetLogCmd { get { return getLogCmd ?? (getLogCmd = new Command { CanExecuteDelegate = _ => !inwork, ExecuteDelegate = async _ => await getThisMouthAsync () }); } }
        private Command getLogCmd;

        public Command ClearCmd { get { return clearCmd ?? (clearCmd = new Command { ExecuteDelegate = _ => ClearAccount () }); } }
        private Command clearCmd;

        public Command SettingCloseCmd { get { return settingCloseCmd ?? (settingCloseCmd = new Command { ExecuteDelegate = _ => updateLogs () }); } }
        private Command settingCloseCmd;

        public DateTime SelectDate { get { return selectDate; } set { selectDate = value; RaisePropertyChanged (); getSelectLogAsync (); } }
        private DateTime selectDate = DateTime.Now;

        public int TotalDay { get { return totalDay; } set { totalDay = value; RaisePropertyChanged (); } }
        private int totalDay;

        public int FinishDay { get { return finishDay; } set { finishDay = value; RaisePropertyChanged (); } }
        private int finishDay;

        public float Ratio { get { return ration; } set { ration = value; RaisePropertyChanged (); } }
        private float ration = 100;

        public ObservableCollection<TimeData> TimeTypes { get { return Config.timeTypes; } set { Config.timeTypes = value; RaisePropertyChanged (); } }

        public IEnumerable<TimeType> TypeList { get { return Enum.GetValues (typeof (TimeType)).Cast<TimeType> (); } }


        bool inwork = false;
        async Task getThisMouthAsync () {
            await getLogAsync (DateTime.Now);
        }

        ITimeFilter[] filters = new[] {
            new TimeFilter ()
        };

        async Task getLogAsync ( DateTime date ) {
            if (string.IsNullOrEmpty (Name) || string.IsNullOrEmpty (Password))
                return;

            if (inwork) { return; }

            try {
                inwork = true;
                if (!await Fufu.loginAsync (Name, Password))
                    goto _end_;

                TotalDay = 0;
                FinishDay = 0;
                Logs.Clear ();
                for (int i = date.Day; i > 0; i--) {
                    var d = new DateTime (date.Year, date.Month, i);
                    var log = await Fufu.getLogAsync (d);
                    if (log == null)
                        continue;

                    Logs.Add (log);

                    // 数据处理
                    foreach (var f in filters) {
                        f.Filter (log);
                    }

                    // 统计
                    if (log.week_code != 0 && log.week_code != 6 && !d.IsNowDay ()) {
                        TotalDay++;
                        FinishDay += (log.type_in == TimeType.Normal && log.type_out == TimeType.Normal) ? 1 : 0;
                    }
                }
                Ratio = totalDay > 0 ? (float)finishDay / (float)totalDay : 1;
                Ratio *= 100;
            }
            catch (Exception _e) {
                MessageBox.Show (_e.Message);
                inwork = false;
            }

            _end_:
            inwork = false;
        }

        async Task getSelectLogAsync () {
            if (SelectDate.Year == DateTime.Now.Year && SelectDate.Month >= DateTime.Now.Month) {
                await getLogAsync (DateTime.Now);
            }
            else {
                var date = new DateTime (SelectDate.Year, SelectDate.Month, DateTime.DaysInMonth (SelectDate.Year, SelectDate.Month));
                await getLogAsync (date);
            }
        }

        public MainViewModel () {
            Name = Properties.Settings.Default.Name;
            Password = Security.Decrypt (Properties.Settings.Default.Password, "p@ssw0rd");

            var cfg = Properties.Settings.Default.TimeTypes.Split ('|');
            foreach (var s in cfg) {
                var d = new TimeData ();
                if (d.Parse (s))
                    TimeTypes.Add (d);
            }

            if (!string.IsNullOrEmpty (Name) && !string.IsNullOrEmpty (Password)) {
                getLogAsync (DateTime.Now);
            }
        }

        void ClearAccount () {
            Name = Properties.Settings.Default.Name = "";
            Password = Properties.Settings.Default.Password = "";

            Properties.Settings.Default.Save ();

            Fufu.logout ();
            Logs.Clear ();
        }

        void updateLogs () {
            for (int i = 0; i<Logs.Count; i++) {
                // 移除后重新添加才会刷新
                var ls = Logs[i];
                Logs.RemoveAt (i);
                foreach (var f in filters)
                    f.Filter (ls);
                Logs.Insert (i, ls);
            }


        }

        public void Dispose () {

            Properties.Settings.Default.Name = Name;
            Properties.Settings.Default.Password = Security.Encrypt (Password, "p@ssw0rd");

            var sbd = new StringBuilder ();
            for (int i = 0; i<TimeTypes.Count (); i++) {
                sbd.Append (TimeTypes[i].UnParse () + "|");
            }
            Properties.Settings.Default.TimeTypes = sbd.Length > 1 ? sbd.ToString (0, sbd.Length -1) : "";

            Properties.Settings.Default.Save ();

            foreach (var f in filters) {
                if (f is IDisposable)
                    (f as IDisposable).Dispose ();
            }

            Fufu.logout ();
        }
    }


}
