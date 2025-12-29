#if UNITY_EDITOR

using Enums;
using Ingame.Board;
using Ingame.Screw;
using Level;
using System.Collections;
using System.Drawing;
using Unity.Jobs;
using UnityEngine;
public class ScrewLevelMaker : Screw
{
    [SerializeField] private bool isHeld = false;
    [SerializeField] private bool isSelecting = false;


    public LayerMask mask;
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
        if (Input.GetMouseButtonDown(0) &&
            (LevelMaker.instance.isEditScrewPosition
             || LevelMaker.instance.isEditScrewColor
             || LevelMaker.instance.isEditHinge
             || LevelMaker.instance.isRemoveScrew))
        {
            Debug.Log("Mouse Button Down Detected");
            int screwLayerMask = LayerMask.GetMask("Screw");
            RaycastHit2D hit = Physics2D.Raycast(_mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, float.PositiveInfinity, screwLayerMask);

            Debug.Log("Raycast hit: " + (hit.collider != null ? hit.collider.gameObject.name : "Nothing"));
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Debug.Log("Screw object clicked: " + gameObject.name);
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

    public IEnumerator InitOnLevelMaker()
    {
        string bodyLayer = hingeController.GetConnectedBodyRenderLayer(0);
        yield return new WaitUntil(() => bodyLayer != null);
        SetSortingOrderAndLayer(sortingOrder, bodyLayer);
        ChangeScrewColor(Color);
    }

    internal void ResetHinge()
    {
        var hinge = hingeController.HingeJoint2D;
        if (hinge == null) return;
        hinge.connectedBody = null;

        Destroy(hinge);
    }

    private void OnMouseClick()
    {

        Debug.Log("Screw clicked in Level Maker mode: " + isSelecting);
        if (!isSelecting)
        {
            if (LevelMaker.instance.isEditScrewPosition)
            {
                isHeld = LevelMaker.instance.isEditScrewPosition;
                return;
            }

            if (LevelMaker.instance.isEditHinge)
            {
                //if (hingeController.HingeJoint2D == null) return;
                ScrewChangeColorOnClick(isSelecting);
                isHeld = true; // Đánh dấu screw là đang được giữ
                LevelMaker.instance.OnScrewClicked(); // Gọi phương thức từ LevelMaker
                isSelecting = true; // Bật chế độ chọn
                Debug.Log("Selected screw. Now select another object.");
                TurnColliderIs(!isSelecting);
                LevelMaker.instance.ChosePartCoroutine(this);
                return;
            }

            if (LevelMaker.instance.isEditScrewColor)
            {
                Color = (ColorEnum)LevelMaker.instance.currentScrewColorID;
                GameObjectToLevelConverter.ins.UpdateScrewTotal();
                ChangeScrewColor(Color);
            }
            if (LevelMaker.instance.isRemoveScrew)
            {
                ResetHinge();
                GameObjectToLevelConverter.ins.RemoveScrew(this);
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("Clicked object is not a valid part.");
        }
    }


    public void ScrewChangeColorOnClick(bool isSelected)
    {
        ColorEnum temp;
        temp = Color == ColorEnum.Green ? ColorEnum.Red : ColorEnum.Green;
        temp = isSelected ? temp : Color;
        Debug.Log("Color temp " + temp + " main color: " + Color + " and is selected " + isSelected);
        render.sprite = temp.ToScrewSprite();

    }
    public void TurnColliderIs(bool isEnable)
    {

        Debug.Log("Turn collider is " + isEnable);
        CircleCollider2D.enabled = isEnable;
    }
    public HingeJoint2D CreateHingeWithMousePos(Rigidbody2D targetBody, HingeConnection connection)
    {
        Debug.Log($"try to add new hing: mousepos {connection.hingePosition} ");
        GameObject newHingeChild = new()
        {
            transform =
            {
                parent = transform,
                position = connection.hingePosition,
                //position = targetScrew.transform.position
            },
            name = "connectW" + targetBody.name,
        };
        // Tạo đối tượng HingeJoint2D mới và thêm vào đối tượng này
        HingeJoint2D hingeJoint = newHingeChild.AddComponent<HingeJoint2D>();
        newHingeChild.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        hingeJoint.connectedBody = targetBody; // Kết nối hinge với đối tượng screw mục tiêu
        // Lưu HingeJoint2D vào danh sách nếu cần
        hingeController.HingeJoint2D = hingeJoint;
        hingeController.BodyConnect =targetBody; // Thêm Rigidbody2D vào danh sách bodyConnect
        hingeJoint.autoConfigureConnectedAnchor = true;
        Debug.Log("Created hinge joint with: " + targetBody.name + ",layer : " + targetBody.gameObject.layer);
        isSelecting = false;
        ScrewChangeColorOnClick(false);
        TurnColliderIs(!isSelecting);
        return hingeJoint;
    }
    public override HingeJoint2D CreateHinge(Rigidbody2D targetScrew, HingeConnection connection)
    {
        Debug.Log("try to add new hinge " + targetScrew == null);
        GameObject newHingeChild = new()
        {
            transform =
            {
                parent = transform,
                localPosition = connection.hingePosition,
                //position = targetScrew.transform.position
            },
            name = "connectW" + targetScrew.name,
        };
        // Tạo đối tượng HingeJoint2D mới và thêm vào đối tượng này
        HingeJoint2D hingeJoint = newHingeChild.AddComponent<HingeJoint2D>();
        newHingeChild.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        hingeJoint.connectedBody = targetScrew; // Kết nối hinge với đối tượng screw mục tiêu
        // Lưu HingeJoint2D vào danh sách nếu cần
        hingeController.HingeJoint2D= hingeJoint;
        hingeController.BodyConnect = targetScrew; // Thêm Rigidbody2D vào danh sách bodyConnect
        hingeJoint.autoConfigureConnectedAnchor = true;
        Debug.Log("Created hinge joint with: " + targetScrew.name);
        isSelecting = false;
        ScrewChangeColorOnClick(false);
        TurnColliderIs(!isSelecting);
        return hingeJoint;
    }


    private void OnMouseHold()
    {

        Debug.Log("on mouse hold detected");
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
        ChangeScrewColor(Color);
    }
}
#endif