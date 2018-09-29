using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (Text))]
public class VersionView : MonoBehaviour {

    void Start () {
        GetComponent<Text> ().text = Lancher.Instance.version;
    }
}
