using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainWindow : MonoBehaviour {

    [Header ("Tools")]
    public Button settingbtn;

    [Header ("Button")]
    public Dropdown yearDd;
    public Dropdown mouthDd;
    public Dropdown dayDd;

    [Header ("Button")]
    public Button nowbtn;
    public Button weekbtn;
    public Toggle autoNowday;
    public float autoInterval = 1.5f;

    List<logItem> logList;
    [Header ("Logs")]
    public GameObject grid;
    public GameObject prefab;


    WpfApp1.MainViewModel vm;

    private void Awake () {
        vm = new WpfApp1.MainViewModel ();
        vm.PropertyChanged += Vm_PropertyChanged;
    }

    void Start () {
        settingbtn.onClick.AddListener (() => {
            Lancher.Instance.OpenDialogWindow<SettingWindow> ();
        });

        // 日
        Callback<int, int> updateDayList = ( y, m ) => {
            List<string> day = new List<string> ();
            int max = yearDd.value == 0 && mouthDd.value == 0 ? DateTime.Now.Day : DateTime.DaysInMonth (y, m);
            for (int i = 1; i <= max; i++) {
                var d = new DateTime (y, m, Mathf.Min (i, max));
                i += 7 - (int)d.DayOfWeek;
                var n = new DateTime (y, m, Mathf.Min (i, max));
                day.Add (d.Day.ToString () + "~" + Mathf.Min (i, max).ToString () + "日");
            }
            day.Reverse ();
            dayDd.ClearOptions ();
            dayDd.AddOptions (day);
        };
        updateDayList (DateTime.Now.Year, DateTime.Now.Month);
        dayDd.value = 0;
        dayDd.onValueChanged.AddListener (( i ) => {
            getLog (false, false);
        });

        // 月
        Callback<int> updateMouthList = ( y ) => {
            List<string> mouth = new List<string> ();
            int max = y == DateTime.Now.Year ? DateTime.Now.Month : 12;
            for (int i = max; i > 0; i--) mouth.Add (i.ToString () + "月");
            mouthDd.ClearOptions ();
            mouthDd.AddOptions (mouth);
        };
        updateMouthList (DateTime.Now.Year);
        mouthDd.value = 0;
        mouthDd.onValueChanged.AddListener (( i ) => {
            updateDayList (DateTime.Now.Year - yearDd.value, mouthDd.options.Count - mouthDd.value);
            getLog (false);
        });

        // 年
        var now = DateTime.Now;
        List<string> year = new List<string> ();
        for (int i = 0; i < 3; i++) year.Add ((now.Year - i).ToString () + "年");
        yearDd.ClearOptions ();
        yearDd.AddOptions (year);
        yearDd.value = 0;
        yearDd.onValueChanged.AddListener (( i ) => {
            updateMouthList (now.Year - yearDd.value);
        });

        nowbtn.onClick.AddListener (() => {
            getLog (true);
        });
        weekbtn.onClick.AddListener (() => {
            dayDd.value = 0;
            yearDd.value = 0;
            mouthDd.value = 0;
            getLog (false, false, true);
        });
        autoNowday.onValueChanged.AddListener (( b ) => {
            if (b) StartCoroutine (autoGetNowDay ());
            else StopAllCoroutines ();
            yearDd.interactable = mouthDd.interactable = dayDd.interactable = !b;
            weekbtn.interactable = nowbtn.interactable = !b;
        });
    }

    Coroutine timeOutCo;
    void getLog ( bool now, bool mouth = true, bool week = false ) {

        nowbtn.interactable = false;
        yearDd.interactable = false;
        mouthDd.interactable = false;
        weekbtn.interactable = false;
        dayDd.interactable = false;

        if (now) {
            yearDd.value = 0;
            mouthDd.value = mouthDd.options.Count - DateTime.Now.Month;
            dayDd.value = 0;
            vm.getLogAsync (DateTime.Now.Year, DateTime.Now.Month, 1, DateTime.Now.Day);
        }
        else {
            if (mouth)
                vm.getMonthLog (DateTime.Now.Year - yearDd.value, mouthDd.options.Count - mouthDd.value);
            else if (week)
                vm.getLogAsync (DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.getLastMonday (), DateTime.Now.Day);
            else {
                var str = dayDd.options[dayDd.value].text.Split ('~');
                int sta = str[0].ToInt ();
                int end = str[1].Substring (0, str[1].Length - 1).ToInt ();
                vm.getLogAsync (DateTime.Now.Year - yearDd.value, mouthDd.options.Count - mouthDd.value, sta, end);
            }
        }

        timeOutCo = StartCoroutine (getLogTimeout ());
    }

    IEnumerator getLogTimeout () {
        yield return new WaitForSeconds (15);
        MessageLine.Show ("请求超时");
        mouthDd.interactable = true;
        yearDd.interactable = true;
        dayDd.interactable = true;
        nowbtn.interactable = true;
        weekbtn.interactable = true;
    }
    IEnumerator autoGetNowDay () {
        while (true) {
            var str = dayDd.options[dayDd.value].text.Split ('~');
            int sta = str[0].ToInt ();
            int end = str[1].Substring (0, str[1].Length - 1).ToInt ();
            vm.getLogAsync (DateTime.Now.Year - yearDd.value, mouthDd.options.Count - mouthDd.value, sta, end, true);
            logList[0].updateAnime ();
            yield return new WaitForSeconds (autoInterval);
        }
    }
    private void Vm_PropertyChanged ( object sender, System.ComponentModel.PropertyChangedEventArgs e ) {
        if (e.PropertyName == "Logs")
            this.UpdateGrid (ref logList, vm.Logs.Count, grid, prefab, ( i ) => {
                logList[i].init (vm.Logs[i]);
            });
        else if (e.PropertyName == "Logs_End") {
            mouthDd.interactable = !autoNowday.isOn;
            yearDd.interactable = !autoNowday.isOn;
            dayDd.interactable = !autoNowday.isOn;
            nowbtn.interactable = !autoNowday.isOn;
            weekbtn.interactable = !autoNowday.isOn;
            if (timeOutCo != null) StopCoroutine (timeOutCo);
        }
        else if (e.PropertyName == "Login_Failed") {
            MessageLine.Show ("登录失败".color ("red"));
        }
    }
}
