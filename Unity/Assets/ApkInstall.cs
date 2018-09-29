using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// APK install.
/// 使用方法：
/// <1>该脚本不需要继承MonoBehavior;
/// <2>直接用APKInstall.getInstance().installAPP(path);就可以
/// </summary>
public class APKInstall {
    private static APKInstall instance;

    public static APKInstall GetInstance () {
        if (instance == null) {
            instance = (APKInstall)Activator.CreateInstance (typeof (APKInstall), true);
        }
        return instance;
    }

    //下载并写入文件
    //actionBytes- 下载完毕对bytes的处理
    //percentAciton - 进度处理
    //endAciton- 完成回调（写入到Application.persistentDataPath目录，或者您的别的目录）
    public IEnumerator DownloadFile ( string url, string downLoadPathName, Action<string, string, byte[]> actionBytes, Action<float> percentAciton, Action<bool, string, string> endAciton ) {
        WWW www = new WWW (url);
        while (!www.isDone) {
            if (null != percentAciton) percentAciton (www.progress);
            yield return null;
        }

        if (!string.IsNullOrEmpty (www.error)) {
            Debug.LogError ("WWW DownloadFile:" + www.error);
            if (null != endAciton) endAciton (false, www.error, downLoadPathName);
            yield break;
        }

        if (null != actionBytes) actionBytes (url, downLoadPathName, www.bytes);
        if (null != endAciton) endAciton (true, www.text, downLoadPathName);
        www.Dispose ();
    }

    //bReTry表示第一次安装不成功的时候，再试一次下面的方式
    public bool InstallAPK ( string path, bool bReTry ) {
        try {
            var Intent = new AndroidJavaClass ("android.content.Intent");
            var ACTION_VIEW = Intent.GetStatic<string> ("ACTION_VIEW");
            var FLAG_ACTIVITY_NEW_TASK = Intent.GetStatic<int> ("FLAG_ACTIVITY_NEW_TASK");
            var intent = new AndroidJavaObject ("android.content.Intent", ACTION_VIEW);

            var file = new AndroidJavaObject ("java.io.File", path);
            var Uri = new AndroidJavaClass ("android.net.Uri");
            var uri = Uri.CallStatic<AndroidJavaObject> ("fromFile", file);

            intent.Call<AndroidJavaObject> ("setDataAndType", uri, "application/vnd.android.package-archive");

            if (!bReTry) {
                intent.Call<AndroidJavaObject> ("addFlags", FLAG_ACTIVITY_NEW_TASK);
                intent.Call<AndroidJavaObject> ("setClassName", "com.android.packageinstaller", "com.android.packageinstaller.PackageInstallerActivity");
            }

            var UnityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
            var currentActivity = UnityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
            currentActivity.Call ("startActivity", intent);

            Debug.Log ("Install New Apk Ok");
            return true;
        }
        catch (System.Exception e) {
            Debug.LogError ("Error Install APK:" + e.Message + " -- " + e.StackTrace + "  bRetry=" + bReTry);
            return false;
        }
    }
}