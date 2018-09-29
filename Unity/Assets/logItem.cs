using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WpfApp1;

public class logItem : MonoBehaviour {

    public Color normal;
    public Color normalbg;
    public Color gray;

    public Text date;
    public Text week;
    public Text _in;
    public Image inbg;
    public Text _out;
    public Image outbg;

    public void init ( Log log ) {
        date.text = log.date;
        date.color = log.needShow () ? normal : gray;
        week.text = log.week;
        week.color = log.needShow () ? normal : gray;

        _in.text = log.time_in.color (log.type_in != TimeType.None ? log.color_in : "#00000000");
        _out.text = log.time_out.color (log.type_out != TimeType.None ? log.color_out : "#00000000");
        Color ic; ColorUtility.TryParseHtmlString (log.color_in, out ic);
        Color oc; ColorUtility.TryParseHtmlString (log.color_out, out oc);
        inbg.color = log.type_in == TimeType.Color ? ic : (log.needShow () ? normalbg : gray);
        outbg.color = log.type_out == TimeType.Color ? oc : (log.needShow () ? normalbg : gray);
    }
}
