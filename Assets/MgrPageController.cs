using DeadMosquito.AndroidGoodies;
using Newtonsoft.Json;
using PartaGames.Android;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


public class MgrPageController : MonoBehaviour
{
    public InputField mgrPasswordInput;
    public Button BtnExport;
    public Button BtnQuestionMgr;
    public Button BtnMgrPwd;
    public Text TextPwd;
    public Text PwdText;
    public Text TopicText;
    public GameObject HoldInputPanel;
    public GameObject introPage;
    public GameObject hospitalPage;
    public DepartmentMgrPageController departmentPageController;
    public FileChooser fileChooser;
    public PollsConfig pollsConfig;
    public GameObject loadingPanel;
    public GridLayoutGroup grid;
    public ScrollRect scrollRect;
    public MessageBox messageBox;
    //public GameObject answerPage;
    //public GameObject holdPanel;

    private static float gridOriginHeight = 0;

    public enum PWD_STAGE
    {
        NO_PWD = 0,
        VERIFY_PWD,
        VERIFY_QUESTION_PWD,
        VERIFY_EXPORT_PWD,
        NEW_PWD,
        CONFIRM_PWD,
        SAVE_PWD,
    }

    PWD_STAGE pwdStage = PWD_STAGE.NO_PWD; //0 for create ,1 for cert, 2 for change
    // Use this for initialization
    void Start()
    {
        PwdText.text = "Pwd is : " + PollsConfig.Password;
        
    }

    private void Awake()
    {
       
    }

    private void OnEnable()
    {
        if (gridOriginHeight == 0)
        {
            gridOriginHeight = grid.GetComponent<RectTransform>().sizeDelta.y;
        }
        BtnMgrPwd.gameObject.SetActive(PollsConfig.isOfflineMode);
        if (PollsConfig.isOfflineMode)
        {
            if (PollsConfig.Password == "")
            {
                TextPwd.text = "创建密码";
                BtnExport.interactable = false;
                BtnQuestionMgr.interactable = false;
                Invoke("OnSetupPwd", 0.1f);
            }
            else
            {
                TopicText.text = "";
                TextPwd.text = "管理密码";
                BtnExport.interactable = true;
                BtnQuestionMgr.interactable = true;
            }
        }
        else
        {
            BtnExport.interactable = true;
            TopicText.text = "";
            BtnQuestionMgr.interactable = true;
        }

        for (int i = 0; i < grid.transform.childCount; i++)
        {
            Destroy(grid.transform.GetChild(i).gameObject);
        }
        //fill list
        foreach (KeyValuePair<string, List<PollsConfig.AnswerStorge>> pair in PollsConfig.Answers)
        {
            PollsConfig.AnswerStorge _ans = pair.Value[0];
            AddNewCell(_ans.department.name, pair.Value.Count);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    WebCamTexture webcameratex;
    private IEnumerator startCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        Debug.Log("RequestUserAuthorization succeeded.");
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamTexture tex = new WebCamTexture(WebCamTexture.devices[1].name, 640, 480, 12);
            tex.Play();
            tex.Stop();
            tex = null;
            cameraAuthed = 1;
            PlayerPrefs.SetInt("cameraauth", cameraAuthed);
        }
    }

