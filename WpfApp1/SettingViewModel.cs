using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1 {
    public class SettingViewModel : ViewModel {

        public ObservableCollection<TimeData> TimeTypes { get { return Config.timeTypes; } set { Config.timeTypes = value; } }

        public IEnumerable<TimeType> TypeList { get { return Enum.GetValues(typeof(TimeType)).Cast<TimeType>(); } }

        public IEnumerable<string> Colors { get { return colors ?? (colors = ColorHelp.Colors.Keys); } }
        private IEnumerable<string> colors;

        public Command ClearCmd { get { return clearCmd ?? (clearCmd = new Command { ExecuteDelegate = _ => Config.ClearData() }); } }
        private Command clearCmd;
    }
}
