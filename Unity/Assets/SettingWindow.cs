using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WpfApp1;

public class SettingWindow : MonoBehaviour {

    [Header ("Data")]
    public InputField user;
    public InputField passwd;

    [Header ("Button")]
    public Button saveBtn;
    public Button clearBtn;

    // Use this for initialization
    void Start () {
        user.text = Config.name;
        passwd.text = Config.password;

        saveBtn.onClick.AddListener (() => {
            Config.name = user.text;
            Config.password = passwd.text;
            Config.saveConfig ();
            this.FinishWindow ();
        });

        clearBtn.onClick.AddListener (() => {
            user.text = "";
            passwd.text = "";
        });
    }
}
