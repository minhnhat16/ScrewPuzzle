using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public Slot fromSlot;
    public Slot toSlot;

    public float duration;
    public float delay;
    public float height;
    public Ease ease;

    public int totalGold;
    public int totalGem;
    public int playerLevel; //Add data to data model

    public float timeDisableCard;
    public bool isAnimPlaying;
    public float cardPositionOffsetY = 0.075f;
    public float cardPositionOffsetZ = -0.025f;

    public bool isDealBtnActive;
    private string mode;
    bool isSimulationMode;

    private void Awake()
    {
        Instance = this;
    }
    IEnumerator Start()
    {
        yield return new WaitUntil(() => SlotCamera.Instance.S_Camera != null);
        string[] args = Environment.GetCommandLineArgs();
        mode = "game";
        foreach (string arg in args)
        {
            if (arg.StartsWith("--mode="))
            {
                mode = arg.Split('=')[1];
            }
        }
        if (mode == "game")
        {
            isSimulationMode = false;

        }
        else if (mode == "simulator")
        {
            isSimulationMode = true;
        }
    }
    public void Update()
    {
        if (isDealBtnActive) return;
        TouchHandle();
    }
    public bool IsFromSlotNull()
    {
        //Debug.Log("IsFromSlotNull");

        if (fromSlot == null)
        {
            //Debug.Log("NULL");
            return true;
        }
        return false;
    }
    public void TouchHandle()
    {
        if (isAnimPlaying || isDealBtnActive) return;
        if (isSimulationMode)
        {
            Debug.Log("Touch handle invoking");
            if (Input.touchCount <= 0 || GameManager.instance.IsNewPlayer) return;
            Touch touch = Input.GetTouch(0);
            Ray ray = SlotCamera.Instance.S_Camera.ScreenPointToRay(touch.position);
            if (!Physics.Raycast(ray, out var hit)) return;

            GameObject tObjct = hit.collider.gameObject;

            if (touch.phase == TouchPhase.Began)
            {
                //Debug.Log("Touch began");   
                if (tObjct.transform.parent.TryGetComponent(out Slot s))
                {
                    //Debug.Log($"Slot {s.gameObject} + slotID {s.ID} + {s.onToucheHandle}");
                    if(s._cards.Count < 0 )  
                    s.onToucheHandle?.Invoke(SlotActiveDebug(s.status));
                }

            }
        }
        else
        {
            if (GameManager.instance.IsNewPlayer) return;

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = SlotCamera.Instance.GetCam().ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(ray, out var hit, Mathf.Infinity))
                {
                    Debug.Log("raycast null");

                    return;
                }

                GameObject tObjct = hit.collider.gameObject;

                // Debug.Log("Mouse button down");
                if (tObjct.transform.parent.TryGetComponent(out Slot s))
                {
                    //Debug.Log("RayCastTarget" + $"");
                    Debug.Log($"Slot {s.gameObject} + slotID {s.ID} + {s.onToucheHandle}");
                    s.onToucheHandle.Invoke(SlotActiveDebug(s.status));
                }
            }
        }
    }
    bool SlotActiveDebug(SlotStatus status)
    {
        Debug.Log(status);
        return status == SlotStatus.Active;
    }

    public void PlayerTouchTutorial(TutorialStep currentStep, Action callback)
    {
        if (isSimulationMode)
        {
            Debug.Log("Player touch tutorial");
            if (Input.touchCount <= 0) return;

            Touch touch = Input.GetTouch(0);
            Ray ray = SlotCamera.Instance.S_Camera.ScreenPointToRay(touch.position);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (touch.phase == TouchPhase.Began)
            {

                if (currentStep.collider == hit.collider)
                {
                    //Debug.Log("this is true collider");
                    currentStep.PlayerHit(callback);
                }
            }
        }
        else
        {
            if (!Input.GetMouseButtonDown(0)) return;
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = SlotCamera.Instance.S_Camera.ScreenPointToRay(mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider == null) return;

            if (currentStep.collider == hit.collider)
            {
                //Debug.Log("this is true collider");
                currentStep.PlayerHit(callback);
            }
        }

    }
}
