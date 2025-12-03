using DG.Tweening;
using Enums;
using Level;
using System;
using System.Linq.Expressions;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace Ingame.Screw
{
    public class Screw : MonoBehaviour,IResetable
    {
        [SerializeField] private ColorEnum color;
        [SerializeField] private bool isInHold;
        [SerializeField] private bool isClicked;
        public int layerMask;
        [SerializeField] internal int sortingOrder;
        [SerializeField] private bool isMultipleJoint;
        [SerializeField] protected CircleCollider2D _circleCollider2D;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] protected SpriteRenderer render;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] protected HingeController hingeController;
        public HingeController HingeController
        {
            get => hingeController;
            set => hingeController = value;
        }

        [SerializeField] private bool isShaking;

        [SerializeField] private bool isMoving;
        private string basePartLayerID;

        protected Transform _transform { get; set; }
        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;

        }

        protected CircleCollider2D CircleCollider2D
        {
            get => _circleCollider2D;
            set => _circleCollider2D = value;
        }

        protected SpriteRenderer Renderer
        {
            get => render;
            set => render = value;
        }
        public ColorEnum Color { get { return color; } set { color = value; } }

        public bool IsActionComplete { get; set; }
        // Start is called before the first frame update
        public virtual void Start()
        {
            isClicked = false;
            StartCoroutine(Init());
        }

        private void OnEnable()
        {

        }

        public IEnumerator Init()
        {
            IsInHold = false;
            string bodyLayer = hingeController.GetConnectedBodyRenderLayer(0);
            yield return new WaitUntil(() => bodyLayer != null);
            SetSortingOrderAndLayer(sortingOrder, bodyLayer);
          
        }

        public Screw()
        {
            IsActionComplete = false;
        }

        public string BasePartLayerID { get => basePartLayerID; set => basePartLayerID = value; }
        public bool IsInHold { get => isInHold; set => isInHold = value; }
        public bool IsClicked { get => isClicked; set => isClicked = value; }

        public virtual void Awake()
        {
            // _color = (ColorEnum)System.Enum.Parse(typeof(ColorEnum),"Color");
            _transform = GetComponent<Transform>();
            Position = GetComponent<Transform>().position;
            _circleCollider2D = GetComponentInChildren<CircleCollider2D>();
            render = GetComponentInChildren<SpriteRenderer>();
            layerMask = gameObject.layer;
        }

        public void ResetRender()
        {
            render.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            //cross.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        internal void SetSortingOrderAndLayer(int order, string layer)
        {

            basePartLayerID = layer;
            render.sortingLayerName = layer;
            //cross.sortingLayerName = layer;
            render.sortingOrder = order + 1;
            // cross.sortingOrder = order + 2;

            int layerIndex = SortingLayer.GetLayerValueFromName(layer);
            float z = 0.2f * (layerIndex + 1);
            Debug.LogWarning("Layer index " + gameObject.name + " is " + layerIndex);
            transform.position = new Vector3(Position.x, Position.y, z);
        }

        private void SetScrewColor()
        {
            if (color == ColorEnum.Clear) return;
            var targetColor = ConfigFileManager.Instance.GetColor(color);
            render.color = targetColor;
        }
        public bool OnScrewClicked()
        {
            // Ngăn chặn nhiều lần click bằng cờ `isClicked`
            if (isClicked) return true;
            isClicked = true;

            // Giả sử `connectedBody` thuộc về một lớp cần bỏ qua
            int connectedBodyLayer = hingeController.GetIntBodyLayer(0); // Lấy lớp của connectedBody

            // Tạo LayerMask để kiểm tra các lớp từ 0 đến 19, nhưng bỏ qua lớp của connectedBody
            LayerMask mask = 0;

            for (int i = 10; i < connectedBodyLayer; i++) // Chỉ xét từ lớp 10 đến lớp 19
            {
                mask |= (1 << i); // Dùng phép OR bit để thêm từng lớp vào LayerMask
            }


            // Kiểm tra các Collider2D thuộc các lớp trong bán kính của CircleCollider2D
            Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(CircleCollider2D.transform.position, CircleCollider2D.radius - 0.1f, mask);

            // Nếu có đối tượng nào chặn, đưa ra cảnh báo và thực hiện hành động
            if (overlappingColliders.Length > 0)
            {
                ShakeScrew();  // Gọi hàm để tạo hiệu ứng rung (nếu có)
                isClicked = false; // Reset flag để có thể click lần sau
                ResetClickedFlag();
                return true;
            }

            // Không có va chạm, có thể tiếp tục logic khác
            return false;
        }

        public void ResetClickedFlag()
        {
            StartCoroutine(ResetClickFlagAfterDelay(0.5f));

        }
        public IEnumerator ResetClickFlagAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);  // Wait for the specified delay
            isClicked = false;
            CircleCollider2D.enabled = !isClicked;
        }


        private void ShakeScrew()
        {
            if (isShaking) return;
            Tween t = render.transform.DOShakePosition(1f, new Vector3(0.15f, 0, 0)).SetEase(Ease.OutBounce);
            t.OnPlay(() => isClicked = isShaking = true);
            t.OnComplete(() =>
            {
                render.transform.localPosition = Vector3.zero;
                isClicked = isShaking = false;
            });
        }

        private bool CheckTrueColorBox()
        {
            // get color box queue if contain box same color return true
            //else put the screw to default queue
            return true;
        }
        public virtual void DoMoveToHold(HoldScrew holdScrew, bool isTele)
        {
            isClicked = isMoving = true;
            IsInHold = true;
            if (isTele)
            {
                gameObject.SetActive(true);
                Debug.Log("teleport screw to hold");
                isClicked = isMoving = true;
                IsInHold = true;
                _circleCollider2D.enabled = false;
                FreeHinge();
                _transform.SetParent(holdScrew.transform);
                _transform.localPosition = Vector3.zero;
                return;
            }
            DoMoveToHold(holdScrew);
        }
        //further add move with DOTween
        public virtual void DoMoveToHold(HoldScrew holdScrew)
        {

            DoMoveScrewUp(() =>
            {
                _circleCollider2D.enabled = false;
                FreeHinge();
                JumpScrewToHold(holdScrew);
            });
        }

        public bool IsMoving()
        {
            // Check if the screw's velocity is not zero, implying movement
            return isMoving;// Adjust threshold as necessary
        }
        public void JumpScrewToHold(HoldScrew holdScrew)
        {
            Vector3 toPos = holdScrew.Transf.position + new Vector3(0, 0.18f);

            Sequence seq = DOTween.Sequence();

            seq.Append(render.transform
                .DOJump(toPos, 1.6f, 1, 0.45f)
                .SetEase(Ease.OutQuad));

            seq.Join(render.transform
                .DOScale(1.12f, 0.3f)
                .SetEase(Ease.OutSine));

            seq.OnPlay(() => isMoving = true);

            seq.OnComplete(() => MoveScrewDown(holdScrew));
        }

        public Tween MoveScrewDown(HoldScrew holdScrew)
        {
            var targetPos = render.transform.position - new Vector3(0, 0.18f);

            Sequence seq = DOTween.Sequence();

            seq.Append(render.transform
                .DOMoveY(targetPos.y, 0.15f)
                .SetEase(Ease.InQuad));

            seq.Join(render.transform
                .DORotate(new Vector3(0, 0, -360f), 0.15f, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutSine));

            seq.Join(render.transform
                .DOScale(1f, 0.15f)
                .SetEase(Ease.OutBack));

            seq.OnComplete(() =>
            {
                _transform.SetParent(holdScrew.transform);
                _transform.transform.localPosition = Vector3.zero;
                isMoving = false;
                render.transform.localPosition = Vector3.zero;



            });

            return seq;
        }
        public void DoMoveScrewUp(Action callback)
        {
            Vector3 targetPos = render.transform.position + new Vector3(0, 0.22f, 0);

            Sequence seq = DOTween.Sequence();

            seq.Append(render.transform
                .DOMove(targetPos, 0.2f)
                .SetEase(Ease.OutQuad));

            seq.Join(render.transform
                .DORotate(new Vector3(0, 0, 290), 0.2f, RotateMode.FastBeyond360)
                .SetEase(Ease.OutSine));

            seq.Join(render.transform
                .DOScale(1.08f, 0.1f)
                .SetEase(Ease.OutBack));

            seq.OnComplete(() =>
            {
                hingeController.Reset();
                render.transform.DOScale(1f, 0.1f).SetEase(Ease.OutSine);
                callback?.Invoke();
            });
        }
        public virtual void FreeHinge()
        {
            _circleCollider2D.isTrigger = true;
            hingeController.FreeHinges();
        }
        public void ChangeScrewColor(ColorEnum color)
        {
            this.color = color;
            render.sprite = color.ToScrewSprite();
        }
        public virtual HingeJoint2D CreateHinge(Rigidbody2D targetPart, HingeConnection connection)
        {
            // Spawn a new Hinge object from the pool
            HingeObject hinge = HingePool.Instance.pool.SpawnNonGravity();
            GameObject newHingeChild = hinge.gameObject;

            // Set parent and position
            newHingeChild.transform.SetParent(transform);
            newHingeChild.transform.localPosition = connection.hingePosition;

            // Check if HingeJoint2D exists, or create a new one if it doesn't
            HingeJoint2D hingeJoint = newHingeChild.GetComponent<HingeJoint2D>();
            if (hingeJoint == null)
            {
                hingeJoint = newHingeChild.AddComponent<HingeJoint2D>();
            }

            // Configure Rigidbody2D
            Rigidbody2D hingeBody = newHingeChild.GetComponent<Rigidbody2D>();
            if (hingeBody == null)
            {
                hingeBody = newHingeChild.AddComponent<Rigidbody2D>();
            }
            hingeBody.bodyType = RigidbodyType2D.Static;

            // Configure the hinge joint
            hingeJoint.connectedBody = targetPart;
            hingeJoint.autoConfigureConnectedAnchor = true;

            // Add hinge joint and connected body to the hinge controller lists
            hingeController.HingeJoint2D.Add(hingeJoint);
            hingeController.BodyConnect.Add(targetPart);

            return hingeJoint;
        }

        public void OnReset()
        {
            isClicked = false;
            color = ColorEnum.Clear;
            render.transform.localScale = Vector3.one;
            CircleCollider2D.enabled = true;
            render.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            SetSortingOrderAndLayer(0, LayerEnum.Default.ToString());
            hingeController.Reset();
            ResetRender();
        }
    }
}
