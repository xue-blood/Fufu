using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1 {
    public class MainViewModel : ViewModel {
        public string Name { get { return name; } set { name = value; RaisePropertyChanged(); Fufu.logout(); } }
        private string name;

        public string Password { get { return password; } set { password = value; RaisePropertyChanged(); Fufu.logout(); } }
        private string password;

        public ObservableCollection<Log> Logs { get { return logs; } set { logs = value; RaisePropertyChanged(); } }
        private ObservableCollection<Log> logs = new ObservableCollection<Log>();

        public Command GetLogCmd { get { return getLogCmd ?? (getLogCmd = new Command { CanExecuteDelegate = _ => !inwork, ExecuteDelegate = async _ => await getThisMouthAsync() }); } }
        private Command getLogCmd;

        public Command SettingCmd { get { return settingCmd ?? (settingCmd = new Command { CanExecuteDelegate = _ => Get<SettingViewModel>() == null, ExecuteDelegate = _ => new Setting().Show() }); } }
        private Command settingCmd;


        public DateTime SelectDate { get { return selectDate; } set { selectDate = value; RaisePropertyChanged(); getSelectLogAsync(); } }
        private DateTime selectDate = DateTime.Now;

        public int TotalDay { get { return totalDay; } set { totalDay = value; RaisePropertyChanged(); } }
        private int totalDay;

        public int FinishDay { get { return finishDay; } set { finishDay = value; RaisePropertyChanged(); } }
        private int finishDay;

        public float Ratio { get { return ration; } set { ration = value; RaisePropertyChanged(); } }
        private float ration = 100;


        bool inwork = false;
        async Task getThisMouthAsync() {
            await getLogAsync(DateTime.Now);
        }

        ITimeFilter[] filters = new ITimeFilter[] {
            new TimeFilter (),
            new TodayFilter(),
            new ReOrderFilter(),
        };

        async Task getLogAsync(DateTime date) {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Password))
                return;

            if (inwork) { return; }

            try {
                inwork = true;
                if (!await Fufu.loginAsync(Name, Password))
                    goto _end_;

                TotalDay = 0;
                FinishDay = 0;
                Logs.Clear();
                for (int i = date.Day; i > 0; i--) {
                    var d = new DateTime(date.Year, date.Month, i);
                    var log = await Fufu.getLogAsync(d);
                    if (log == null)
                        continue;

                    // 预处理
                    foreach (var f in filters) f.PreFilter (log, Logs.Count > 0 ? Logs[Logs.Count - 1] : null);

                    // 后处理
                    foreach (var f in filters) { f.Filter (log); }

                    if (Logs.Count > 0) {
                        var l = Logs[Logs.Count - 1];
                        Logs.RemoveAt (Logs.Count - 1);
                        Logs.Add (l);
                    }
                    Logs.Add (log);

                    // 统计
                    if (log.week_code != 0 && log.week_code != 6 && !d.IsNowDay()) {
                        TotalDay++;
                        FinishDay += (log.type_in == TimeType.Normal && log.type_out == TimeType.Normal) ? 1 : 0;
                    }
                }
                Ratio = totalDay > 0 ? (float)finishDay / (float)totalDay : 1;
                Ratio *= 100;
            }
            catch (Exception _e) {
                MessageBox.Show(_e.Message);
                inwork = false;
            }

_end_:
            inwork = false;
        }

        async Task getSelectLogAsync() {
            if (SelectDate.Year == DateTime.Now.Year && SelectDate.Month >= DateTime.Now.Month) {
                await getLogAsync(DateTime.Now);
            }
            else {
                var date = new DateTime(SelectDate.Year, SelectDate.Month, DateTime.DaysInMonth(SelectDate.Year, SelectDate.Month));
                await getLogAsync(date);
            }
        }

        public MainViewModel() {
            Name = Properties.Settings.Default.Name;
            Password = Security.Decrypt(Properties.Settings.Default.Password, "p@ssw0rd");

            Config.loadConfig ();

            Config.onConfigUpdated += updateLogs;
            Config.onDataClear += ClearAccount;

            if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Password)) {
                getLogAsync(DateTime.Now);
            }
        }

        void ClearAccount() {
            Name = Properties.Settings.Default.Name = "";
            Password = Properties.Settings.Default.Password = "";

            Properties.Settings.Default.Save();

            Fufu.logout();
            Logs.Clear();
        }

        void updateLogs() {
            for (int i = 0; i < Logs.Count; i++) {
                // 移除后重新添加才会刷新
                Log ls = Logs[i]; Logs.RemoveAt(i);
                Log lsn = null;
                if (i > 0) {
                    lsn = Logs[i - 1];
                    Logs.RemoveAt (i - 1);
                }
                ls.re_time_in = false;
                ls.re_time_out = false;

                foreach (var f in filters) f.PreFilter (ls, lsn);
                foreach (var f in filters) f.Filter(ls);
                if( i > 0 ) Logs.Insert (i -1 , lsn);
                Logs.Insert (i, ls);
            }
        }

        public override void Dispose() {
            base.Dispose();
            Properties.Settings.Default.Name = Name;
            Properties.Settings.Default.Password = Security.Encrypt(Password, "p@ssw0rd");

            Config.saveConfig ();
            Properties.Settings.Default.Save();

            foreach (var f in filters) {
                if (f is IDisposable)
                    (f as IDisposable).Dispose();
            }

            Fufu.logout();
        }
    }


}