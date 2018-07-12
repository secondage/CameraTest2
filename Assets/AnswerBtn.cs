using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnswerBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public int index;
    public AnswerPageController controller;
    public Image imageNormal;
    public Image imagePress;
    public Image imageSelect;
    public Text answerText;
    public Text answerNumber;

    public bool ignoreOnce = false;
    // 延迟时间
    private float delay = 0.3f;

    // 按钮是否是按下状态  
    private bool isDown = false;

    // 按钮最后一次是被按住状态时候的时间  
    private float lastIsDownTime;

    private bool isLongPress = false;

    //public Sprite normalSprite;
    //public Sprite pressSprite;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (isDown)
        {
            // 当前时间 -  按钮最后一次被按下的时间 > 延迟时间0.2秒  
            if (Time.time - lastIsDownTime > delay)
            {
                // 触发长按方法  
                Debug.Log("长按");
                isLongPress = true;
                // 记录按钮最后一次被按下的时间  
                lastIsDownTime = Time.time;

            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
       
        isDown = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
        isDown = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
        isLongPress = false;
        lastIsDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
        
    }

    public void OnValueChange()
    {
        if (ignoreOnce)
        {
            ignoreOnce = false;
            return;
        }
        if (GetComponent<Toggle>().isOn)
        {
            controller.OnAnswerBtnCheck(GetComponent<Toggle>(), index, isLongPress);
        }
        else
        {
            controller.OnAnswerBtnUnCheck(GetComponent<Toggle>(), index);
        }
        // GetComponent<Button>().
        /*if (GetComponent<Image>().sprite == normalSprite)
        {
            GetComponent<Image>().sprite = pressSprite;
            controller.OnAnswerBtnCheck(GetComponent<Button>(), index);
        }
        else
        {
            GetComponent<Image>().sprite = normalSprite;
            controller.OnAnswerBtnUnCheck(GetComponent<Button>(), index);
        }*/
    }
}
