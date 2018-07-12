using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Specialized;
using System.Text;
using DG.Tweening;
using DeadMosquito.AndroidGoodies;
using Newtonsoft.Json;

public class PollsConfig : MonoBehaviour
{
    [Serializable]
    public class HospitalCellInfo
    {
        public Guid projectID;        //项目id
        public int hospitalID;
        public string name;     //医院名称
        public DateTime createTime;          //创建时间
        public Dictionary<string, DepartmentCellInfo> departments = new Dictionary<string, DepartmentCellInfo>();
    };
    [Serializable]
    public class DepartmentCellInfo
    {
        public int departmentID;
        public HospitalCellInfo hospital;
        public string name;     //科室名称
                                // public string questionPath; //题目文件路径
        public string title; //问卷描述
        public string questionID;
        public List<Question> questions;
        public int numPeople;
        public bool qusetionLoaded = false;
    };
    [Serializable]
    public class Question
    {
        public int id;
        public int oid;
        public int type;
        public int limit;
        public string question;
        public string shorttext;
        public string rev1;
        public string rev2;
        public string[] indeices = new string[5];
        public string[] answers = new string[20];
        public int[] aid = new int[20];
        public string[] icons = new string[12];
    };

    public class QuestionBlock
    {
        public HospitalCellInfo hospitalInfo;
        public DepartmentCellInfo departmentInfo;
        public Dictionary<string, List<Question>> questionMap;
    }

    [Serializable]
    public class Answer
    {
        public DateTime startTime;
        public DateTime endTime;
        public string shorttxt;
        public string longtxt;
        public int id;
        public int type;
        public int limit;
        public int questionIdx;
        public string aid;
        public byte[] answers = new byte[20];
        public byte answer;
    };
    [Serializable]
    public class AnswerStorge
    {
        public string guid;
        public HospitalCellInfo hospital;
        public DepartmentCellInfo department;
        public List<Answer> answers;
        public List<byte[]> photos;
    }

    public class QuestionParser
    {
        public QuestionParser()
        {

        }
        public int id { get; set; }
        public string originalid { get; set; }
        public int type { get; set; }
        public string text { get; set; }
        public string shorttxt { get; set; }
        public string rev1 { get; set; }
        public string rev2 { get; set; }
        public string index1 { get; set; }
        public string index2 { get; set; }
        public string index3 { get; set; }
        public string index4 { get; set; }
        public string index5 { get; set; }
        public int limit { get; set; }
        public int aid1 { get; set; }
        public string answer1 { get; set; }
        public int aid2 { get; set; }
        public string answer2 { get; set; }
        public int aid3 { get; set; }
        public string answer3 { get; set; }
        public int aid4 { get; set; }
        public string answer4 { get; set; }
        public int aid5 { get; set; }
        public string answer5 { get; set; }
        public int aid6 { get; set; }
        public string answer6 { get; set; }
        public int aid7 { get; set; }
        public string answer7 { get; set; }
        public int aid8 { get; set; }
        public string answer8 { get; set; }
        public int aid9 { get; set; }
        public string answer9 { get; set; }
        public int aid10 { get; set; }
        public string answer10 { get; set; }
        public int aid11 { get; set; }
        public string answer11 { get; set; }
        public int aid12 { get; set; }
        public string answer12 { get; set; }
        public int aid13 { get; set; }
        public string answer13 { get; set; }
        public int aid14 { get; set; }
        public string answer14 { get; set; }
        public int aid15 { get; set; }
        public string answer15 { get; set; }
        public int aid16 { get; set; }
        public string answer16 { get; set; }
        public int aid17 { get; set; }
        public string answer17 { get; set; }
        public int aid18 { get; set; }
        public string answer18 { get; set; }
        public int aid19 { get; set; }
        public string answer19 { get; set; }
        public int aid20 { get; set; }
        public string answer20 { get; set; }
        public string icon1 { get; set; }
        public string icon2 { get; set; }
        public string icon3 { get; set; }
        public string icon4 { get; set; }
        public string icon5 { get; set; }
        public string icon6 { get; set; }
        public string icon7 { get; set; }
        public string icon8 { get; set; }
        public string icon9 { get; set; }
        public string icon10 { get; set; }
        public string icon11 { get; set; }
        public string icon12 { get; set; }
    }

    static public readonly bool isOfflineMode = false;
    static public Dictionary<string, List<AnswerStorge>> Answers = new Dictionary<string, List<AnswerStorge>>();