    string TmpPwd;
    void SetupPwd(string pwd)
    {
        HoldInputPanel.SetActive(false);
        if (pwd == null || pwd == "")
            return;
        TopicText.text = "";
        string pattern = @"^[^ ]{6,6}$";
        Regex regex = new Regex(pattern);
        if (!regex.IsMatch(pwd))
        {
#if UNITY_ANDROID
            Toast.ShowToast("密码为6位字符，且不能存在空格");
#endif
            Debug.LogWarning("密码为6位字符，且不能存在空格");
            return;
        }
        if (pwd.Length != 6)
        {
#if UNITY_ANDROID
            Toast.ShowToast("密码长度为6位");
#endif
            return;
        }
        if (pwdStage == PWD_STAGE.NEW_PWD)
        {
            TmpPwd = PollsConfig.GetMD5(pwd);
            TopicText.text = "请再次输入管理员密码";
            Invoke("InputNewPwd", 0.5f);
            pwdStage = PWD_STAGE.CONFIRM_PWD;
        }
        else if (pwdStage == PWD_STAGE.CONFIRM_PWD) //
        {
            if (TmpPwd == PollsConfig.GetMD5(pwd))
            {
#if UNITY_ANDROID
                Toast.ShowToast("管理员密码设置成功");
#endif
                TmpPwd = "";
                PollsConfig.Password = pwd;
                PwdText.text = "Pwd is : " + PollsConfig.Password;
                TextPwd.text = "管理密码";
                BtnExport.interactable = true;
                BtnQuestionMgr.interactable = true;
                TopicText.text = "";
            }
            else
            {
#if UNITY_ANDROID
                Toast.ShowToast("密码输入不一致，请重新再试");
#endif
                TmpPwd = "";
            }
        }
        else if (pwdStage == PWD_STAGE.VERIFY_PWD)
        {
            if (PollsConfig.GetMD5(pwd) == PollsConfig.Password)
            {
                TopicText.text = "请输入新的管理员密码";
                Invoke("InputNewPwd", 0.5f);
                pwdStage = PWD_STAGE.NEW_PWD;
            }
            else
            {
#if UNITY_ANDROID
                Toast.ShowToast("请输入正确的管理员密码");
#endif
                TopicText.text = "";
            }
        }
        else if (pwdStage == PWD_STAGE.VERIFY_QUESTION_PWD)
        {
            if (PollsConfig.GetMD5(pwd) == PollsConfig.Password)
            {
                this.gameObject.SetActive(false);
                hospitalPage.gameObject.SetActive(true);
            }
            else
            {
#if UNITY_ANDROID
                Toast.ShowToast("请输入正确的管理员密码");
#endif
                TopicText.text = "";
            }
            
        }
        else if (pwdStage == PWD_STAGE.VERIFY_EXPORT_PWD)
        {
            if (PollsConfig.GetMD5(pwd) == PollsConfig.Password)
            {
                if (PollsConfig.Answers.Count == 0)
                {
#if UNITY_ANDROID
                    Toast.ShowToast("目前无可导出数据");
#endif
                }
                else
                {
                    fileChooser.setup(FileChooser.OPENSAVE.OPEN, "");
                    fileChooser.openSaveButton.GetComponentInChildren<Text>().text = "选择";
                    fileChooser.TextTopic.text = "请选择需要导出至的位置";
                    fileChooser.callbackYes = delegate (string filename, string fullname)
                    {
                    //first hide the filechooser
                    fileChooser.gameObject.SetActive(false);
                        Debug.Log("select " + fullname);
                        pollsConfig.ExportData(fullname);
                    };

                    fileChooser.callbackNo = delegate ()
                    {
                        fileChooser.gameObject.SetActive(false);
                    };
                }
            }
            else
            {
#if UNITY_ANDROID
                Toast.ShowToast("请输入正确的管理员密码");
#endif
                TopicText.text = "";
            }
            
        }
    }

    public void ResumeInput()
    {
        HoldInputPanel.SetActive(false);
    }

    public void InputNewPwd()
    {
        mgrPasswordInput.ActivateInputField();
        mgrPasswordInput.text = "";
    }

    public void OnPwdInputEnd(string pwd)
    {
        SetupPwd(pwd);
    }

    public void OnSetupPwd()
    {
        HoldInputPanel.SetActive(true);
        if (PollsConfig.Password == "")
        {
            TopicText.text = "请创建管理员密码";
            pwdStage = PWD_STAGE.NEW_PWD;
        }
        else
        {
            TopicText.text = "请输入管理员密码";
            pwdStage = PWD_STAGE.VERIFY_PWD;
        }
        mgrPasswordInput.text = "";
        mgrPasswordInput.ActivateInputField();
    }

    public void OnReturnBtnClick()
    {
        fileChooser.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
        introPage.gameObject.SetActive(true);
    }

    public void OnQuestionMgrBtnClick()
    {
        /*TopicText.text = "请输入管理员密码";
        pwdStage = PWD_STAGE.VERIFY_QUESTION_PWD;
        mgrPasswordInput.text = "";
        mgrPasswordInput.ActivateInputField();*/
        this.gameObject.SetActive(false);
        hospitalPage.gameObject.SetActive(true);
    }

