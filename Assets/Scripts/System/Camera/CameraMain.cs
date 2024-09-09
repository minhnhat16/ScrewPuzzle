using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMain : MonoBehaviour
{
    public static CameraMain instance;
    public Camera main;
    public GameObject camPrefab;
    public float height;
    public float width;
    public GameObject _obj;
    private const float baseAspect = 9f / 16f;
    public float rate;
    public virtual  void Awake()
    {
        instance = this;
    }
    public virtual void Start()
    {
        rate = baseAspect;
        GetCamera();
        GetCameraAspect();
    }
    public void GetCamera()
    {
        if (_obj == null)
        {
            _obj = Instantiate(Resources.Load("Prefabs/Camera/MainCamera", typeof(GameObject)),transform) as GameObject;
            _obj.transform.SetParent(transform.parent);
        }
        main = _obj.GetComponent<Camera>();
    }
    public Camera GetCam()
    {   GetCamera();
        return main;
    }
    public void GetCameraAspect()
    {
        float targetAspect = main.aspect;
        main.orthographicSize = baseAspect / targetAspect * main.orthographicSize;
        height = main.orthographicSize * 2;
        width = height * main.aspect;
    }
    public float GetLeft()
    {
        return main.transform.position.x - width * 0.5f;
    }
    public float GetRight()
    {
        return main.transform.position.x + width * 0.5f;
    }
    public float GetTop()
    {
        return main.transform.position.y + height* 0.5f;
    }
    public float GetBottom()
    {
        return main.transform.position.y - height * 0.5f;
    }
}