    static public Dictionary<string, List<Question>> QuestionMap = new Dictionary<string, List<Question>>();

    static public HospitalCellInfo selectedHospital = null; //当前医院
    static public DepartmentCellInfo selectedDepartment = null; //当前科室

    public class QExclude
    {
        public List<QExclude> List = new List<QExclude>();
        public string idxA;
        public string idxB;
        public int qidxA;
        public int qidxB;

    }
    static public Dictionary<string, QExclude> QExcludeMap = new Dictionary<string, QExclude>();

    static public Dictionary<string, HospitalCellInfo> Hospitals = new Dictionary<string, HospitalCellInfo>();
    public static readonly int MAX_ANS_COUNT = 20;

    static public string netUserToken;
    static public int netUserID;
    static public int netExpireTime;

    static public string GetMD5(string msg)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
        byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
        md5.Clear();

        string destString = "";
        for (int i = 0; i < md5Data.Length; i++)
        {
            destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
        }
        destString = destString.PadLeft(32, '0');
        return destString;
    }

    static public string Password
    {
        get
        {
            return PlayerPrefs.GetString("Password");
        }

        set
        {
            PlayerPrefs.SetString("Password", GetMD5(value));
            PlayerPrefs.Save();
        }
    }

    static public string CurrentHospital { get; set; }
    static public string CurrentDepartment { get; set; }
    static public int NumPeoples
    {
        get
        {
            return PlayerPrefs.GetInt("NumPeoples");
        }
        set
        {
            PlayerPrefs.SetInt("NumPeoples", value);
            PlayerPrefs.Save();
        }
    }

    static public HospitalCellInfo GetHospitalCellInfoByName(string name)
    {
        if (!Hospitals.ContainsKey(name))
        {
            return null; //rlready exist
        }
        return Hospitals[name];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns>0="succeeded"</returns>
    static public int AddHospitals(string name, int id)
    {
        if (Hospitals.ContainsKey(name))
        {
            return -1; //rlready exist
        }
        try
        {
            HospitalCellInfo hc = new HospitalCellInfo
            {
                name = name,
                projectID = Guid.NewGuid(),
                createTime = DateTime.Now,
                hospitalID = id
            };
            if (/*AddDepartment("通用科室", hc) == 0*/true)
            {
                Hospitals.Add(name, hc);
                return 0;
            }
            else
            {
                hc = null;
                return -2;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return -2;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    static public void DelAllHospital()
    {
        Hospitals.Clear();
        selectedHospital = null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    static public void DelHospital(string name)
    {
        if (selectedHospital != null)
        {
            return;
        }
        if (Hospitals.ContainsKey(name))
        {
            Hospitals.Remove(name);
        }
        selectedHospital = null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <param name="hci"></param>
    /// <returns></returns>
    static public int AddDepartment(string name, int id, HospitalCellInfo hci = null)
    {
        if (hci == null && selectedHospital == null)
        {
            return -3; //hopstial not selected
        }
        if (hci != null && hci.departments.ContainsKey(name))
        {
            return -1; //rlready exist
        }
        else if (selectedHospital != null && selectedHospital.departments.ContainsKey(name))
        {
            return -1; //rlready exist
        }
        try
        {
            DepartmentCellInfo dci = new DepartmentCellInfo
            {
                name = name,
                questionID = "",
                hospital = hci != null ? hci : selectedHospital,
                departmentID = id
            };
            if (hci != null)
            {
                hci.departments.Add(name, dci);
            }
            else
            {
                selectedHospital.departments.Add(name, dci);
            }
            return 0;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return -2;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    static public void ClearQuestionMap()
    {
        QuestionMap.Clear();
    }
    /// <summary>
    /// 
    /// </summary>
    static public void DelAllDepartment()
    {
        if (selectedHospital == null)
        {
            return;
        }
        selectedHospital.departments.Clear();
        selectedDepartment = null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    static public void DelDepartment(string name)
    {
        if (selectedHospital == null)
        {
            return;
        }
        if (selectedHospital.departments.ContainsKey(name))
        {
            selectedHospital.departments.Remove(name);
        }
        selectedDepartment = null;
    }

    static public int GetDepartmentCellInfoCount(HospitalCellInfo hci)
    {
        if (hci != null && hci.departments != null)
        {
            return hci.departments.Count;
        }
        return 0;
    }

    static public DepartmentCellInfo GetDepartmentCellInfoByName(string name, HospitalCellInfo hci = null)
    {
        if (selectedHospital == null && hci == null)
        {
            return null;
        }
        if (selectedHospital != null)
        {
            if (!selectedHospital.departments.ContainsKey(name))
                return null; //rlready exist
            else
                return selectedHospital.departments[name];
        }
        if (hci != null)
        {
            if (!hci.departments.ContainsKey(name))
                return null; //rlready exist
            else
                return hci.departments[name];
        }
        return null;
    }

    static public void SerializeData()
    {
        FileStream fs = new FileStream(Application.persistentDataPath + "/hospital.bin", FileMode.OpenOrCreate);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, Hospitals);
        fs.Close();
        if (selectedHospital != null)
        {
            PlayerPrefs.SetString("SelectedHospital", selectedHospital.name);
            PlayerPrefs.Save();
        }
        if (selectedDepartment != null)
        {
            PlayerPrefs.SetString("SelectedDepartment", selectedDepartment.name);
            PlayerPrefs.Save();
        }
    }

    static public void UnserializeData()
    {
        if (System.IO.File.Exists(Application.persistentDataPath + "/hospital.bin"))
        {
            FileStream fs = new FileStream(Application.persistentDataPath + "/hospital.bin", FileMode.Open);
            if (fs != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                Hospitals = bf.Deserialize(fs) as Dictionary<string, HospitalCellInfo>;
                if (!isOfflineMode)
                {
                    QExcludeMap.Clear();
                    foreach (KeyValuePair<string, HospitalCellInfo> pair in Hospitals)
                    {
                        foreach (KeyValuePair<string, DepartmentCellInfo> dci in pair.Value.departments)
                        {
                            if (dci.Value.qusetionLoaded)
                            {
                                QuestionMap[dci.Value.questionID] = dci.Value.questions;
                                foreach (Question q in dci.Value.questions)
                                {
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
                                                    if (QExcludeMap.ContainsKey(qe.idxA))
                                                    {
                                                        QExcludeMap[qe.idxA].List.Add(qe);
                                                    }
                                                    else
                                                    {
                                                        QExclude _qe = new QExclude();
                                                        _qe.List.Add(qe);
                                                        QExcludeMap.Add(qe.idxA, _qe);
                                                    }
                                                    if (QExcludeMap.ContainsKey(qe.idxB))
                                                    {
                                                        QExcludeMap[qe.idxB].List.Add(qe2);
                                                    }
                                                    else
                                                    {
                                                        QExclude _qe = new QExclude();
                                                        _qe.List.Add(qe2);
                                                        QExcludeMap.Add(qe.idxB, _qe);
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                Toast.ShowToast("解析互斥题目失败, 请检查题库");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                fs.Close();
            }
        }
        if (PlayerPrefs.GetString("SelectedHospital") != "")
        {
            selectedHospital = GetHospitalCellInfoByName(PlayerPrefs.GetString("SelectedHospital"));
        }
        if (PlayerPrefs.GetString("SelectedDepartment") != "")
        {
            selectedDepartment = GetDepartmentCellInfoByName(PlayerPrefs.GetString("SelectedDepartment"));
        }
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {
        Screen.sleepTimeout = (int)SleepTimeout.NeverSleep;

        MgrPageController.cameraAuthed = PlayerPrefs.GetInt("cameraauth");
        if (Application.platform == RuntimePlatform.Android)
        {
            // Screen.SetResolution(1280, 800, true);
        }

        Debug.Log(Application.persistentDataPath);
        DOTween.Init(false, false, LogBehaviour.ErrorsOnly);
        DOTween.useSafeMode = false;
        DOTween.useSmoothDeltaTime = false;
        DOTween.defaultUpdateType = UpdateType.Late;
        Application.targetFrameRate = 30;
        UnserializeData();
        LoadAnswer();
    }

    static bool isAppPause = false;
    static bool isAppFocus = false;
    private void OnApplicationPause(bool pause)
    {
        isAppPause = pause;
    }

    private void OnApplicationFocus(bool focus)
    {
        isAppFocus = focus;
        if (isAppPause && !isAppFocus)
        {
            // Application.Quit();
        }
    }

    static public void LoadAnswer()
    {
        if (System.IO.File.Exists(Application.persistentDataPath + "/answer.bin"))
        {
            FileStream fs = new FileStream(Application.persistentDataPath + "/answer.bin", FileMode.Open);
            if (fs != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                Answers = bf.Deserialize(fs) as Dictionary<string, List<AnswerStorge>>;
                fs.Close();

                /*FileStream fs2 = new FileStream(Application.persistentDataPath + "/" + "answer2.bin", FileMode.OpenOrCreate);
                BinaryFormatter bf2 = new BinaryFormatter();
                bf2.Serialize(fs2, Answers);
                fs.Close();*/
            }
        }

    }

    public static string get_ascii(string unicodeString)
    {


        byte[] Buff = System.Text.Encoding.Unicode.GetBytes(unicodeString);
        string retStr = System.Text.Encoding.ASCII.GetString(Buff, 0, Buff.Length);
        return retStr;
    }

   
    public void ExportData(string path)
    {
        try
        {
            //AnswerStorge ans = null;       
            foreach (KeyValuePair<string, List<AnswerStorge>> pair in Answers)
            {
                ES2Spreadsheet sheet = new ES2Spreadsheet();
                AnswerStorge _ans = pair.Value[0];
                //统计有多少个单选
                /*int numType2 = 0;
                for (int i = 0; i < pair.Value[0].answers.Count; i++)
                {
                    if (pair.Value[0].answers[i].type == 2)
                        numType2++;
                }*/
                // Add data to cells in the spreadsheet.

                //List<Question> qs = QuestionMap[pair.Value[0].department.questionPath];
                for (int row = 0; row < pair.Value.Count + 1; row++)
                {
                    int tcol = 0;
                    for (int col = 0; col < pair.Value[0].answers.Count + 6; col++, tcol++)
                    {
                        if (row == 0)
                        {
                            switch (col)
                            {
                                case 0:
                                    sheet.SetCell(tcol, row, "设备id");
                                    break;
                                case 1:
                                    sheet.SetCell(tcol, row, "病人id");
                                    break;
                                case 2:
                                    sheet.SetCell(tcol, row, "调查科室");
                                    break;
                                case 3:
                                    sheet.SetCell(tcol, row, "调查开始时间");
                                    break;
                                case 4:
                                    sheet.SetCell(tcol, row, "调查结束时间");
                                    break;
                                case 5:
                                    sheet.SetCell(tcol, row, "耗时（秒)");
                                    break;
                                default:
                                    if (pair.Value[0].answers[col - 6].type == 2)
                                    {
                                        sheet.SetCell(tcol, row, (col - 5).ToString() + "." + pair.Value[0].answers[col - 6].shorttxt);
                                        sheet.SetCell(tcol + 1, row, (col - 5).ToString() + "." + pair.Value[0].answers[col - 6].shorttxt);
                                        sheet.SetCell(tcol + 2, row, (col - 5).ToString() + "." + pair.Value[0].answers[col - 6].shorttxt);
                                        tcol += 2;
                                    }
                                    else
                                    {
                                        sheet.SetCell(tcol, row, (col - 5).ToString() + "." + pair.Value[0].answers[col - 6].shorttxt);
                                    }
                                    break;
                            }
                        }
                        else
                        {

                            switch (col)
                            {
                                case 0:
                                    if (AndroidDeviceInfo.SERIAL == null)
                                        sheet.SetCell(tcol, row, "dummy");
                                    else
                                        sheet.SetCell(tcol, row, AndroidDeviceInfo.SERIAL);
                                    break;
                                case 1:
                                    sheet.SetCell(tcol, row, pair.Value[row - 1].guid);
                                    break;
                                case 2:
                                    sheet.SetCell(tcol, row, pair.Value[row - 1].department.name);
                                    break;
                                case 3:
                                    sheet.SetCell(tcol, row, pair.Value[row - 1].answers[0].startTime.ToShortDateString().ToString() + " " +
                                       pair.Value[row - 1].answers[0].startTime.ToShortTimeString().ToString());
                                    break;
                                case 4:
                                    sheet.SetCell(tcol, row, pair.Value[row - 1].answers[pair.Value[row - 1].answers.Count - 1].endTime.ToShortDateString().ToString() + " " +
                                       pair.Value[row - 1].answers[pair.Value[row - 1].answers.Count - 1].endTime.ToShortTimeString().ToString());
                                    break;
                                case 5:
                                    sheet.SetCell(tcol, row, (pair.Value[row - 1].answers[pair.Value[row - 1].answers.Count - 1].endTime - pair.Value[row - 1].answers[0].startTime).TotalSeconds);
                                    break;
                                default:
                                    if (pair.Value[row - 1].answers[col - 6].type != 2)
                                    {
                                        sheet.SetCell(tcol, row, pair.Value[row - 1].answers[col - 6].longtxt);
                                    }
                                    else
                                    {
                                        UInt32 t = 0;
                                        for (int i = 0; i < MAX_ANS_COUNT; ++i)
                                        {
                                            if (pair.Value[row - 1].answers[col - 6].answers[i] == 1)
                                            {
                                                t += (UInt32)Math.Pow(2, i);
                                            }
                                        }
                                        //sheet.SetCell(col, row, t.ToString());
                                        string[] sArray = pair.Value[row - 1].answers[col - 6].longtxt.Split(',');
                                        if (sArray.Length == 4)
                                        {
                                            sheet.SetCell(tcol, row, sArray[0]);
                                            sheet.SetCell(tcol + 1, row, sArray[1]);
                                            sheet.SetCell(tcol + 2, row, sArray[2]);
                                        }
                                        else if (sArray.Length == 3)
                                        {
                                            sheet.SetCell(tcol, row, sArray[0]);
                                            sheet.SetCell(tcol + 1, row, sArray[1]);
                                            sheet.SetCell(tcol + 2, row, "");
                                        }
                                        else if (sArray.Length == 2)
                                        {
                                            sheet.SetCell(tcol, row, sArray[0]);
                                            sheet.SetCell(tcol + 1, row, "");
                                            sheet.SetCell(tcol + 2, row, "");
                                        }
                                        tcol += 2;
                                    }
                                    break;
                            }
                        }
                    }
                }

               

                string _folderPath = path + "/" + _ans.hospital.name;
                _folderPath += "/" + _ans.department.name;
                _folderPath += "/" + Path.GetFileNameWithoutExtension(_ans.department.questionID);
                sheet.Save(_folderPath + "/" + "output.csv");
                foreach (AnswerStorge ans in pair.Value)
                {
                    for (int i = 0; i < ans.photos.Count; ++i)
                    {
                        string imgstring = "data:image / jpg; base64," + Convert.ToBase64String(ans.photos[i]);
                        File.WriteAllBytes(_folderPath + "/" + ans.guid + "-" + (i + 1).ToString() + ".jpg", ans.photos[i]);

                    }
                }
#if UNITY_ANDROID
                //Toast.ShowToast("导出数据成功，导出路径为 ：" + _folderPath);
#endif

            }


            //SimpleExport_ScoreCSV.ExportCSV()
        }
        catch (Exception e)
        {
#if UNITY_ANDROID
            Toast.ShowToast("导出本地数据错误，请退出后重试");
#endif
            Debug.LogError(e.Message);
        }
    }


    static public void SaveAnswer(List<Answer> answers, List<byte[]> photos, string guid)
    {
        try
        {

            AnswerStorge ans = new AnswerStorge();
            ans.hospital = selectedHospital;
            ans.department = selectedDepartment;
            ans.answers = answers;
            ans.photos = new List<byte[]>();
            foreach (byte[] d in photos)
            {
                ans.photos.Add(d);
            }
            // photos.CopyTo(ans.photos);
            //ans.photos = photos;
            ans.guid = guid;
            string hash = ans.hospital.name + "%" + ans.department.name + "%" + Path.GetFileNameWithoutExtension(selectedDepartment.questionID);
            List<AnswerStorge> slist = null;
            if (Answers.ContainsKey(hash))
            {
                slist = Answers[hash];
            }
            else
            {
                slist = new List<AnswerStorge>();
                Answers[hash] = slist;
            }
            slist.Add(ans);
            string folderPath = Application.persistentDataPath + "/" + ans.hospital.name;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            folderPath += "/" + ans.department.name;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            folderPath += "/" + Path.GetFileNameWithoutExtension(selectedDepartment.questionID);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            FileStream fs = new FileStream(folderPath + "/" + "answer.bin", FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, slist);
            fs.Close();
            fs = new FileStream(Application.persistentDataPath + "/" + "answer.bin", FileMode.OpenOrCreate);
            bf = new BinaryFormatter();
            bf.Serialize(fs, Answers);
            fs.Close();


        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
