using System;
using Enum;
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
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(_mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                OnMouseClick();  // Trigger click only when mouse is over the screw object
            }
        }

        // Detect Mouse Hold (Dragging the screw)
        if (Input.GetMouseButton(0) && isHeld)
        {
            OnMouseHold();
        }

        // Detect Mouse Release
        if (Input.GetMouseButtonUp(0) && isHeld)
        {
            OnMouseRelease();
        }
    }

    private void OnMouseClick()
    {
        if (!LevelMaker.instance.isEditScrewPosition) return;
        ScrewChangeColorOnClick(isSelecting);
        if (!isSelecting)
        {
            if (LevelMaker.instance.isEditHinge)
            {
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
            }
            
        }
        else
        {
            Debug.Log("Clicked object is not a valid part.");
           
        }
    }

    public void ScrewChangeColorOnClick(bool isSelected)
    {
        ChangeScrewColor(isSelected ? UnityEngine.Color.cyan : UnityEngine.Color.green);
    }
    public void ChangeScrewColor(Color color)
    {
        render.color = color; 
    }
    public void ChangeScrewColor(ColorEnum color)
    {
        switch (color)
        {
            case ColorEnum.Red:
                ChangeScrewColor(UnityEngine.Color.red);
                break;
            case ColorEnum.Blue:
                ChangeScrewColor(UnityEngine.Color.blue);
                break;
            case ColorEnum.Yellow:
                ChangeScrewColor(UnityEngine.Color.yellow);
                break;
            case ColorEnum.Black:
                ChangeScrewColor(UnityEngine.Color.black);
                break;
            case ColorEnum.Magenta:
                ChangeScrewColor(UnityEngine.Color.magenta);
                break;
            case ColorEnum.White:
                ChangeScrewColor(UnityEngine.Color.white);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(color), color, null);
        }
    }
    public void CreateHinge(Rigidbody2D targetScrew)
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
        ScrewChangeColorOnClick(isSelecting);
    }

    private void OnMouseHold()
    {
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
}
