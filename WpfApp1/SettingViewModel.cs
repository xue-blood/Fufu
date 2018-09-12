using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1 {
    public class SettingViewModel : ViewModel {
        // 数据
        public ObservableCollection<TimeData> TimeTypes { get { return Config.timeTypes; } set { Config.timeTypes = value; } }
        public ObservableCollection<OutTimeData> OutTimes { get { return Config.timeReorder; } set { Config.timeReorder = value; } }
        public bool enableOutTime { get { return Properties.Settings.Default.enableOutTime; } set { Properties.Settings.Default.enableOutTime = value; RaisePropertyChanged (); Config.UpdateConfig (); } }


        public IEnumerable<TimeType> TypeList { get { return Enum.GetValues (typeof (TimeType)).Cast<TimeType> (); } }

        public IEnumerable<string> Colors { get { return colors ?? (colors = ColorHelp.Colors.Keys); } }
        private IEnumerable<string> colors;

        public Command ClearCmd { get { return clearCmd ?? (clearCmd = new Command { ExecuteDelegate = _ => Config.ClearData () }); } }
        private Command clearCmd;

        public Command ResetCmd { get { return resetCmd ?? (resetCmd = new Command { ExecuteDelegate = _ => reset () }); } }
        private Command resetCmd;

        void reset () { Config.pauseUpdate = true; Config.loadConfig (); TimeTypes.Refresh (); OutTimes.Refresh (); Config.pauseUpdate = false; Config.UpdateConfig (); }


        public Command ShowCmd { get { return showCmd ?? (showCmd = new Command { ExecuteDelegate = _ => show () }); } }
        private Command showCmd;
        void show () { Config.pauseUpdate = true; TimeTypes.Refresh (( c ) => c.type = TimeType.Normal); Config.pauseUpdate = false; Config.UpdateConfig (); }

        public Command HideCmd { get { return hideCmd ?? (hideCmd = new Command { ExecuteDelegate = _ => hide () }); } }
        private Command hideCmd;
        void hide () { Config.pauseUpdate = true; TimeTypes.Refresh (( c ) => c.type = TimeType.None); Config.pauseUpdate = false; Config.UpdateConfig (); }


        public Command AdvCmd { get { return advCmd ?? (advCmd = new Command { ExecuteDelegate = _ => adv () }); } }
        private Command advCmd;


        public int SizeX { get { return sizeX; } set { sizeX = value; onsize (); RaisePropertyChanged (); } }
        private int sizeX = 450;

        // 基本设置
        public Thickness BasMargin { get { return basMargin; } set { basMargin = value; RaisePropertyChanged (); } }
        private Thickness basMargin;


        // 高级设置
        public Thickness AdvMargin { get { return advMargin; } set { advMargin = value; RaisePropertyChanged (); } }
        private Thickness advMargin;

        bool adv_open = false;

        void onsize () {
            if (!adv_open) {
                BasMargin = new Thickness (0, 0, 0, 0);
                AdvMargin = new Thickness (SizeX, 0, 0, 0);
            }
            else {
                BasMargin = new Thickness (0, 0, 300, 0);
                AdvMargin = new Thickness (SizeX - 300, 0, 0, 0);
            }
        }

        void adv () {
            sizeX += adv_open ? -300 : 300;
            RaisePropertyChanged ("SizeX");
            adv_open = !adv_open;
            onsize ();
        }

        /// <summary>
        /// 上班配置
        /// </summary>
        public string NewConfig { get { return newConfig; } set { newConfig = value; RaisePropertyChanged (); } }
        private string newConfig = Properties.Settings.Default.TimeTypes;

        /// <summary>
        /// 下班配置
        /// </summary>
        public string OutConfig { get { return outConfig; } set { outConfig = value; RaisePropertyChanged (); } }
        private string outConfig = Properties.Settings.Default.TimeReourde;


        public Command OrgCmd { get { return orgCmd ?? (orgCmd = new Command { ExecuteDelegate = _ => org_cfg () }); } }
        private Command orgCmd;

        public Command NewCmd { get { return newCmd ?? (newCmd = new Command { ExecuteDelegate = _ => new_cfg () }); } }
        private Command newCmd;


        void org_cfg () {
            if (MessageBox.Show ("确定将配置重设为默认值？", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                Config.loadConfig ();
            }
        }

        void new_cfg () {
            if (MessageBox.Show ("确定导入配置？", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                Config.loadConfig (NewConfig, OutConfig);
            }
        }

        public override void Dispose () {
            base.Dispose ();
            Config.saveConfig ();
            Properties.Settings.Default.Save ();
        }
    }
}