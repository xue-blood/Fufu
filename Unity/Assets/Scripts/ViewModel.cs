using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1 {
    public class ViewModel : INotifyPropertyChanged, IDisposable {
        #region interface INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. 
        /// this function also using CallerMemberName, so you could not use param</param>
        protected virtual void RaisePropertyChanged ( [CallerMemberName] string propertyName = null ) {
            if (PropertyChanged != null) {
                PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
            }
        }

        #endregion

        #region 实例
        public static Dictionary<Type, ViewModel> Instances = new Dictionary<Type, ViewModel> ();
        public static ViewModel Get<T> () {
            var t = typeof (T);
            if (Instances.ContainsKey (t))
                return Instances[t];
            return null;
        }

        public virtual void Dispose () { Instances[GetType ()] = null; }

        public ViewModel () {
            var t = this.GetType ();
            if (!Instances.ContainsKey (t))
                Instances.Add (t, this);
            else
                Instances[t] = this;
        }
        #endregion

        #region 字段


        #endregion

        #region 命令


        #endregion

        #region 函数


        #endregion
    }
}