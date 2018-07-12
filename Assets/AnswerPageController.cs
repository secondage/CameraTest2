﻿using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DeadMosquito.AndroidGoodies;

public class AnswerPageController : MonoBehaviour
{
    public Sprite textAnswerBG;
    public Sprite iconAnswerBG;

    public Button leftButton;
    public Button rightButton;
    public Text selectedHospitalText;
    public Text selectedDeparmentText;

    public GameObject introPage;
    public GameObject holdPanel;
    

    public Text textQuestion;
    public GridLayoutGroup grid;
    public GridLayoutGroup iconGrid;

    public Sprite doneNormalSprite;
    public Sprite donePressSprite;
    public Sprite rightNormalSprite;
    public Sprite rightPressSprite;
    public RawImage webcamRawImage;

    public Image leftAvatarImage;
    public Image rightAvatarImage;

    public Slider progressSlider;

    public Button ReadyBtn;
    public Image ClickImg;
    public Image LoadImg;
    public MessageBox messageBox;
    public Text ProgressText;
    public Text tmpText;
    public Button doneButton;

    static public bool previewMode = false;
    [SerializeField]
    Sprite[] emojiNormalSprite = new Sprite[6];
    [SerializeField]
    Sprite[] emojiDisableSprite = new Sprite[6];
    // Use this for initialization



    List<PollsConfig.Answer> currentAnswer;
    List<byte[]> photoList = null;


    string jsonPath;
    string json;
    IEnumerator LoadWWW()
    {
        yield return new WaitForSeconds(0.033f);
        //WWW www = new WWW(Application.streamingAssetsPath + "/data/ActionConfig.json");
        Debug.Log("read " + jsonPath);
        WWW www = new WWW("file://" + jsonPath);
        yield return www;
        if (www.isDone)
        {
            if (www.error != null)
            {
                Debug.LogError(www.error);
            }
            else
            {
                json = www.text;
                parseJson();
                //DataLoaded = true;
                www.Dispose();
                loaded = true;
                ClickImg.enabled = true;
                ClickImg.color = Color.white;
                //ClickImg.DOFade(0.0f, 0.5f).SetLoops(-1);
                LoadImg.enabled = false;
            }
        }
    }

