﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// By 黄冬 2018 @ 仙剑 Youkia
/// </summary>

public class Fufu {

    const string redis_url = "https://zkcserv.com";

    static string server_url = ""; // 真实地址
    static string user_name = "";  // 真实用户名
    static string pass_word = "";  // 真实密码

    static bool isLogin = false;

    public static async Task<bool> loginAsync ( string name, string password ) {
        if (isLogin)
            return true;

        // 获取服务器URL
        // https://zkcserv.com/UserServer/login?username=&password=&is_mobile=1&_is_login=0
        var loginContent = new[] {
                new KeyValuePair<string, string>("username", name),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("is_mobile", "1"),
                new KeyValuePair<string, string>("_is_login", "0"),
            };
        var ret = await (redis_url + "/UserServer/login").GetJsonAsync (loginContent);
        if (ret != null && ret.Get<bool> ("success")) {
            server_url = ret.Get<string> ("url");
        }
        else
            goto _Login_Failed_;

        // 登录
        // https://s9.zkcserv.com/cb_hrms/index.cfm?event=ionicAction.ionicAction.userLogin&_user_name=&_pass_word=&_notification_token=&_device_type=ios&is_mobile=1&_is_login=0&_dc=1533710057573
        loginContent = new[] {
                new KeyValuePair<string, string>("event", "ionicAction.ionicAction.userLogin"),
                new KeyValuePair<string, string>("_user_name", name),
                new KeyValuePair<string, string>("_pass_word", password),
                new KeyValuePair<string, string>("is_mobile", "1"),
                new KeyValuePair<string, string>("_is_login", "0"),
                new KeyValuePair<string, string>("_notification_token", ""),
                new KeyValuePair<string, string>("_device_type", "ios"),
                new KeyValuePair<string, string>("_dc", DateTime.Now.ToUnixTimestamp().ToString()),
            };
        ret = await server_url.GetJsonAsync (loginContent);
        if (ret != null && ret.Get<string> ("flag") == "1") {
            user_name = ret.Get<string> ("_user_name");
            pass_word = ret.Get<string> ("_pass_word");
        }
        else
            goto _Login_Failed_;

        Debug.Log ("登录成功");
        isLogin = true;
        return true;

        _Login_Failed_:
        throw new Exception ("登录失败");
    }

    public static void logout () {
        if (!isLogin)
            return;

        var content = new[] {
                new KeyValuePair<string, string>("event", "ionicAction.ionicAction.doLogout"),
            };
        server_url.GetJsonAsync (content);

        isLogin = false;
    }

    static string[] week_arr = new[] { "日", "一", "二", "三", "四", "五", "六" };

    public static async Task<Log> getLogAsync ( DateTime _date ) {
        if (!isLogin)
            return null;

        string date = _date.ToString ("yyyyMMdd");

        var log = new Log ();

        // 获取
        // https://s9.zkcserv.com/cb_hrms/index.cfm?event=ionicAction.ionicAction.loadAtHomeResultData&_user_name=&_pass_word=&_is_login=1&_notification_token=&_device_type=ios&curr_date=20180809
        var content = new[] {
                new KeyValuePair<string, string>("event", "ionicAction.ionicAction.loadAtHomeResultData"),
                new KeyValuePair<string, string>("_user_name", user_name),
                new KeyValuePair<string, string>("_pass_word", pass_word),
                new KeyValuePair<string, string>("_is_login", "1"),
                new KeyValuePair<string, string>("_notification_token", ""),
                new KeyValuePair<string, string>("_device_type", "ios"),
                new KeyValuePair<string, string>("curr_date", date),
            };
        var ret = await server_url.GetJsonAsync (content);

        log._date = _date;
        log.date = _date.ToString ("yy年M月d日");
        log.week_code = (int)_date.DayOfWeek;
        log.week = week_arr[log.week_code];

        if (ret != null) {
            var cards = ret.Get<List<object>> ("card_data");
            if (cards.Count > 0) {
                var cd = cards[0] as Dictionary<string, object>;
                log.time_in = cd.Get<string> ("time_in");
                log.time_out = cd.Get<string> ("time_out");
                TimeSpan a, b;
                TimeSpan.TryParse (log.time_in, out a);
                TimeSpan.TryParse (log.time_out, out b);
                log.records = new List<TimeSpan> ();
                log.records.Add (a);
                log.records.Add (b);
            }
            else {
                // 尝试第二种方法
                return await getLogAsync2 (_date);
            }
        }
        else {
            // 尝试第二种方法
            return await getLogAsync2 (_date);
        }

        return log;
    }

    // 获取数据第二种方法
    // https://s9.zkcserv.com/cb_hrms/index.cfm?event=ionicAction.ionicAction.getAtSignInData&_user_name=&_pass_word=&_is_login=1&_notification_token=&_device_type=ios&current_date=20180809&time_zone=+08:00
    public static async Task<Log> getLogAsync2 ( DateTime _date ) {
        if (!isLogin)
            return null;
        string date = _date.ToString ("yyyyMMdd");

        var log = new Log ();

        // 获取
        var content = new[] {
                new KeyValuePair<string, string>("event", "ionicAction.ionicAction.getAtSignInData"),
                new KeyValuePair<string, string>("_user_name", user_name),
                new KeyValuePair<string, string>("_pass_word", pass_word),
                new KeyValuePair<string, string>("_is_login", "1"),
                new KeyValuePair<string, string>("_notification_token", ""),
                new KeyValuePair<string, string>("_device_type", "ios"),
                new KeyValuePair<string, string>("current_date", date),
                new KeyValuePair<string, string>("time_zone", "+08:00"),
            };
        var ret = await server_url.GetJsonAsync (content);


        log._date = _date;
        log.date = _date.ToString ("yy年M月d日");
        log.week_code = (int)_date.DayOfWeek;
        log.week = week_arr[log.week_code];

        if (ret != null) {
            var cards = ret.Get<List<object>> ("ca_r");
            if (cards.Count > 0) {
                log.records = new List<TimeSpan> ();
                for (int i = 0; i < cards.Count; i++) {
                    TimeSpan t;
                    TimeSpan.TryParse ((cards[i] as Dictionary<string, object>).Get<string> ("ti"), out t);
                    log.records.Add (t);
                }
            }
        }

        return log;
    }
}

public enum TimeType {
    Normal, // 正常
    Color,  // 颜色
    None,   // 隐藏
}

public class Log {
    // 日期
    public string date { get; set; }
    public DateTime _date { get; set; }
    // 星期
    public string week { get; set; }
    public int week_code { get; set; }
    // 时间
    public string time_in { get; set; }
    public string time_out { get; set; }
    public bool re_time_in { get; set; }
    public bool re_time_out { get; set; }

    // 所有数据
    public List<TimeSpan> records { get; set; }

    // 界面扩展数据
    public TimeType type_in { get; set; }
    public TimeType type_out { get; set; }

    public string color_in { get; set; }
    public string color_out { get; set; }

    public bool isWeekEnd () { return week_code == 0 || week_code == 6; }
    public bool isNone () { return (string.IsNullOrEmpty (time_in) && string.IsNullOrEmpty (time_out)); }
    public bool needShow () { return !isWeekEnd () && !isNone (); }
    public override string ToString () { return date + " " + week + ":" + time_in + "  " + time_out; }
}