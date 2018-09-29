using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Properties {

    public class Settings : MonoBehaviour {
        static Settings _default;
        public static Settings Default { get { return _default ?? (_default = new Settings ()); } }

        public void Save () {
            PlayerPrefs.Save ();
        }
        string timeTypes;
        public string TimeTypes {
            get { return timeTypes ?? (timeTypes = PlayerPrefs.GetString (nameof (TimeTypes), OrgTimeTypes)); }
            set { timeTypes = value; PlayerPrefs.SetString (nameof (TimeTypes), value); }
        }
        public string OrgTimeTypes = @"0:00,0:00,2,True,#B22222,上班未打卡|0:00,9:30,0,True,#228B22,|9:30,9:40,1,True,#A0522D,|9:40,10:00,1,True,#B22222,|10:00,12:00,2,True,#B22222,|12:00,13:30,0,True,#228B22,|13:30,18:00,0,True,#B22222,|18:00,22:00,0,False,#008000,|22:00,23:59,0,False,#008000,|0:00,0:00,2,False,DarkGoldenrod,下班未打卡";

        string timeReourde;
        public string TimeReourde {
            get { return timeReourde ?? (timeReourde = PlayerPrefs.GetString (nameof (TimeReourde), OrgTimeReourde)); }
            set { timeReourde = value; PlayerPrefs.SetString (nameof (TimeReourde), value); }
        }
        public string OrgTimeReourde = "22:00,1.00:00,10:00,#F08080|1.00:00,1.9:00,13:30,#FF0000";

        string user;
        public string User {
            get { return user ?? (user = PlayerPrefs.GetString (nameof (User))); }
            set { user = value; PlayerPrefs.SetString (nameof (User), value); }
        }
        string passwd;
        public string Password {
            get { return passwd ?? (passwd = PlayerPrefs.GetString (nameof (Password))); }
            set { passwd = value; PlayerPrefs.SetString (nameof (Password), value); }
        }

        public bool enableOutTime = true;
        public string OutTime = "18:00";
    }

}