    static public int cameraAuthed = 0;
    public void OnCamAuthClick()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
#if UNITY_ANDROID
            Debug.Log("camera is " + PermissionGranterUnity.IsPermissionGranted("android.permission.CAMERA"));
            StartCoroutine(startCamera());
            //Debug.Log("start Permission granted:");
            //PermissionGranterUnity.GrantPermission("android.permission.CAMERA", PermissionGrantedCallback);
#endif
        }
        else
        {
            cameraAuthed = 1;
            PlayerPrefs.SetInt("cameraauth", cameraAuthed);
        }
    }

    public void OnExportBtnClick()
    {
        /*TopicText.text = "请输入管理员密码";
        pwdStage = PWD_STAGE.VERIFY_EXPORT_PWD;
        mgrPasswordInput.text = "";
        mgrPasswordInput.ActivateInputField();*/
        if (PollsConfig.Answers.Count == 0)
        {
#if UNITY_ANDROID
            Toast.ShowToast("目前无可导出数据");
#endif
        }
        else
        {
            fileChooser.setup(FileChooser.OPENSAVE.OPEN, "");
            fileChooser.openSaveButton.GetComponentInChildren<Text>().text = "选择";
            fileChooser.TextTopic.text = "请选择需要导出至的位置";
            fileChooser.callbackYes = delegate (string filename, string fullname)
            {
                //first hide the filechooser
                fileChooser.gameObject.SetActive(false);
                Debug.Log("select " + fullname);
                pollsConfig.ExportData(fullname);
                if (!PollsConfig.isOfflineMode)
                {
                    webExportData();
                }
            };

            fileChooser.callbackNo = delegate ()
            {
                fileChooser.gameObject.SetActive(false);
            };
        }
    }

    public void PermissionGrantedCallback(string permission, bool isGranted)
    {
        Debug.Log("Permission granted: " + permission + ": " + (isGranted ? "Yes" : "No"));
    }

    void MessageBoxCallback(int r)
    {
        if (r == 1)
        {
            // HoldPanel.gameObject.SetActive(false);
            try
            {
                if (File.Exists(Application.persistentDataPath + "/answer.bin"))
                {
                    File.Delete(Application.persistentDataPath + "/answer.bin");
                }
                PollsConfig.Answers.Clear();
#if UNITY_ANDROID
                Toast.ShowToast("缓存清除成功");
#endif
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
#if UNITY_ANDROID
                Toast.ShowToast("清除缓存失败");
#endif
            }
        }
        else
        {
            // HoldPanel.gameObject.SetActive(false);
        }

    }

    public void OnClearBtnClick()
    {
        messageBox.Show("是否确定清除缓存?", "是", "否", MessageBoxCallback);
    }


    public class answerExportData
    {
        public int id { get; set; }
        public string aid { get; set; }
    }
    public class exportData
    {
        public int HospitalID { get; set; }
        public int DepartmentID { get; set; }
        public int QuestionnaireID { get; set; }
        public string UserID { get; set; }
        public long timestart { get; set; }
        public long timeend { get; set; }
        public List<answerExportData> Answers { get; set; }

    }

    public class exportDataBase64
    {
        public string answers;
        public List<string> pics;
    }


    bool importSuccessed = false;

    void webExportData()
    {
        loadingPanel.gameObject.SetActive(true);
        StartCoroutine(_webExportData());
    }
    List<string> removeList = new List<string>();
    Dictionary<string, List<string>> removeListEx = new Dictionary<string, List<string>>();
    IEnumerator _webExportData(/*exportDataBase64 edb64*/)
    {
        //StartCoroutine(webPostAnswer(edb64.answers, edb64.pics));
        removeList.Clear();
        removeListEx.Clear();
        foreach (KeyValuePair<string, List<PollsConfig.AnswerStorge>> pair in PollsConfig.Answers)
        {
            exportDataBase64 edb64 = new exportDataBase64();
            PollsConfig.AnswerStorge _ans = pair.Value[0];

            //List<Question> qs = QuestionMap[pair.Value[0].department.questionPath];
            exportData ed = null;
            for (int row = 0; row < pair.Value.Count; row++)
            {
                for (int col = 0; col < pair.Value[0].answers.Count + 6; col++)
                {
                    switch (col)
                    {
                        case 0:
                            ed = new exportData();
                            ed.Answers = new List<answerExportData>();
                            break;
                        case 1:
                            if (Application.platform == RuntimePlatform.WindowsEditor)
                            {
                                ed.UserID = pair.Value[row].guid + "_" + "a87f814a75bf11dd" + "_" + pair.Value[row].answers[pair.Value[row].answers.Count - 1].endTime.ToString();
                            }
                            else
                            {
                                ed.UserID = pair.Value[row].guid + "_" + AndroidDeviceInfo.GetAndroidId() + "_" + pair.Value[row].answers[pair.Value[row].answers.Count - 1].endTime.ToShortTimeString();
                            }
                            Debug.Log("length " + ed.UserID + "\n" + ed.UserID.Length);
                            break;
                        case 2:
                            break;
                        case 3:
                            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
                            ed.timestart = (pair.Value[row].answers[0].startTime.Ticks - startTime.Ticks) / 10000;
                            break;
                        case 4:
                            System.DateTime endTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
                            ed.timeend = (pair.Value[row].answers[pair.Value[row].answers.Count - 1].endTime.Ticks - endTime.Ticks) / 10000;
                            break;
                        case 5:
                            break;
                        default:
                            if (pair.Value[row].answers[col - 6].type != 2)
                            {
                                answerExportData aed = new answerExportData();
                                aed.id = pair.Value[row].answers[col - 6].id;
                                aed.aid = pair.Value[row].answers[col - 6].aid;
                                if (aed.aid == null || aed.aid == "")
                                    aed.aid = "-1";
                                ed.Answers.Add(aed);
                            }
                            else
                            {
                                UInt32 t = 0;
                                for (int i = 0; i < PollsConfig.MAX_ANS_COUNT; ++i)
                                {
                                    if (pair.Value[row].answers[col - 6].answers[i] == 1)
                                    {
                                        t += (UInt32)Math.Pow(2, i);
                                    }
                                }
                                answerExportData aed = new answerExportData();
                                aed.id = pair.Value[row].answers[col - 6].id;
                                aed.aid = pair.Value[row].answers[col - 6].aid;
                                if (aed.aid == null || aed.aid == "")
                                    aed.aid = "-1";
                                ed.Answers.Add(aed);

                            }
                            break;
                    }
                }
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                ed.QuestionnaireID = Int32.Parse(_ans.department.questionID);
                //_ans.department.numPeople = 0;
                ed.HospitalID = _ans.hospital.hospitalID;
                ed.DepartmentID = _ans.department.departmentID;
                string json = JsonConvert.SerializeObject(ed);
                byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(json);
                edb64.answers = Convert.ToBase64String(bytes);
                edb64.pics = new List<string>();
                if (Application.version != "2.07.11")
                {
                    for (int i = 0; i < pair.Value[row].photos.Count; ++i)
                    {
                        string imgstring;
                        if (departmentPageController.CaptureData != null && i == 0)
                            imgstring = "data:image / jpg; base64," + Convert.ToBase64String(departmentPageController.CaptureData);
                        else
                            imgstring = "data:image / jpg; base64," + Convert.ToBase64String(pair.Value[row].photos[i]);
                        edb64.pics.Add(imgstring);
                    }
                }
                yield return StartCoroutine(webPostAnswer(edb64.answers, edb64.pics, pair.Key, pair.Value[row].guid));
            }
            
        }

        foreach (string key in removeList)
        {
            PollsConfig.Answers[key][0].department.numPeople = 0;
            //
        }
        List<PollsConfig.AnswerStorge> _removeAstList = new List<PollsConfig.AnswerStorge>();
        foreach(KeyValuePair<string, List<string>> pair in removeListEx)
        {
            foreach(string guid in pair.Value)
            {
                for (int i = PollsConfig.Answers[pair.Key].Count - 1; i >= 0; i--)
                {
                    if (PollsConfig.Answers[pair.Key][i].guid == guid)
                    {
                        PollsConfig.Answers[pair.Key].RemoveAt(i);
                    }
                }
            }
            if (PollsConfig.Answers[pair.Key].Count == 0)
            {
                PollsConfig.Answers.Remove(pair.Key);
            }
        }
        
        if (PollsConfig.Answers.Count == 0)
        {
            Toast.ShowToast(string.Format("导入{0:d}条记录", removeList.Count));
            if (File.Exists(Application.persistentDataPath + "/answer.bin"))
            {
                File.Delete(Application.persistentDataPath + "/answer.bin");
            }
        }
        else
        {
            Toast.ShowToast(string.Format("导入{0:d}条记录, 剩余{1:d}条记录", removeList.Count, PollsConfig.Answers.Count));
            FileStream fs = new FileStream(Application.persistentDataPath + "/" + "answer.bin", FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, PollsConfig.Answers);
            fs.Close();
        }
        PollsConfig.SerializeData();
        loadingPanel.gameObject.SetActive(false);
    }


    public class retWithoutData
    {
        public int ret { get; set; }
        public string info { get; set; }
    }

    IEnumerator webPostAnswer(string edstring, List<string> pics, string key, string guid)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", PollsConfig.netUserID);
        form.AddField("Token", PollsConfig.netUserToken);
        form.AddField("Data", edstring);
        form.AddField("Version", Application.version.ToString());
        if (pics.Count >= 1)
            form.AddField("Pic1", pics[0]);
        if (pics.Count >= 2)
            form.AddField("Pic2", pics[1]);
        if (pics.Count >= 3)
            form.AddField("Pic3", pics[2]);
        if (pics.Count >= 4)
            form.AddField("Pic4", pics[3]);
        WWW www = new WWW("http://47.106.71.112/api/importuseranswer.aspx", form);
        yield return www;
        if (www.isDone && www.error == null && www.text != "")
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            retWithoutData ret = JsonConvert.DeserializeObject<retWithoutData>(www.text, settings);
            if (ret.ret == 1)
            {
                //Toast.ShowToast("导入数据库成功");
                removeList.Add(key);
                if (removeListEx.ContainsKey(key))
                {
                    removeListEx[key].Add(guid);
                }
                else
                {
                    List<string> _list = new List<string>();
                    removeListEx.Add(key, _list);
                    removeListEx[key].Add(guid);
                }
                Debug.Log(ret.info);
            }
            else
            {
                Debug.LogError(ret.info);
                Toast.ShowToast(ret.info);
            }
            www.Dispose();
        }
        else
        {
            Debug.Log("导出错误，请稍后再试");
        }
    }


    void AddNewCell(string name, int num, bool load = false)
    {
        GameObject newone = Instantiate(Resources.Load("ui/MgrAnswerCell") as GameObject);
        if (newone != null)
        {
            newone.transform.SetParent(grid.transform);
            newone.transform.localScale = Vector3.one;
            newone.transform.position = Vector3.zero;
            RectTransform rti = newone.GetComponent<RectTransform>();
            rti.anchoredPosition3D = new Vector3(rti.anchoredPosition3D.x, rti.anchoredPosition3D.y, 0);

            Text nametext = newone.GetComponentInChildren<Text>();
            if (nametext != null)
            {
                nametext.text = name + "  " + "完成 " + num + "次";
                Invoke("_refreshList", 0.1f);
            }
        }
        else
        {
            Debug.Log("Instantiate MgrAnswerCell failed.");
#if UNITY_ANDROID
            Toast.ShowToast("未知错误，请退出后重试");
#endif
        }
    }

    /// <summary>
    /// 刷新列表Grid控件的尺寸
    /// </summary>
    private void _refreshList()
    {
        if (grid != null)
        {
            RectTransform rt = grid.GetComponent<RectTransform>();
            GridLayoutGroup glg = grid.GetComponent<GridLayoutGroup>();

            //if ((_items.Count / glg.constraintCount) * (glg.cellSize.y + glg.spacing.y) > rt.sizeDelta.y) {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, Math.Max(gridOriginHeight, (glg.transform.childCount) * (glg.cellSize.y + glg.spacing.y) - glg.spacing.y));
            //scrollRect.enabled = (rt.sizeDelta.y > gridOriginHeight);
            scrollRect.verticalNormalizedPosition = 1.0f;
            //}
            //Invoke("_scorllToTop", 0.1f);
        }
    }

}
