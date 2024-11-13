using System;
using Enums;
using Ingame.Screw;
using UnityEditor;
using UnityEngine;

public class ScrewLevelMaker : Screw
{
    [SerializeField] private LevelMaker levelMaker; // Tham chiếu đến LevelMaker
    [SerializeField] private bool isHeld = false;
    [SerializeField] private bool isSelecting = false;

    private Camera _mainCamera;

    public override void Start()
    {
    }

    public override void Awake()
    {
        base.Awake();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        ClickScrewEdit();
    }

    private void ClickScrewEdit()
    {
        // Detect Mouse Click when the mouse is over the screw
        if (Input.GetMouseButtonDown(0) &&
            (LevelMaker.instance.isEditScrewPosition 
             ||LevelMaker.instance.isEditScrewColor 
             || LevelMaker.instance.isEditHinge))
        {
            
            RaycastHit2D hit = Physics2D.Raycast(_mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                OnMouseClick(); // Trigger click only when mouse is over the screw object
            }
        }

        // Detect Mouse Hold (Dragging the screw)
        if (Input.GetMouseButton(0) && isHeld)
        {
            OnMouseHold();
        }

        //Detect Mouse Release
        if (Input.GetMouseButtonUp(0) && isHeld)
        {
            OnMouseRelease();
        }
    }

    internal void ResetHinge()
    {
        var allHinge = hingeController.HingeJoint2D;
        foreach(var hinge in allHinge)
        {
            Destroy(hinge.gameObject);
        }
        hingeController.HingeJoint2D.Clear();
        hingeController.BodyConnect.Clear();
    }

    private void OnMouseClick()
    {
        if (!isSelecting)
        {
            if (LevelMaker.instance.isEditScrewPosition)
            {
                isHeld = LevelMaker.instance.isEditScrewPosition;
                return;
            }
    
            if (LevelMaker.instance.isEditHinge)
            {
                ScrewChangeColorOnClick(isSelecting);
                isHeld = true; // Đánh dấu screw là đang được giữ
                LevelMaker.instance.OnScrewClicked(); // Gọi phương thức từ LevelMaker
                isSelecting = true; // Bật chế độ chọn
                Debug.Log("Selected screw. Now select another object.");
                LevelMaker.instance.ChosePartCoroutine(this);
                return;
            }

            if (LevelMaker.instance.isEditScrewColor)
            {
                Color = (ColorEnum)LevelMaker.instance.currentScrewColorID;
                ChangeScrewColorByEnum(Color);
            }
        }
        else
        {
            Debug.Log("Clicked object is not a valid part.");
        }
    }

    public void ScrewChangeColorOnClick(bool isSelected)
    {
        ChangeScrewColorByEnum(isSelected ? Color: ColorEnum.Green);
    }

    public override HingeJoint2D CreateHinge(Rigidbody2D targetScrew)
    {
        Debug.Log("try to add new hinge " + targetScrew == null);
        GameObject newHingeChild = new()
        {
            transform =
            {
                parent = transform,
                localPosition = Vector3.zero,
                position = targetScrew.transform.position
            },
            name = "connectW" + targetScrew.name,
        };
        // Tạo đối tượng HingeJoint2D mới và thêm vào đối tượng này
        HingeJoint2D hingeJoint = newHingeChild.AddComponent<HingeJoint2D>();
        newHingeChild.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        hingeJoint.connectedBody = targetScrew; // Kết nối hinge với đối tượng screw mục tiêu
        // Lưu HingeJoint2D vào danh sách nếu cần
        hingeController.HingeJoint2D.Add(hingeJoint);
        hingeController.BodyConnect.Add(targetScrew); // Thêm Rigidbody2D vào danh sách bodyConnect
        hingeJoint.autoConfigureConnectedAnchor = true;
        Debug.Log("Created hinge joint with: " + targetScrew.name);
        isSelecting = false;
        ScrewChangeColorOnClick(true);
        return hingeJoint;
    }


    private void OnMouseHold()
    {
        if (!LevelMaker.instance.isEditScrewPosition) return;
        // Get the mouse position in world space
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = _mainCamera.WorldToScreenPoint(transform.position).z; // Maintain the object's z position
        Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);

        // Set the game object's position to follow the mouse position
        transform.position = worldPosition;

        Debug.Log("Mouse is holding and dragging the Screw.");
    }

    private void OnMouseRelease()
    {
        isHeld = false;
        Debug.Log("Mouse Released the Screw.");
    }

    public void ResetScrew()
    {
        isHeld = isSelecting = false;
        ChangeScrewColorByEnum(Color);
    }
}