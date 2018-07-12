using DeadMosquito.AndroidGoodies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowAndroidID : MonoBehaviour {
    public Text androididText;
	// Use this for initialization
	void Start () {
#if UNITY_ANDROID
        androididText.text = AndroidDeviceInfo.GetAndroidId();
#endif
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
