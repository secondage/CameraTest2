using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using DeadMosquito.AndroidGoodies;
using Newtonsoft.Json;
using System.IO;

public class IntroPageController : MonoBehaviour
{
    public Text textSelectedHospital;
    public Text textSelecteDepartment;
    public Text textNumPeoples;
    [SerializeField]
    Button btnStart;
    [SerializeField]
    Button btnPreview;
    [SerializeField]
    Button btnMgr;
    [SerializeField]
    GameObject mgrPage;
    [SerializeField]
    GameObject answerPage;
    public GameObject departmentPage;

    public InputField passwordInput;
    public Text textTopic;
    public Text textVerSimple;
    public GameObject loadingPanel;
    public GameObject upgradePanel;
    public Image fillImg;
    public Button upgradeBtn;

    public Text verText;

    private void OnEnable()
    {
#if UNITY_ANDROID

        var builder = new StringBuilder();
        // Device info
        builder.AppendLine("ANDROID_ID : " + AndroidDeviceInfo.GetAndroidId());
        builder.AppendLine("APP_VERSION : " + Application.version.ToString());

        builder.AppendLine("----------- Build class------------");
        builder.AppendLine("DEVICE : " + AndroidDeviceInfo.DEVICE);
        builder.AppendLine("MODEL : " + AndroidDeviceInfo.MODEL);
        builder.AppendLine("SERIAL : " + AndroidDeviceInfo.SERIAL);
        builder.AppendLine("PRODUCT : " + AndroidDeviceInfo.PRODUCT);
        builder.AppendLine("MANUFACTURER : " + AndroidDeviceInfo.MANUFACTURER);
        builder.AppendLine("RESOLUTION : " + Screen.currentResolution.ToString());
        builder.AppendLine("SDK : " + AndroidDeviceInfo.SDK_INT);

        verText.text = builder.ToString();
        textVerSimple.text = "版本号: " + Application.version.ToString() + " 设备号: " + AndroidDeviceInfo.GetAndroidId();
        if (Application.internetReachability != NetworkReachability.NotReachable && Application.platform != RuntimePlatform.WindowsEditor)
        {
            CheckNeedUpgrade();
        }
#endif
        if (PollsConfig.selectedHospital != null)
        {
            textSelectedHospital.text = PollsConfig.selectedHospital.name;
            textSelectedHospital.color = Color.blue;
        }
        else
        {
            textSelectedHospital.text = "未选定医院";
            textSelectedHospital.color = Color.red;
        }
        if (PollsConfig.selectedDepartment != null)
        {
            textSelecteDepartment.text = PollsConfig.selectedDepartment.name;
            textSelecteDepartment.color = Color.blue;
        }
        else
        {
            textSelecteDepartment.text = "未选定科室";
            textSelecteDepartment.color = Color.red;
        }
        if (PollsConfig.selectedHospital != null /*&& PollsConfig.selectedDepartment != null*/)
        {
            //check camera auth
            if (MgrPageController.cameraAuthed == 1 || Application.platform == RuntimePlatform.WindowsEditor)
            {
                btnStart.interactable = true;
                btnPreview.interactable = true;
            }
        }
        else
        {
            btnStart.interactable = false;
            btnPreview.interactable = false;
        }
        textNumPeoples.text = "人数 " + PollsConfig.NumPeoples.ToString();
    }
    // Use this for initialization
    void Start()
    {
        StartCoroutine(GetWebCamAuthorization());
    }

    // Update is called once per frame
    void Update()
    {
        if (wwwUpgrade != null)
        {
            Debug.Log(wwwUpgrade.progress);
            fillImg.fillAmount = wwwUpgrade.progress;
        }
    }

