using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using DeadMosquito.AndroidGoodies;
using System.Runtime.InteropServices;

public class DepartmentMgrPageController : MonoBehaviour
{
    [SerializeField]
    InputField nameInput;
    [SerializeField]
    GridLayoutGroup grid;
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    Text textHospitalSelected;
    [SerializeField]
    Text textDepartmentSelected;
    [SerializeField]
    Button btnNew;
    [SerializeField]
    Button btnDelete;
    [SerializeField]
    Button btnStart;
    [SerializeField]
    Button btnPreview;
    [SerializeField]
    Button btnLoad;
    [SerializeField]
    GameObject HospitalMgrPage;
    [SerializeField]
    GameObject IntroPage;
    public GameObject answerPage;
    public GameObject loadingPanel;
    public MessageBox messageBox;
    public Text textNumPeople;
    public GameObject HoldPanel;
    public Button btnReflesh;
    public Text textQuestion;
    public GameObject ReadyPanel;
    public RawImage webcamRawImage;
    WebCamTexture webcamTexture = null;

    static public bool selectMode = false;

    public GameObject AnswerPage;

    public FileChooser fileChooser;

    static Dictionary<DepartmentCell, PollsConfig.DepartmentCellInfo> DepartmentCellMap = new Dictionary<DepartmentCell, PollsConfig.DepartmentCellInfo>();
    static DepartmentCell hotDepartmentCell = null;

    private static float gridOriginHeight = 0;
    private int needDownload = 0;

    // Use this for initialization

   

    public class DepartmentListParser
    {
        public DepartmentListParser()
        {

        }
        public int iHospitalID { get; set; }
        public string vcHospitalName { get; set; }
        public int iDepartmentID { get; set; }
        public string vcDepartmentName { get; set; }

    }

    

    void MessageBoxCallback(int r)
    {
        if (r == 1)
        {
           // HoldPanel.gameObject.SetActive(false);
            ConfirmDelDepartment();
        }
        else
        {
           // HoldPanel.gameObject.SetActive(false);
        }

    }
    void Start()
    {
      /*  AndroidNativeController.OnPositiveButtonPressEvent = (message) =>
        {
            HoldPanel.gameObject.SetActive(false);
            ConfirmDelDepartment();
        };
        AndroidNativeController.OnNegativeButtonPressEvent = (message) =>
        {
            HoldPanel.gameObject.SetActive(false);
            // Code whatever you want on click "NO" Button.
        };*/

        // UpdateList();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static byte[] StructToBytes(object structObj, int size)
    {
        byte[] bytes = new byte[size];
        IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将结构体拷到分配好的内存空间
        Marshal.StructureToPtr(structObj, structPtr, false);
        //从内存空间拷贝到byte 数组
        Marshal.Copy(structPtr, bytes, 0, size);
        //释放内存空间
        Marshal.FreeHGlobal(structPtr);
        return bytes;

    }

    public string deviceName;
    void OpenWebCamera()
    {
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            if (webcamTexture == null)
            {
                WebCamDevice[] devices = WebCamTexture.devices;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    deviceName = devices[0].name;
                else
                    deviceName = devices[1].name;
                webcamTexture = new WebCamTexture(deviceName, 320, 240, 12);
                RawImage ri = GetComponent<RawImage>();
                webcamRawImage.texture = webcamTexture;
                webcamTexture.Play();
                //webcamTexture.Pause();
            }
            else
            {
                webcamTexture.Play();
                //webcamTexture.Pause();
            }
        }
    }
    public byte[] CaptureData = null;
    void CaptureWebCamTexture()
    {
        //webcamTexture.Play();
        //yield return new WaitForEndOfFrame();
        webcamTexture.Pause();
        //Texture2D t = new Texture2D(400, 300);
        //t.ReadPixels(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 50, 360, 300), 0, 0, false);

        WebCamTexture wt = (WebCamTexture)webcamRawImage.texture;

        int width = webcamRawImage.texture.width;
        int height = webcamRawImage.texture.height;
        Texture2D t = new Texture2D(width, height, TextureFormat.ARGB32, false);
        // RenderTexture.active = ri.texture;
        Color[] colors = wt.GetPixels();
        byte[] colorbytes = StructToBytes(colors, colors.Length * 4);
        //Array colorarray = new Array();
        /*using (ZipOutputStream s = new ZipOutputStream(File.Create(Application.persistentDataPath + "/Photoes/" + "1.zip")))
        {
            s.SetLevel(5);
            s.Password = "1q2w3e";
            ZipEntry entry = new ZipEntry(Path.GetFileName("block1"));
            entry.DateTime = DateTime.Now;
            s.PutNextEntry(entry);
            s.Write(colorbytes, 0, colorbytes.Length);
            s.Finish();
            s.Close();
        }*/

        t.SetPixels(wt.GetPixels());
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            if (AndroidDeviceInfo.MODEL.Contains("T10(") || AndroidDeviceInfo.MODEL.Contains("t10("))
            {
                //t = RotateTexture(t, 180);
            }
        }