    bool loaded = false;
    bool[] captured = new bool[4];
    private void parseJson()
    {
        try
        {
            if (!previewMode)
            {
                if (photoList != null)
                {
                    photoList.Clear();
                }
                currentAnswer = new List<PollsConfig.Answer>();
                photoList = new List<byte[]>();
            }
            if (!PollsConfig.QuestionMap.ContainsKey(jsonPath))
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                List<PollsConfig.QuestionParser> configs = JsonConvert.DeserializeObject<List<PollsConfig.QuestionParser>>(json, settings);
                List<PollsConfig.Question> qs = new List<PollsConfig.Question>();

                foreach (PollsConfig.QuestionParser d in configs)
                {
                    PollsConfig.Question q = new PollsConfig.Question();

                    q.id = d.id;
                    q.type = d.type;
                    q.limit = d.limit;
                    q.question = d.text;
                    q.shorttext = d.shorttxt;
                    q.oid = d.originalid != "" ? Int32.Parse(d.originalid) : 0;
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
                    if (!previewMode)
                    {
                        PollsConfig.Answer a = new PollsConfig.Answer();
                        a.limit = q.limit;
                        a.shorttxt = q.shorttext;
                        a.longtxt = "";
                        currentAnswer.Add(a);
                    }
                }

                PollsConfig.QuestionMap.Add(jsonPath, qs);
            }
            else
            {
                if (!previewMode)
                {
                    List<PollsConfig.Question> qs = PollsConfig.QuestionMap[PollsConfig.selectedDepartment.questionID];
                    foreach (PollsConfig.Question q in qs)
                    {
                        PollsConfig.Answer a = new PollsConfig.Answer();
                        a.limit = q.limit;
                        a.shorttxt = q.shorttext;
                        a.longtxt = "";
                        currentAnswer.Add(a);
                    }
                }
            }
            holdPanel.SetActive(false);
            numQuestion = 0;
            if (!previewMode)
            {
                userGuid = Guid.NewGuid().ToString();
                OpenWebCamera();
                CreateTmpFolder();
            }
            StartQuestions();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + "\n" + e.StackTrace);
        }
    }

    private void ReadyToAnswer()
    {
        if (!previewMode)
        {
            currentAnswer = new List<PollsConfig.Answer>();
            photoList = new List<byte[]>();

            foreach (PollsConfig.Question q in PollsConfig.selectedDepartment.questions)
            {

                PollsConfig.Answer a = new PollsConfig.Answer();
                a.limit = q.limit;
                a.shorttxt = q.shorttext;
                a.longtxt = "";
                a.id = q.id;
                a.aid = "-1";
                currentAnswer.Add(a);
            }
           
            
        }
        ClickImg.enabled = true;
        ClickImg.color = Color.white;
        //ClickImg.DOFade(0.0f, 0.5f).SetLoops(-1);
        LoadImg.enabled = false;
        holdPanel.SetActive(false);
        numQuestion = 0;
        if (!previewMode)
        {
            userGuid = Guid.NewGuid().ToString();
            OpenWebCamera();
            CreateTmpFolder();
        }
        loaded = true;
        StartQuestions();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        doneBtnTime = -1.0f;
        /*if (photoList != null)
        {
            photoList.Clear();
            photoList = null;
        }*/
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
        webcamTexture = null;
        doneButton.gameObject.SetActive(false);
        if (PollsConfig.selectedDepartment == null || PollsConfig.selectedHospital == null)
        {
#if UNITY_ANDROID
            Toast.ShowToast("未知错误，请退出后重试");
            this.gameObject.SetActive(false);
            introPage.gameObject.SetActive(true);
#endif
        }
        if (!PollsConfig.selectedDepartment.qusetionLoaded)
        {
#if UNITY_ANDROID
            Toast.ShowToast("未选择题库，请退出后重试");
            this.gameObject.SetActive(false);
            introPage.gameObject.SetActive(true);
#endif
        }

        loaded = false;
        selectedDeparmentText.text = PollsConfig.selectedDepartment.name;
        selectedHospitalText.text = PollsConfig.selectedHospital.name;

        questionAccessed = new int[PollsConfig.selectedDepartment.questions.Count]; //初始化题目访问map，用来记录每一题是否被跳过还是被作答
        frameCaptured = new byte[PollsConfig.selectedDepartment.questions.Count]; //记录每一题是否被拍照
        if (PollsConfig.isOfflineMode)
        {
            holdPanel.SetActive(true);
            jsonPath = PollsConfig.selectedDepartment.questionID;
            //tmpText.text = jsonPath;
            //tmpText.text = jsonPath;
            StartCoroutine(LoadWWW());
            ClickImg.enabled = false;
            LoadImg.enabled = true;
        }
        else
        {
            ReadyToAnswer();
        }

        if (Application.version != "2.07.6")
        {
            ReadyBtn.gameObject.SetActive(true);
        }

    }

    void MessageBoxCallback(int r)
    {
        if (r == 1)
        {
            if (confirmValue == 1) //exit
            {
                this.gameObject.SetActive(false);
                introPage.gameObject.SetActive(true);
            }
            else if (confirmValue == 2) //retry
            {
                parseJson();
            }
            else if (confirmValue == 3)
            {
                doneBtnTime = 2.0f;
                doneButton.gameObject.SetActive(true);
            }
        }
        
    }

    int confirmValue = 1;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (doneBtnTime > 0)
        {
            doneBtnTime -= Time.deltaTime;
            if (doneBtnTime <= 0.0f)
            {
                OnDoneBtnClick();
            }
        }
        LoadImg.transform.Rotate(0, 0, 5);
    }

    int[] questionAccessed;
    int numQuestion = 0;
    string userGuid = "";
    int currentChecked = 0;
    void StartQuestions()
    {
        

        //leftAvatarImage.enabled = (numQuestion % 2 == 0);
        //rightAvatarImage.enabled = (numQuestion % 2 != 0);
        leftAvatarImage.enabled = true;
        rightAvatarImage.enabled = true;
        if (numQuestion % 2 == 0)
        {
            leftAvatarImage.sprite = Resources.Load<Sprite>("avatar/man" + UnityEngine.Random.Range(1, 7));
            leftAvatarImage.SetNativeSize();
        }
        else
        {
            rightAvatarImage.sprite = Resources.Load<Sprite>("avatar/woman" + UnityEngine.Random.Range(1, 7));
            rightAvatarImage.SetNativeSize();
        }
        List<PollsConfig.Question> qs = PollsConfig.QuestionMap[PollsConfig.selectedDepartment.questionID];
        ProgressText.text = (numQuestion + 1) + " / " + qs.Count.ToString();
        progressSlider.value = (float)(numQuestion) / (float)(qs.Count - 1);
        if (qs != null)
        {
            //take photo
            if ((numQuestion == 1 || 
                numQuestion == qs.Count - 1 || 
                numQuestion == (qs.Count - 1) / 3 ||
                numQuestion == (qs.Count - 1) / 3 * 2) && webcamTexture != null)
            {
                if (frameCaptured[numQuestion] == 0)
                {
                    webcamTexture.Play();
                    //webcamTexture.Pause();

                    Invoke("CaptureWebCamTexture", 2.0f);
                    frameCaptured[numQuestion] = 1;
                }
            }
            PollsConfig.Question q = qs[numQuestion];
            if (!previewMode)
            {
                currentAnswer[numQuestion].startTime = DateTime.Now;
            }
            leftButton.gameObject.SetActive(numQuestion != 0);
           
            if (numQuestion == qs.Count - 1)
            {
                rightButton.GetComponent<Image>().sprite = doneNormalSprite;
                SpriteState ss = new SpriteState();
                ss.highlightedSprite = doneNormalSprite;
                ss.pressedSprite = donePressSprite;
                ss.disabledSprite = null;
                rightButton.GetComponent<Button>().spriteState = ss;
                rightButton.gameObject.SetActive(true);
            }
            else
            {
                rightButton.GetComponent<Image>().sprite = rightNormalSprite;
                SpriteState ss = new SpriteState();
                ss.highlightedSprite = rightNormalSprite;
                ss.pressedSprite = rightPressSprite;
                ss.disabledSprite = null;
                rightButton.GetComponent<Button>().spriteState = ss;
                if (!previewMode)
                {
                    //rightButton.gameObject.SetActive(q.limit > 1);  //任何题目可以右转
                }
                else
                {
                    rightButton.gameObject.SetActive(true);
                }
            }
            textQuestion.text = (numQuestion + 1).ToString() + "." + q.question;
            currentChecked = 0;
            if (q.type == 1 || q.type == 2)
            {
                Image bg = GetComponent<Image>();
                bg.sprite = textAnswerBG;
                //count question
                int qcount = 0;
                for (int i = 0; i < grid.transform.childCount; i++)
                {
                    Destroy(grid.transform.GetChild(i).gameObject);
                }
                grid.gameObject.SetActive(true);
                for (int i = 0; i < iconGrid.transform.childCount; i++)
                {
                    Destroy(iconGrid.transform.GetChild(i).gameObject);
                }
                iconGrid.gameObject.SetActive(false);
                for (int i = 0; i < 20; i++)
                {
                    if (q.answers[i] != null && q.answers[i] != "")
                    {
                        qcount++;
                    }
                }
                int c = (int)(qcount / 5.0f + 0.9f);
                int r = qcount % 5;
                if (c > 1)
                    r = 5;
                else if (r == 0)
                    r = c * 5;

                for (int i = 0; i < 20; i++)
                {
                    if (q.answers[i] != null && q.answers[i] != "")
                    {
                        //判断是否有互斥
                        bool found = false;
                        if (PollsConfig.QExcludeMap.ContainsKey(q.oid.ToString()))
                        {
                            //找到idxb
                            PollsConfig.QExclude qe = PollsConfig.QExcludeMap[q.oid.ToString()];
                            foreach (PollsConfig.QExclude _qe in qe.List)
                            {
                                foreach (PollsConfig.Question _q in qs)
                                {
                                    if (_q.oid.ToString() == _qe.idxB)
                                    {
                                        //判断idxb的qidxb是否作答
                                        if (currentAnswer[qs.IndexOf(_q)].answers[_qe.qidxB - 1] != 0 && _qe.qidxA == i + 1)
                                        {
                                            found = true;
                                            break;
                                        }

                                    }
                                }
                            }
                        }
                        if (found)
                            continue;
                        GameObject newone = Instantiate(Resources.Load("ui/AnswerToggle") as GameObject);
                        if (newone != null)
                        {
                            newone.transform.SetParent(grid.transform);
                            newone.transform.localScale = Vector3.one;
                            newone.transform.position = Vector3.zero;
                            RectTransform rti = newone.GetComponent<RectTransform>();
                            rti.anchoredPosition3D = new Vector3(rti.anchoredPosition3D.x, rti.anchoredPosition3D.y, 0);
                            newone.GetComponentInChildren<Text>().text = q.answers[i];
                            if (r == 1)
                                newone.GetComponentInChildren<Text>().fontSize = 50;
                            else if (r == 2)
                                newone.GetComponentInChildren<Text>().fontSize = 40;
                            else if (r == 3)
                                newone.GetComponentInChildren<Text>().fontSize = 32;
                            else if (r == 4)
                                newone.GetComponentInChildren<Text>().fontSize = 30;
                            else if (r == 5)
                                newone.GetComponentInChildren<Text>().fontSize = 26;

                            AnswerBtn ab = newone.GetComponent<AnswerBtn>();
                            ab.index = i;
                            ab.controller = this;
                            if (q.limit == 1)
                            {
                                Toggle toggle = newone.GetComponent<Toggle>();
                                toggle.group = grid.GetComponent<ToggleGroup>();
                            }
                            if (!previewMode)
                            {
                                if (currentAnswer[numQuestion].answers[i] != 0)
                                {
                                    Toggle toggle = newone.GetComponent<Toggle>();
                                    ab.ignoreOnce = true;
                                    toggle.isOn = true;
                                    currentChecked++;
                                }
                            }
                        }

                    }
                }


                float ymax = (grid.GetComponent<RectTransform>().sizeDelta.x - 50) / r * 0.4f;
                ymax = Math.Min(ymax, (grid.GetComponent<RectTransform>().sizeDelta.y - 50) / c);
                float xmax = ymax / 0.4f;
                xmax = Math.Min(xmax, 330);
                grid.cellSize = new Vector2(xmax, xmax * 0.4f);
                //grid.GetComponent<RectTransform>().sizeDelta.y;
            }
            else
            {
                Image bg = GetComponent<Image>();
                bg.sprite = iconAnswerBG;
                for (int i = 0; i < grid.transform.childCount; i++)
                {
                    Destroy(grid.transform.GetChild(i).gameObject);
                }
                grid.gameObject.SetActive(false);
                for (int i = 0; i < iconGrid.transform.childCount; i++)
                {
                    Destroy(iconGrid.transform.GetChild(i).gameObject);
                }
                int qcount = 0;
                iconGrid.gameObject.SetActive(true);
                for (int i = 0; i < 13; i++)
                {
                    if (q.answers[i] != null && q.answers[i] != "")
                    {
                        GameObject newone = Instantiate(Resources.Load("ui/IconAnswerToggle") as GameObject);
                        if (newone != null)
                        {
                            newone.transform.SetParent(iconGrid.transform);
                            newone.transform.localScale = Vector3.one;
                            newone.transform.position = Vector3.zero;
                            RectTransform rti = newone.GetComponent<RectTransform>();
                            rti.anchoredPosition3D = new Vector3(rti.anchoredPosition3D.x, rti.anchoredPosition3D.y, 0);
                            AnswerBtn ab = newone.GetComponent<AnswerBtn>();
                            ab.index = i;
                            ab.controller = this;
                            Toggle toggle = newone.GetComponent<Toggle>();
                            //toggle.spriteState.highlightedSprite = emojiNormalSprite[q.icons[i]];

                            SpriteState ss = new SpriteState();
                            if (q.type == 3)
                            {
                                ss.highlightedSprite = emojiNormalSprite[Int32.Parse(q.icons[i]) - 1];
                                ss.pressedSprite = emojiNormalSprite[Int32.Parse(q.icons[i]) - 1];
                                ss.disabledSprite = null;
                                toggle.spriteState = ss;
                                ab.imageNormal.sprite = emojiNormalSprite[Int32.Parse(q.icons[i]) - 1];
                                ab.imageSelect.sprite = emojiDisableSprite[Int32.Parse(q.icons[i]) - 1];
                                ab.answerText.text = q.answers[i];
                                ab.answerNumber.text = "";
                            }
                            else
                            {
                                ss.highlightedSprite = emojiNormalSprite[5];
                                ss.pressedSprite = emojiNormalSprite[5];
                                ss.disabledSprite = null;
                                toggle.spriteState = ss;
                                ab.imageNormal.sprite = emojiNormalSprite[5];
                                ab.imageSelect.sprite = emojiDisableSprite[5];
                                ab.answerText.text = "";
                                ab.answerNumber.text = q.answers[i];
                            }
                            if (q.limit == 1)
                            {
                                toggle = newone.GetComponent<Toggle>();
                                toggle.group = iconGrid.GetComponent<ToggleGroup>();
                            }
                            if (!previewMode)
                            {
                                if (currentAnswer[numQuestion].answers[i] != 0)
                                {
                                    toggle = newone.GetComponent<Toggle>();
                                    ab.ignoreOnce = true;
                                    toggle.isOn = true;
                                }
                            }
                            qcount++;

                        }

                    }
                }
                iconGrid.cellSize = new Vector2(Math.Min(150 * (6.0f / (float)qcount), 150), Math.Min(150 * (6.0f / (float)qcount), 150) * 1.0f);
                if (qcount == 5 && q.type == 3)
                {
                    iconGrid.spacing = new Vector2(50, 5);
                }
                else
                {
                    iconGrid.spacing = new Vector2(5, 5);
                }
            }
        }
    }


    public void OnLeftBtnClick()
    {
        //numQuestion--;
        for (int i = numQuestion - 1; i >= 0; i--)
        {
            if (questionAccessed[i] >= 1)
            {
                numQuestion = i;
                StartQuestions();
                break;
            }
        }
       
            
    }


    float doneBtnTime = -1.0f;
    public void OnRightBtnClick(int forceid = -1)
    {

        List<PollsConfig.Question> qs = PollsConfig.QuestionMap[PollsConfig.selectedDepartment.questionID];
        if (numQuestion < qs.Count - 1)
        {
            if (previewMode)
            {
                numQuestion++;
                StartQuestions();
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    if (currentAnswer[numQuestion].answers[i] != 0)
                    {
                        currentAnswer[numQuestion].endTime = DateTime.Now;
                       
                        if (forceid == -1)
                        {
                            //numQuestion++;
                            bool found = false;
                            for (int j = numQuestion + 1; j < qs.Count; j++)
                            {
                                if (j == questionAccessed[numQuestion]) //产生跳转的题目的accessed中存放的是跳转目的题目的序号
                                {
                                    numQuestion = j;
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                questionAccessed[numQuestion] = 1;   //此题被作答
                                numQuestion++;
                            }
                               
                        }
                        else
                        {
                            questionAccessed[numQuestion] = forceid; 
                            numQuestion = forceid;
                        }

                        StartQuestions();
                        return;
                    }
                }
#if UNITY_ANDROID
                Toast.ShowToast("请选择一项答案");
#endif
            }
        }
        else
        {
            if (previewMode)
            {
                OnDoneBtnClick();
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    if (currentAnswer[numQuestion].answers[i] != 0)
                    {
                        questionAccessed[numQuestion] = 1; //此题被作答
                        currentAnswer[numQuestion].endTime = DateTime.Now;
                        leftAvatarImage.enabled = false;
                        rightAvatarImage.enabled = false;
                        confirmValue = 3;
                        messageBox.Show("所有题目已经回答完毕，是否结束?", "是", "否", MessageBoxCallback);
                        //doneBtnTime = 2.0f;
                        //doneButton.gameObject.SetActive(true);
                        return;
                    }
                }
#if UNITY_ANDROID
                Toast.ShowToast("请选择一项答案");
#endif
            }
        }
    }


    void _doAnswerBtnCheck()
    {

    }

    private float lastAnswerTime = 0.0f;
    /// <summary>
    /// 设置回答按钮被按下的相应
    /// currentChecked为当前有多少个回答被选中
    /// 如果大于问题设置，则将Toggle设为未选中，注意由于uncheck的时候会
    /// 减去currentChecked的值，所以在设置为未选中之前要先将currentChecked++
    /// </summary>
    /// <param name="toggle"></param>
    /// <param name="idx"></param>
    public void OnAnswerBtnCheck(Toggle toggle, int idx, bool isLoogPress)
    {
        if (!previewMode)
        {
            List<PollsConfig.Question> qs = PollsConfig.QuestionMap[PollsConfig.selectedDepartment.questionID];
            PollsConfig.Question q = qs[numQuestion];
            if (q.limit == 1)
            {
                //if (!isLoogPress)
                if (Application.platform != RuntimePlatform.WindowsEditor && Time.time - lastAnswerTime < 1.3f)
                {
#if UNITY_ANDROID
                    Toast.ShowToast("请仔细阅读题目，谨慎作答，谢谢");
#endif
                    //OnAnswerBtnCheck
                    toggle.isOn = false;
                    return;
                }
                Debug.Log("time is " + (Time.time - lastAnswerTime).ToString());
                lastAnswerTime = Time.time;
                currentAnswer[numQuestion].id = q.id;
                currentAnswer[numQuestion].answers[idx] = 1;
                currentAnswer[numQuestion].answer = (byte)(idx + 1);
                currentAnswer[numQuestion].type = q.type;
                if (currentAnswer[numQuestion].type == 1)
                {
                    currentAnswer[numQuestion].longtxt = (idx + 1).ToString();
                    
                }
                else if (currentAnswer[numQuestion].type == 3)
                {
                    currentAnswer[numQuestion].longtxt = (5 - idx).ToString();
                }
                else if (currentAnswer[numQuestion].type == 4)
                {
                    currentAnswer[numQuestion].longtxt = (idx + 1).ToString();
                }
                currentAnswer[numQuestion].aid = q.aid[idx].ToString();
                if (q.rev1 != "") //判断是跳转还是互斥
                {
                    if (q.rev2 == "") //跳转模式
                    {
                        string[] _params = q.rev1.Split(new char[1] { ',' });
                        int found = -1;
                        for (int i = 0; i < _params.Length; i += 2)
                        {
                            if (_params[i] == (idx + 1).ToString())
                            {
                                found = Int32.Parse(_params[i + 1]);
                                break;
                            }
                        }
                        if (found != -1)
                        {
                            foreach (PollsConfig.Question _q in qs)
                            {
                                if (_q.oid == found)
                                {
                                    /*if (qs.IndexOf(_q) - numQuestion > 1)
                                    {
                                        for (int i = numQuestion + 1; i < qs.IndexOf(_q); ++i)
                                        {
                                            PollsConfig.Question __q = qs[i];
                                            currentAnswer[i].id = __q.id;
                                        }
                                    }*/
                                    OnRightBtnClick(qs.IndexOf(_q));
                                }
                            }
                        }
                        else
                        {
                            questionAccessed[numQuestion] = 1; //若无跳转，则将accessed设置为1
                            OnRightBtnClick();
                        }
                    }
                    else
                    {
                        questionAccessed[numQuestion] = 1; //若无跳转，则将accessed设置为1
                        OnRightBtnClick();
                    }
                }
                else
                    OnRightBtnClick();
            }
            else if (currentChecked < q.limit)
            {
                currentAnswer[numQuestion].answers[idx] = 1;
                currentAnswer[numQuestion].type = q.type;
                currentAnswer[numQuestion].id = q.id;
                currentChecked++;
                currentAnswer[numQuestion].longtxt = "";
                currentAnswer[numQuestion].aid = "";
                for (int i = 0; i < currentAnswer[numQuestion].answers.Length; ++i)
                {
                    if (currentAnswer[numQuestion].answers[i] == 1)
                    {
                        currentAnswer[numQuestion].longtxt += (i + 1).ToString() + ",";
                        currentAnswer[numQuestion].aid += q.aid[i].ToString() + ",";
                    }
                }
                currentAnswer[numQuestion].aid = currentAnswer[numQuestion].aid.Remove(currentAnswer[numQuestion].aid.Length - 1);
            }
            else
            {
#if UNITY_ANDROID
                Toast.ShowToast(string.Format("此题最多能选择{0}个答案", q.limit));
#endif
                currentChecked++;
                toggle.isOn = false;

            }
            Debug.Log(" currentChecked " + currentChecked);
        }
    }

    public void OnAnswerBtnUnCheck(Toggle toggle, int idx)
    {
        if (!previewMode)
        {
            List<PollsConfig.Question> qs = PollsConfig.QuestionMap[PollsConfig.selectedDepartment.questionID];
            PollsConfig.Question q = qs[numQuestion];
            if (q.limit == 1)
            {
                currentAnswer[numQuestion].answers[idx] = 0;
            }
            else
            {
                currentAnswer[numQuestion].answers[idx] = 0;
                currentChecked--;
                currentAnswer[numQuestion].longtxt = "";
                for (int i = 0; i < currentAnswer[numQuestion].answers.Length; ++i)
                {
                    if (currentAnswer[numQuestion].answers[i] == 1)
                    {
                        currentAnswer[numQuestion].longtxt += (i + 1) + "." + q.answers[i] + " ";
                    }
                }
            }
            Debug.Log(" currentChecked " + currentChecked);
        }
    }



    public void OnDoneBtnClick()
    {
        if (!previewMode)
        {
            PollsConfig.selectedDepartment.numPeople++;
            PollsConfig.NumPeoples = PollsConfig.NumPeoples + 1;
            PollsConfig.SerializeData();
            PollsConfig.SaveAnswer(currentAnswer, photoList, userGuid);
        }
        this.gameObject.SetActive(false);
        introPage.gameObject.SetActive(true);
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

    public void OnReadyBtnClick()
    {
        if (loaded)
        {
            //ClickImg.DOKill();
            ReadyBtn.gameObject.SetActive(false);
        }
    }


    public static object ByteToStruct(byte[] bytes, Type type)
    {
        int size = Marshal.SizeOf(type);
        if (size > bytes.Length)
        {
            return null;
        }
        //分配结构体内存空间
        IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将byte数组拷贝到分配好的内存空间
        Marshal.Copy(bytes, 0, structPtr, size);
        //将内存空间转换为目标结构体
        object obj = Marshal.PtrToStructure(structPtr, type);
        //释放内存空间
        Marshal.FreeHGlobal(structPtr);
        return obj;
    }

    public string deviceName;
    //接收返回的图片数据  
    WebCamTexture webcamTexture = null;
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


    void AddMaskOnPhoto(Texture2D texture, string mask)
    {

    }

    Texture2D RotateTexture(Texture2D texture, float eulerAngles)
    {
        int x;
        int y;
        int i;
        int j;
        float phi = eulerAngles / (180 / Mathf.PI);
        float sn = Mathf.Sin(phi);
        float cs = Mathf.Cos(phi);
        Color32[] arr = texture.GetPixels32();
        Color32[] arr2 = new Color32[arr.Length];
        int W = texture.width;
        int H = texture.height;
        int xc = W / 2;
        int yc = H / 2;

        for (j = 0; j < H; j++)
        {
            for (i = 0; i < W; i++)
            {
                arr2[j * W + i] = new Color32(0, 0, 0, 0);

                x = (int)(cs * (i - xc) + sn * (j - yc) + xc);
                y = (int)(-sn * (i - xc) + cs * (j - yc) + yc);

                if ((x > -1) && (x < W) && (y > -1) && (y < H))
                {
                    arr2[j * W + i] = arr[y * W + x];
                }
            }
        }

        Texture2D newImg = new Texture2D(W, H);
        newImg.SetPixels32(arr2);
        newImg.Apply();

        return newImg;
    }

    byte[] frameCaptured = null;
    string tmpPath;
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
                t = RotateTexture(t, 180);
            }
        }

        //距X左的距离        距Y屏上的距离  
        // t.ReadPixels(new Rect(220, 180, 200, 180), 0, 0, false);  
        //t.Apply();
        byte[] byt = t.EncodeToJPG(50);
        if (photoList.Count == 4)
        {
            photoList.RemoveAt(3);
        }
        photoList.Add(byt);
        //File.WriteAllBytes(tmpPath + "/" + numQuestion + ".jpg", byt);
        webcamTexture.Stop();
    }

    void CreateTmpFolder()
    {
        try
        {
            string folderPath = Application.persistentDataPath + "/" + PollsConfig.selectedHospital.name;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            folderPath += "/" + PollsConfig.selectedDepartment.name;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            folderPath += "/" + Path.GetFileNameWithoutExtension(PollsConfig.selectedDepartment.questionID);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            folderPath += "/Photos";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            folderPath += "/" + userGuid;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            tmpPath = folderPath;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
#if UNITY_ANDROID
            Toast.ShowToast("创建临时路径失败");
#endif
        }
    }

    private void OnDisable()
    {
        //photoList.Clear();
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
        webcamTexture = null;
    }

    public void OnReturnBtnClick()
    {
        if (previewMode)
        {
            this.gameObject.SetActive(false);
            introPage.gameObject.SetActive(true);
        }
        else
        {
            confirmValue = 1;
            messageBox.Show("答题尚未完成，是否退出?", "是", "否", MessageBoxCallback);
        }
    }

    public void OnRetryBtnClick()
    {
        if (!previewMode)
        {
            confirmValue = 2;
            messageBox.Show("答题尚未完成，是否重新开始?", "是", "否", MessageBoxCallback);
        }
        else
        {
            parseJson();
        }
    }
}