    public void OnMgrBtnClick()
    {
        if (PollsConfig.isOfflineMode)
        {
            btnMgr.interactable = false;
            if (PollsConfig.Password != "")
            {
                textTopic.text = "请输入管理员密码";
                passwordInput.text = "";
                passwordInput.ActivateInputField();
            }
            else
            {
                btnMgr.interactable = true;
                this.gameObject.SetActive(false);
                mgrPage.gameObject.SetActive(true);
            }
        }
        else
        {
            //check network status first
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Toast.ShowToast("当前网络不可用，请检查后重试.");
            }
            else
            {
                textTopic.text = "请输入管理员密码";
                passwordInput.text = "";
                passwordInput.ActivateInputField();
            }
        }
    }

    public void OnStartBtnClick()
    {
        /*if (PollsConfig.selectedHospital != null && PollsConfig.selectedDepartment != null)
        {
            AnswerPageController.previewMode = false;
            this.gameObject.SetActive(false);
            answerPage.gameObject.SetActive(true);
        }*/
        DepartmentMgrPageController.selectMode = true;
        this.gameObject.SetActive(false);
        departmentPage.gameObject.SetActive(true);
    }

    public void OnPreviewBtnClick()
    {
        if (PollsConfig.selectedHospital != null && PollsConfig.selectedDepartment != null)
        {
            AnswerPageController.previewMode = true;
            this.gameObject.SetActive(false);
            answerPage.gameObject.SetActive(true);
        }
    }

    public bool InstallAPK(string path)
    {
        try
        {
            
            var Intent = new AndroidJavaClass("android.content.Intent");
            var ACTION_VIEW = Intent.GetStatic<string>("ACTION_VIEW");
            var FLAG_ACTIVITY_NEW_TASK = Intent.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
            var intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);

            var file = new AndroidJavaObject("java.io.File", path);
            var Uri = new AndroidJavaClass("android.net.Uri");
            var uri = Uri.CallStatic<AndroidJavaObject>("fromFile", file);

            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
            intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);
            intent.Call<AndroidJavaObject>("setClassName", "com.android.packageinstaller", "com.android.packageinstaller.PackageInstallerActivity");

            var UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("startActivity", intent);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            Toast.ShowToast(e.Message);
            return false;
        }
    }


    private bool installApp24(string apkPath)
    {
        bool success = true;
        //GameObject.Find("TextDebug").GetComponent<Text>().text = "Installing App";

        try
        {
            //Get Activity then Context
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            //Get the package Name
            string packageName = unityContext.Call<string>("getPackageName");
            string authority = packageName + ".fileprovider";

            AndroidJavaClass intentObj = new AndroidJavaClass("android.content.Intent");
            string ACTION_VIEW = intentObj.GetStatic<string>("ACTION_VIEW");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);


            int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
            int FLAG_GRANT_READ_URI_PERMISSION = intentObj.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");

            //File fileObj = new File(String pathname);
            AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", apkPath);
            //FileProvider object that will be used to call it static function
            AndroidJavaClass fileProvider = new AndroidJavaClass("android.support.v4.content.FileProvider");
            //getUriForFile(Context context, String authority, File file)
            AndroidJavaObject uri = fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", unityContext, authority, fileObj);

            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
            intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);
            intent.Call<AndroidJavaObject>("addFlags", FLAG_GRANT_READ_URI_PERMISSION);
            currentActivity.Call("startActivity", intent);

            //GameObject.Find("TextDebug").GetComponent<Text>().text = "Success";
        }
        catch (System.Exception e)
        {
            Toast.ShowToast(e.Message);
            //GameObject.Find("TextDebug").GetComponent<Text>().text = "Error: " + e.Message;
            success = false;
        }

        return success;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="local"></param>
    /// <param name="remote"></param>
    /// <returns> True for need to upgrade </returns>
    bool CompareVersion(string local, string remote)
    {
        string[] locals = local.Split('.');
        string[] remotes = remote.Split('.');
        if (locals.Length < 2 || remote.Length < 2)
        {
            Toast.ShowToast("版本号解析错误");
            Debug.LogError("版本号解析错误 local " + local + " " + "remote " + remote);
            return false;
        }
        if (int.Parse(locals[0]) > int.Parse(remotes[0]))
            return false;
        if (int.Parse(locals[0]) < int.Parse(remotes[0]))
            return true;
        if (int.Parse(locals[1]) > int.Parse(remotes[1]))
            return false;
        if (int.Parse(locals[1]) < int.Parse(remotes[1]))
            return true;
        if (remotes.Length == 3)
        {
            if (locals.Length == 2)
                return true;
            if (locals.Length == 3)
            {
                if (int.Parse(locals[2]) > int.Parse(remotes[2]))
                    return false;
                if (int.Parse(locals[2]) < int.Parse(remotes[2]))
                    return true;
            }
        }
        return false;
    }
    WWW wwwUpgrade = null;
    IEnumerator _webGetApk(string version/*exportDataBase64 edb64*/)
    {
        wwwUpgrade = new WWW("http://47.106.71.112:8080/hospital_" + version + ".apk");
        yield return wwwUpgrade;
        if (wwwUpgrade.isDone && wwwUpgrade.error == null && wwwUpgrade.bytes != null && wwwUpgrade.bytes.Length > 0)
        {
            Debug.Log("upgrade package downloaded.");
           
            //save file
            using (FileStream file = new FileStream(Application.persistentDataPath + "/hospital_" + version + ".apk", FileMode.Create))
            {
                file.Write(wwwUpgrade.bytes, 0, wwwUpgrade.bytes.Length);
                file.Close();
            }

            wwwUpgrade.Dispose();
            wwwUpgrade = null;

            upgradeBtn.interactable = true;
            upgradePanel.SetActive(false);

            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                if (AndroidDeviceInfo.SDK_INT < 24)
                    InstallAPK(Application.persistentDataPath + "/hospital_" + version + ".apk");
                else
                    installApp24(Application.persistentDataPath + "/hospital_" + version + ".apk");
                Application.Quit();
            }
        }
        else
        {
            Toast.ShowToast("下载更新包错误。");
            Debug.LogError(wwwUpgrade.error);
        }
    }

    public void OnUpgradeBtnClick()
    {
        //upgradeBtn.interactable = false;
        //StartCoroutine(_webGetApk(remoteVersion));
        upgradePanel.SetActive(false);
        if (wwwUpgrade != null)
        {
            wwwUpgrade.Dispose();
            wwwUpgrade = null;
        }
        StopCoroutine(downloadFunc);

    }

    string remoteVersion;
    bool forceupdate = false;
    IEnumerator downloadFunc = null;
    class versionClass { public string version { get; set; } public bool force { get; set; } }
    IEnumerator _webGetVersion(/*exportDataBase64 edb64*/)
    {
        WWW www = new WWW("http://47.106.71.112:8080/version.json");
        yield return www;
        if (www.isDone && www.error == null && www.text != "")
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            versionClass ret = JsonConvert.DeserializeObject<versionClass>(www.text, settings);
            remoteVersion = ret.version;
            forceupdate = ret.force;
            www.Dispose();
            Debug.Log("remote version is " + remoteVersion);
            loadingPanel.SetActive(false);
            if (CompareVersion(Application.version.ToString(), remoteVersion))
            {
                upgradePanel.SetActive(true);
                if (forceupdate)
                {
                    upgradeBtn.interactable = false;
                    downloadFunc = _webGetApk(remoteVersion);
                    StartCoroutine(downloadFunc);
                }
                else
                {
                    downloadFunc = _webGetApk(remoteVersion);
                    StartCoroutine(downloadFunc);
                    upgradeBtn.interactable = true;
                }
            }
        }
    }

    void CheckNeedUpgrade()
    {
        loadingPanel.SetActive(true);
        StartCoroutine(_webGetVersion());
    }


    public IEnumerator GetWebCamAuthorization()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {


        }
    }

    public void OnSelectDepBtnClick()
    {

    }

    public class loginRetData
    {
        public int iUserID { get; set; }
        public string vcToken { get; set; }
        public int iExpireDay { get; set; }
    }

    public class loginRetWithData
    {
        public int ret { get; set; }
        public loginRetData data { get; set; }
        public string info { get; set; }
    }
    public class loginRetWithoutData
    {
        public int ret { get; set; }
        public string info { get; set; }
    }
    IEnumerator webAuthPassword(string pwd)
    {
        WWWForm form = new WWWForm();
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            form.AddField("UserName", "user3");
        }
        else
        {
            form.AddField("UserName", AndroidDeviceInfo.GetAndroidId());
        }
        form.AddField("UserPwd", pwd);
        WWW www = new WWW("http://47.106.71.112/api/login.aspx", form);
        yield return www;
        if (www.isDone && www.error == null)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            loginRetWithoutData configs = JsonConvert.DeserializeObject<loginRetWithoutData>(www.text, settings);
            if (configs.ret == 1)
            {
                loginRetWithData data = JsonConvert.DeserializeObject<loginRetWithData>(www.text, settings);
                PollsConfig.netUserID = data.data.iUserID;
                PollsConfig.netUserToken = data.data.vcToken;
                PollsConfig.netExpireTime = data.data.iExpireDay;
                this.gameObject.SetActive(false);
                mgrPage.gameObject.SetActive(true);
            }
            else
            {
                Toast.ShowToast(configs.info);
            }
        }
        else if (www.error != null)
        {
            Toast.ShowToast("网络连接故障，请稍后重试。");
            Debug.LogError(www.error);
        }
    }

    public void OnPwdInputEnd()
    {

        btnMgr.interactable = true;
        textTopic.text = "";
        if (passwordInput.text == "")
            return;
        if (PollsConfig.isOfflineMode)
        {
            if (PollsConfig.GetMD5(passwordInput.text) == PollsConfig.Password)
            {
                this.gameObject.SetActive(false);
                mgrPage.gameObject.SetActive(true);
            }
            else
            {
#if UNITY_ANDROID
                Toast.ShowToast("请输入正确的管理员密码");
#endif
            }
        }
        else
        {
            StartCoroutine(webAuthPassword(passwordInput.text));
        }
    }


}