        //距X左的距离        距Y屏上的距离  
        // t.ReadPixels(new Rect(220, 180, 200, 180), 0, 0, false);  
        //t.Apply();
        CaptureData = t.EncodeToJPG(50);
        //File.WriteAllBytes(tmpPath + "/" + numQuestion + ".jpg", byt);
        webcamTexture.Stop();
    }


    private void OnEnable()
    {
        if (Application.version == "2.07.6")
        {
            if (webcamTexture != null)
            {
                webcamTexture.Stop();
            }
            webcamTexture = null;
            OpenWebCamera();

          
        }
        textNumPeople.text = "";
        textQuestion.text = "";
        textHospitalSelected.text = PollsConfig.selectedHospital.name;
        textDepartmentSelected.text = "未选择科室";
        textDepartmentSelected.color = Color.red;
        if (gridOriginHeight == 0)
        {
            gridOriginHeight = grid.GetComponent<RectTransform>().sizeDelta.y;
        }
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            Destroy(grid.transform.GetChild(i).gameObject);
        }
        hotDepartmentCell = null;
        UpdateList();
        btnNew.interactable = selectMode ? false : true;
        btnDelete.interactable = false;
        btnStart.interactable = false;
        btnPreview.interactable = false;
        btnLoad.interactable = false;
        btnNew.gameObject.SetActive(PollsConfig.isOfflineMode);
        btnDelete.gameObject.SetActive(PollsConfig.isOfflineMode);
        btnLoad.gameObject.SetActive(PollsConfig.isOfflineMode);
        btnReflesh.gameObject.SetActive(!PollsConfig.isOfflineMode);
        btnReflesh.interactable = !selectMode;
        PollsConfig.selectedDepartment = null;
        needDownload = 0;
        if (!PollsConfig.isOfflineMode)
        {
            if (PollsConfig.GetDepartmentCellInfoCount(PollsConfig.selectedHospital) == 0)
            {
                RefleshDepartments();
            }
        }
        if (Application.version == "2.07.6")
        {
            if (selectMode)
            {
                ReadyPanel.gameObject.SetActive(true);
                DOTween.To(x =>
                {

                }, 0, 1, 5.0f).OnComplete(() =>
                {
                    webcamTexture.Play();
                    Invoke("CaptureWebCamTexture", 1.5f);
                    ReadyPanel.gameObject.SetActive(false);
                });

            }
        }
    }

    public void UpdateList()
    {
        if (PollsConfig.selectedHospital.departments.Count <= 0)
            return;
        foreach (KeyValuePair<string, PollsConfig.DepartmentCellInfo> pair in PollsConfig.selectedHospital.departments)
        {
            AddNewCell(pair.Key, true);
        }
    }

    private DepartmentCell AddNewCell(string name, bool load = false)
    {
        PollsConfig.DepartmentCellInfo dci = PollsConfig.GetDepartmentCellInfoByName(name);
        if (selectMode && !dci.qusetionLoaded)
            return null;
        GameObject newone = Instantiate(Resources.Load("ui/DepartmentCell") as GameObject);
        if (newone != null)
        {
            newone.transform.SetParent(grid.transform);
            newone.transform.localScale = Vector3.one;
            newone.transform.position = Vector3.zero;
            RectTransform rti = newone.GetComponent<RectTransform>();
            rti.anchoredPosition3D = new Vector3(rti.anchoredPosition3D.x, rti.anchoredPosition3D.y, 0);

            
            if (dci != null)
            {
                DepartmentCell dc = newone.GetComponent<DepartmentCell>();
                dc.controller = this;
                dc.textName.text = dci.name;
                if (dci.qusetionLoaded)
                {
                    dc.SetToReadyStage();
                }
                else
                {
                    dc.SetToNormalStage();
                }
                DepartmentCellMap.Add(dc, dci);
                if (!load)
                {
                    PollsConfig.SerializeData();
                }
                Invoke("_refreshList", 0.1f);
                return dc;
            }
            return null;
        }
        else
        {
            Debug.Log("Instantiate DepartmentCell failed.");
#if UNITY_ANDROID
            Toast.ShowToast("未知错误，请退出后重试");
#endif
            return null;
        }
    }


    IEnumerator webGetQuestionsID(DepartmentCell dc, int departmentid)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", PollsConfig.netUserID);
        form.AddField("Token", PollsConfig.netUserToken);
        form.AddField("DepartmentID", departmentid);
        WWW www = new WWW("http://47.106.71.112/api/getquestionnaires.aspx", form);
        yield return www;
        if (www.isDone && www.error == null && www.text != "")
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            retWithoutData ret = JsonConvert.DeserializeObject<retWithoutData>(www.text, settings);
            if (ret.ret == 1)
            {
                getQidRetWithData data = JsonConvert.DeserializeObject<getQidRetWithData>(www.text, settings);
                //get dci by name
                PollsConfig.DepartmentCellInfo dci = PollsConfig.GetDepartmentCellInfoByName(data.data[0].vcDepartmentName); 
                if (dci != null)
                {
                    dci.title = data.data[0].Title;
                    if (!PollsConfig.QuestionMap.ContainsKey(data.data[0].iQuestionnaireID.ToString()))
                    {
                        needDownload++;
                        loadingPanel.SetActive(true);
                        yield return StartCoroutine(webLoadQuestions(dc, data.data[0].vcDepartmentName, data.data[0].iQuestionnaireID));
                    }
                    else
                    {
                        //PollsConfig.DepartmentCellInfo dci = PollsConfig.GetDepartmentCellInfoByName(data.data[0].vcDepartmentName);
                        dci.questions = PollsConfig.QuestionMap[data.data[0].iQuestionnaireID.ToString()];
                        dci.qusetionLoaded = true;
                        dci.questionID = data.data[0].iQuestionnaireID.ToString();
                        dc.SetToReadyStage();
                        PollsConfig.SerializeData();
                    }
                }
                else
                {
                    Toast.ShowToast("获取科室绑定问卷失败.");
                    Debug.LogError("webGetQuestionsID failed. " + data.data[0].vcDepartmentName);
                }
            }
            else
            {
                Toast.ShowToast(ret.info);
            }
            www.Dispose();
        }
        else
        {
            Debug.Log("There no questions in department " + departmentid);
        }
    }


    IEnumerator webLoadQuestions(DepartmentCell dc, string departmentname, int id)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", PollsConfig.netUserID);
        form.AddField("Token", PollsConfig.netUserToken);
        form.AddField("QuestionnaireID", id);
        WWW www = new WWW("http://47.106.71.112/api/getquestionnaire.aspx", form);
        yield return www;
        if (www.isDone && www.error == null)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            retWithoutData ret = JsonConvert.DeserializeObject<retWithoutData>(www.text, settings);
            if (ret.ret == 1)
            {
                needDownload--;
                if (needDownload == 0)
                {
                    loadingPanel.SetActive(false);
                }
                getQRetWithData data = JsonConvert.DeserializeObject<getQRetWithData>(www.text, settings);
                parseJson(id.ToString(), data.data);
                PollsConfig.DepartmentCellInfo dci = PollsConfig.GetDepartmentCellInfoByName(departmentname);
                dci.questions = PollsConfig.QuestionMap[id.ToString()];
                dci.qusetionLoaded = true;
                dci.questionID = id.ToString();
                dc.SetToReadyStage();
                PollsConfig.SerializeData();
            }
            else
            {
                Toast.ShowToast(ret.info);
            }
            www.Dispose();
        }

    }

    public class getQRetWithData
    {
        public int ret { get; set; }
        public List<PollsConfig.QuestionParser> data { get; set; }
        public string info { get; set; }
    }

    public class QuestionIDParser
    {
        public QuestionIDParser()
        {

        }
        public int iQuestionnaireID { get; set; }
        public string Title { get; set; }
        public string Descr { get; set; }
        public int iDepartmentID { get; set; }
        public string vcDepartmentName { get; set; }
        public int iHospitalID { get; set; }
        public string vcHospitalName { get; set; }
        

    }

    public class getQidRetWithData
    {
        public int ret { get; set; }
        public List<QuestionIDParser> data { get; set; }
        public string info { get; set; }
    }

    public class getDepRetWithData
    {
        public int ret { get; set; }
        public List<DepartmentListParser> data { get; set; }
        public string info { get; set; }
    }

    public class retWithoutData
    {
        public int ret { get; set; }
        public string info { get; set; }
    }
    IEnumerator webGetDepartments(int id)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", PollsConfig.netUserID);
        form.AddField("Token", PollsConfig.netUserToken);
        form.AddField("HospitalID", id);
        WWW www = new WWW("http://47.106.71.112/api/getdepartments.aspx", form);
        yield return www;
        if (www.isDone && www.error == null)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            retWithoutData ret = JsonConvert.DeserializeObject<retWithoutData>(www.text, settings);
            if (ret.ret == 1)
            {
                getDepRetWithData configs = JsonConvert.DeserializeObject<getDepRetWithData>(www.text, settings);
                foreach (DepartmentListParser c in configs.data)
                {
                    int result = PollsConfig.AddDepartment(c.vcDepartmentName, c.iDepartmentID);
                    if (result == -1)
                    {
#if UNITY_ANDROID
                        Toast.ShowToast("科室名已存在");
#endif
                    }
                    else if (result == -2)
                    {
#if UNITY_ANDROID
                        Toast.ShowToast("创建科室失败");
#endif
                    }
                    else if (result == -3)
                    {
#if UNITY_ANDROID
                        Toast.ShowToast("未选择医院");
#endif
                    }
                    else
                    {
                        DepartmentCell dc = AddNewCell(c.vcDepartmentName);
                        if (dc != null)
                        {
                            yield return StartCoroutine(webGetQuestionsID(dc, c.iDepartmentID));
                        }
                    }
                }
            }
            else
            {
                Toast.ShowToast(ret.info);
            }
            //loadingPanel.SetActive(false);
            www.Dispose();
            
        }

    }



    public void OnDepartmentInputNameEnd()
    {
        HoldPanel.gameObject.SetActive(false);
        if (nameInput.text == "")
            return;
        string pattern = @"^[^ ]{2,16}$";
        Regex regex = new Regex(pattern);
        if (!regex.IsMatch(nameInput.text))
        {
#if UNITY_ANDROID
            Toast.ShowToast("科室名称为2-16个字符，且不能存在空格");
#endif
            Debug.LogWarning("科室名称为2-16个字符，且不能存在空格");
            return;
        }
        pattern = @"^[^\/\:\*\?\""\<\>\|\,\.\。\，\？\、\；\“\”]+$";
        regex = new Regex(pattern);
        if (!regex.IsMatch(nameInput.text))
        {
#if UNITY_ANDROID
            Toast.ShowToast("科室名称仅能使用汉字，英文字母，数字");
#endif
            Debug.LogWarning("科室名称仅能使用汉字，英文字母，数字");
            return;
        }
        string name = nameInput.text;
        int result = PollsConfig.AddDepartment(name, 0);
        if (result == -1)
        {
#if UNITY_ANDROID
            Toast.ShowToast("科室名已存在");
#endif
        }
        else if (result == -2)
        {
#if UNITY_ANDROID
            Toast.ShowToast("创建科室失败");
#endif
        }
        else if (result == -3)
        {
#if UNITY_ANDROID
            Toast.ShowToast("未选择医院");
#endif
        }
        else
        {
            AddNewCell(name);
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


    public void RefleshDepartments()
    {
        PollsConfig.DelAllDepartment();
        PollsConfig.ClearQuestionMap();
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            Destroy(grid.transform.GetChild(i).gameObject);
        }
        hotDepartmentCell = null;
        btnNew.interactable = selectMode ? false : true;
        btnDelete.interactable = false;
        btnStart.interactable = false;
        btnPreview.interactable = false;
        btnLoad.interactable = false;
        PollsConfig.selectedDepartment = null;
        loadingPanel.gameObject.SetActive(true);
        needDownload = 0;
        StartCoroutine(webGetDepartments(PollsConfig.selectedHospital.hospitalID));
    }

    public void AddDepartment()
    {
        HoldPanel.gameObject.SetActive(true);
        nameInput.text = "请输入科室名称";
        nameInput.ActivateInputField();
    }


    public void OnClickCell(DepartmentCell cell)
    {
        if (DepartmentCellMap.ContainsKey(cell))
        {
            PollsConfig.DepartmentCellInfo dci = DepartmentCellMap[cell];
            textNumPeople.text = "已完成" + dci.numPeople + "人次";
            textDepartmentSelected.text = "已选择 ：" + dci.name;
            textDepartmentSelected.color = Color.blue;
            if (!PollsConfig.isOfflineMode)
            {
                textQuestion.text = dci.title;
            }
            else
            {
                textQuestion.text = "";
            }
            if (hotDepartmentCell == null)
            {
                hotDepartmentCell = cell;
                hotDepartmentCell.SetToSelectedStage();
            }
            else
            {
                PollsConfig.DepartmentCellInfo hdci = DepartmentCellMap[hotDepartmentCell];
                if (hdci != null)
                {
                    if (hdci.qusetionLoaded)
                    {
                        hotDepartmentCell.SetToReadyStage();
                    }
                    else
                    {
                        hotDepartmentCell.SetToNormalStage();
                    }
                    hotDepartmentCell = cell;
                    hotDepartmentCell.SetToSelectedStage();
                }
            }
            
            btnDelete.interactable = selectMode ? false : true;
            btnStart.interactable = dci.qusetionLoaded;
            btnPreview.interactable = selectMode ? false : dci.qusetionLoaded;
            btnLoad.interactable = selectMode ? false : !dci.qusetionLoaded;
        }
    }

    public void DelDepartment()
    {
        if (hotDepartmentCell == null)
            return;
        else
        {
            /*  if (Application.platform == RuntimePlatform.WindowsEditor)
              {
                  ConfirmDelDepartment();
              }
              else
              {
                  HoldPanel.gameObject.SetActive(true);
  #if UNITY_ANDROID
                  AndroidNativePluginLibrary.Instance.ShowConfirmationDialouge("删除科室", "是否删除科室：" + hotDepartmentCell.textName.text, "是", "否");
  #endif
              }*/
            messageBox.Show("是否删除科室：" + hotDepartmentCell.textName.text, "是", "否", MessageBoxCallback);
        }
    }

    public void ConfirmDelDepartment()
    {
        if (DepartmentCellMap.ContainsKey(hotDepartmentCell))
        {
            textNumPeople.text = "";
            PollsConfig.DelDepartment(DepartmentCellMap[hotDepartmentCell].name);
            DestroyImmediate(hotDepartmentCell.gameObject);
            hotDepartmentCell = null;
            textDepartmentSelected.text = "未选择科室";
            textDepartmentSelected.color = Color.red;
            btnDelete.interactable = false;
            btnStart.interactable = false;
            btnLoad.interactable = false;
            btnPreview.interactable = false;
            PollsConfig.SerializeData();
            Invoke("_refreshList", 0.1f);
        }
    }


    public void BackToHospitalMgrPage()
    {
        fileChooser.gameObject.SetActive(false);
        hotDepartmentCell = null;
        this.gameObject.SetActive(false);
        if (!selectMode)
            HospitalMgrPage.gameObject.SetActive(true);
        else
            IntroPage.gameObject.SetActive(true);

    }

    
   


    public void OnLoadQuestion()
    {
        // EditorUtility.OpenFilePanel("sss", "\\", ".json");
        fileChooser.setup(FileChooser.OPENSAVE.OPEN, "json");
        fileChooser.openSaveButton.GetComponentInChildren<Text>().text = "打开";
        fileChooser.TextTopic.text = "请选择需要打开的题目文件";
        fileChooser.callbackYes = delegate (string filename, string fullname)
        {
            //first hide the filechooser
            fileChooser.gameObject.SetActive(false);
            Debug.Log("select " + fullname);
            /*
            if (PollsConfig.QuestionMap.ContainsKey(fullname))
            {
                if (hotDepartmentCell != null)
                {
                    PollsConfig.DepartmentCellInfo hdci = DepartmentCellMap[hotDepartmentCell];
                    if (hdci != null)
                    {
                        hdci.qusetionLoaded = true;
                        hdci.questionPath = fullname;
                    }
                    PollsConfig.selectedDepartment = hdci;
                    btnPreview.interactable = true;
                    btnStart.interactable = true;
                    PollsConfig.SerializeData();
                }
            }
            else
            {
                //jsonPath = fullname;
                //jsonName = filename;
                //StartCoroutine(LoadWWW());
            }*/

            if (hotDepartmentCell != null)
            {
                PollsConfig.DepartmentCellInfo hdci = DepartmentCellMap[hotDepartmentCell];
                if (hdci != null)
                {
                    hdci.qusetionLoaded = true;
                    hdci.questionID = fullname;
                }
                PollsConfig.selectedDepartment = hdci;
                btnPreview.interactable = selectMode ? false : true;
                btnStart.interactable = true;
                btnLoad.interactable = false;
                PollsConfig.SerializeData();
            }

        };

        fileChooser.callbackNo = delegate ()
        {
            fileChooser.gameObject.SetActive(false);
        };
    }

    public void OnStartClick()
    {
        if (!selectMode)
        {
            PollsConfig.DepartmentCellInfo hdci = DepartmentCellMap[hotDepartmentCell];
            if (hdci != null)
            {
                PollsConfig.selectedDepartment = hdci;
            }
            PollsConfig.SerializeData();
            this.gameObject.SetActive(false);
            IntroPage.gameObject.SetActive(true);
        }
        else
        {
            PollsConfig.DepartmentCellInfo hdci = DepartmentCellMap[hotDepartmentCell];
            if (hdci != null)
            {
                PollsConfig.selectedDepartment = hdci;
            }
            PollsConfig.SerializeData();
            AnswerPageController.previewMode = false;
            this.gameObject.SetActive(false);
            answerPage.gameObject.SetActive(true);
        }
    }

    public void OnPreviewClick()
    {
        PollsConfig.DepartmentCellInfo hdci = DepartmentCellMap[hotDepartmentCell];
        if (hdci != null)
        {
            PollsConfig.selectedDepartment = hdci;
        }
        AnswerPageController.previewMode = true;
        //this.gameObject.SetActive(false);
        AnswerPage.gameObject.SetActive(true);
    }


    

    //private void parseJson(string guid, string json)
    private void parseJson(string guid, List<PollsConfig.QuestionParser> configs)
    {
        try
        {
            PollsConfig.QExcludeMap.Clear();
            List<PollsConfig.Question> qs = new List<PollsConfig.Question>();

            foreach (PollsConfig.QuestionParser d in configs)
            {
                PollsConfig.Question q = new PollsConfig.Question();
                q.id = d.id;
                q.oid = d.originalid != "" ? Int32.Parse(d.originalid) : 0;
                q.type = d.type;
                q.limit = d.limit;
                q.question = d.text;
                q.shorttext = d.shorttxt;
                q.rev1 = d.rev1;
                q.rev2 = d.rev2;
                if (q.rev1 != "") //判断是跳转还是互斥
                {
                    if (q.rev2 != "") //跳转模式
                    {
                        try
                        {
                            string[] _params = q.rev1.Split(new char[1] { ',' });
                            string[] _params2 = q.rev2.Split(new char[1] { ',' });
                            for (int i = 0; i < _params.Length; i += 2)
                            {
                                PollsConfig.QExclude qe = new PollsConfig.QExclude();
                                qe.idxA = _params[i];
                                qe.idxB = _params2[i];
                                qe.qidxA = Int32.Parse(_params[i + 1]);
                                qe.qidxB = Int32.Parse(_params2[i + 1]);
                                PollsConfig.QExclude qe2 = new PollsConfig.QExclude();
                                qe2.idxA = _params2[i];
                                qe2.idxB = _params[i];
                                qe2.qidxA = Int32.Parse(_params2[i + 1]);
                                qe2.qidxB = Int32.Parse(_params[i + 1]);
                                if (PollsConfig.QExcludeMap.ContainsKey(qe.idxA))
                                {
                                    PollsConfig.QExcludeMap[qe.idxA].List.Add(qe);
                                }
                                else
                                {
                                    PollsConfig.QExclude _qe = new PollsConfig.QExclude();
                                    _qe.List.Add(qe);
                                    PollsConfig.QExcludeMap.Add(qe.idxA, _qe);
                                }
                                if (PollsConfig.QExcludeMap.ContainsKey(qe.idxB))
                                {
                                    PollsConfig.QExcludeMap[qe.idxB].List.Add(qe2);
                                }
                                else
                                {
                                    PollsConfig.QExclude _qe = new PollsConfig.QExclude();
                                    _qe.List.Add(qe2);
                                    PollsConfig.QExcludeMap.Add(qe.idxB, _qe);
                                }
                            }
                        }
                        catch
                        {
                            Toast.ShowToast("解析互斥题目失败, 请检查题库");
                        }
                    }
                }
                q.indeices[0] = d.index1;
                q.indeices[1] = d.index2;
                q.indeices[2] = d.index3;
                q.indeices[3] = d.index4;
                q.indeices[4] = d.index5;
                q.answers[0] = d.answer1;
                q.answers[1] = d.answer2;
                q.answers[2] = d.answer3;
                q.answers[3] = d.answer4;
                q.answers[4] = d.answer5;
                q.answers[5] = d.answer6;
                q.answers[6] = d.answer7;
                q.answers[7] = d.answer8;
                q.answers[8] = d.answer9;
                q.answers[9] = d.answer10;
                q.answers[10] = d.answer11;
                q.answers[11] = d.answer12;
                q.answers[12] = d.answer13;
                q.answers[13] = d.answer14;
                q.answers[14] = d.answer15;
                q.answers[15] = d.answer16;
                q.answers[16] = d.answer17;
                q.answers[17] = d.answer18;
                q.answers[18] = d.answer19;
                q.answers[19] = d.answer20;
                q.aid[0] = d.aid1;
                q.aid[1] = d.aid2;
                q.aid[2] = d.aid3;
                q.aid[3] = d.aid4;
                q.aid[4] = d.aid5;
                q.aid[5] = d.aid6;
                q.aid[6] = d.aid7;
                q.aid[7] = d.aid8;
                q.aid[8] = d.aid9;
                q.aid[9] = d.aid10;
                q.aid[10] = d.aid11;
                q.aid[11] = d.aid12;
                q.aid[12] = d.aid13;
                q.aid[13] = d.aid14;
                q.aid[14] = d.aid15;
                q.aid[15] = d.aid16;
                q.aid[17] = d.aid17;
                q.aid[18] = d.aid18;
                q.aid[19] = d.aid19;
                q.icons[0] = d.icon1;
                q.icons[1] = d.icon2;
                q.icons[2] = d.icon3;
                q.icons[3] = d.icon4;
                q.icons[4] = d.icon5;
                q.icons[5] = d.icon6;
                q.icons[6] = d.icon7;
                q.icons[7] = d.icon8;
                q.icons[8] = d.icon9;
                q.icons[9] = d.icon10;
                q.icons[10] = d.icon11;
                q.icons[11] = d.icon12;
                qs.Add(q);
            }
            PollsConfig.QuestionMap.Add(guid, qs);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + "\n" + e.StackTrace);
        }
    }
}
