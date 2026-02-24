#if UNITY_EDITOR
using Enums;
using Ingame.Board;
using Ingame.Screw;
using Level;
using UnityEngine;

namespace EditorTools
{
    public class ScrewLevelMaker : ScrewController
    {
        [Header("Level Maker Flags")]
        [SerializeField] private bool isHeld;
        [SerializeField] private bool isSelecting;

        private Camera _mainCamera;

        public LayerMask ScrewLayerMask => LayerMask.GetMask("Screw");
        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleMouseInputInEditor();
        }

        #region 🖱️ Input & Editor Interaction

        private void HandleMouseInputInEditor()
        {
            if (Input.GetMouseButtonDown(0) && IsAnyEditModeActive())
                OnMouseClickEditor();

            if (Input.GetMouseButton(0) && isHeld)
                OnMouseDragEditor();

            if (Input.GetMouseButtonUp(0) && isHeld)
                OnMouseReleaseEditor();
        }

        private bool IsAnyEditModeActive()
        {
            return LevelMaker.instance.isEditScrewPosition ||
                   LevelMaker.instance.isEditScrewColor ||
                   LevelMaker.instance.isEditHinge ||
                   LevelMaker.instance.isRemoveScrew;
        }

        #endregion

        #region 🎯 Mouse Editor Actions

        private void OnMouseClickEditor()
        {
            //if (!IsClickedScrewByRay()) return;

            ////=== Action Handling ===//
            //if (LevelMaker.instance.isEditScrewPosition)
            //{
            //    isHeld = true;
            //    return;
            //}

            //if (LevelMaker.instance.isEditHinge)
            //{
            //    HandleEditHinge();
            //    return;
            //}

            //if (LevelMaker.instance.isEditScrewColor)
            //{
            //    Color = (ColorEnum)LevelMaker.instance.currentScrewColorID;
            //    ChangeScrewColor(Color);
            //    GameObjectToLevelConverter.ins.UpdateScrewTotal();
            //    return;
            //}

            //if (LevelMaker.instance.isRemoveScrew)
            //{
            //    ResetHinge();
            //    GameObjectToLevelConverter.ins.RemoveScrew(this);
            //    DestroyImmediate(gameObject);
            //    return;
            //}
        }

        private bool IsClickedScrewByRay()
        {
            int screwMask = LayerMask.GetMask("Screw");
            RaycastHit2D hit = Physics2D.Raycast(
                _mainCamera.ScreenToWorldPoint(Input.mousePosition),
                Vector2.zero,
                Mathf.Infinity,
                screwMask
            );

            return hit.collider != null && hit.collider.gameObject == gameObject;
        }

        private void HandleEditHinge()
        {
            //ScrewChangeColorOnClick(isSelecting);
            //isHeld = true;
            //isSelecting = true;
            //LevelMaker.instance.OnScrewClicked();
            //TurnColliderIs(false);
            ////LevelMaker.instance.ChosePartCoroutine(this);
        }

        private void OnMouseDragEditor()
        {
            if (!LevelMaker.instance.isEditScrewPosition) return;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = _mainCamera.WorldToScreenPoint(transform.position).z;

            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
            transform.position = worldPos;
        }

        private void OnMouseReleaseEditor()
        {
            isHeld = false;
        }

        #endregion

        #region 🔩 Editor Hinge Management

        internal void ResetHinge()
        {
            //var hinge = hingeController.HingeJoint2D;
            //if (hinge == null) return;

            //hinge.connectedBody = null;
            //DestroyImmediate(hinge);
        }

        //public override HingeJoint2D CreateHinge(Rigidbody2D targetPart, HingeConnection connection)
        //{
        //    GameObject hingeObj = new GameObject($"H GameObject($"H GameObject($"Hinge_{targetPart.name}");
        //    hingeObj.transform.SetParent(transform);
        //    hingeObj.transform.localPosition = connection.hingePosition;

        //    var hingeJoint = hingeObj.AddComponent<HingeJoint2D>();
        //    hingeObj.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        //    hingeJoint.connectedBody = targetPart;
        //    hingeJoint.autoConfigureConnectedAnchor = true;

        //    hingeController.HingeJoint2D = hingeJoint;
        //    hingeController.BodyConnect = targetPart;

        //    isSelecting = false;
        //    ScrewChangeColorOnClick(false);
        //    TurnColliderIs(true);
        //    return hingeJoint;
        //}

        //#endregion


        //public void ScrewChangeColorOnClick(bool isSelected)
        //{
        //    ColorEnum temp = Color == ColorEnum.Green ? ColorEnum.Red : ColorEnum.Green;
        //    temp = isSelected ? temp : Color;
        //    render.sprite = temp.ToScrewSprite();
        //}

        //public void TurnColliderIs(bool isEnable)
        //{
        //    if (CircleCollider2D == null) return;
        //    CircleCollider2D.enabled = isEnable;
        //}

        //public void ResetScrew()
        //{
        //    isHeld = isSelecting = false;
        //    ChangeScrewColor(Color);
        //}

       #endregion
    }
}
#endif