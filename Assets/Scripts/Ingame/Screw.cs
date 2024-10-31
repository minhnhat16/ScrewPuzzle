using System;
using System.Drawing;
using DG.Tweening;
using Enum;
using Managers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using IEnumerator = System.Collections.IEnumerator;

namespace Ingame.Screw
{
    public  class Screw : MonoBehaviour
    {   
        [SerializeField]  private ColorEnum color;
        [SerializeField] private bool isPartiallyVisible;
        [SerializeField] private bool isClicked;
        [SerializeField] protected int layerMask;
        [SerializeField] private int sortingOrder;
        [SerializeField] private bool isMultipleJoint;
        [SerializeField] protected CircleCollider2D _circleCollider2D;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] protected SpriteRenderer render ;

        [SerializeField] private SpriteRenderer cross ;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] protected HingeController hingeController;
        public HingeController HingeController
        {
            get => hingeController;
            set => hingeController = value;
        }

        [SerializeField] private bool isShaking;

        [SerializeField] private bool isMoving;

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
        public ColorEnum Color {get{return color;}set{color = value;}}

        public bool IsActionComplete { get; set; }
        // Start is called before the first frame update
        public virtual void Start(){
            isClicked  =false;
            StartCoroutine(Init());
        }

        private void OnEnable()
        { 
           
        }

        public IEnumerator Init()
        {
            string bodyLayer = hingeController.GetConnectedBodyRenderLayer(0);
            yield return new WaitUntil(()=>bodyLayer !=null );
            SetSortingOrderAndLayer(sortingOrder, bodyLayer);
            /*yield return new WaitUntil(()=>ConfigFileManager.Instance.isDone );
            SetScrewColor();*/
        }
        public IEnumerator InitOnLevelMaker()
        {
            string bodyLayer = hingeController.GetConnectedBodyRenderLayer(0);
            yield return new WaitUntil(()=>bodyLayer !=null );
            SetSortingOrderAndLayer(sortingOrder, bodyLayer);
            // yield return new WaitUntil(()=>ConfigFileManager.Instance.isDone );
            // SetScrewColor();
        }
        public Screw()
        {
            IsActionComplete = false;
        }
        public int LayerMask
        {
            get => layerMask;
            set => layerMask = value;
        }
        public virtual void Awake(){
            // _color = (ColorEnum)System.Enum.Parse(typeof(ColorEnum),"Color");
            _transform = GetComponent<Transform>();
            Position = GetComponent<Transform>().position;
            _circleCollider2D = GetComponentInChildren<CircleCollider2D>();
            render = GetComponentInChildren<SpriteRenderer>();
            layerMask = gameObject.layer;
        }

        private void SetSortingOrderAndLayer(int order, string layer)
        {
            // Debug.LogError("Setting sorting order and layer");
            render.sortingLayerName =layer;
            cross.sortingLayerName = layer;
            render.sortingOrder = order + 1;
            cross.sortingOrder = order + 2;

            int layerIndex = SortingLayer.GetLayerValueFromName(layer);
            Debug.LogWarning("Layer index " + gameObject.name + " is " + layerIndex);
            Position = new Vector3(Position.x, Position.y, -layerIndex  +10);
        }

        private void SetScrewColor()
        {
            if (color == ColorEnum.Clear) return;
            var targetColor = ConfigFileManager.Instance.ColorConfig.GetRecordByKeySearch(color).Color;
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
            Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(CircleCollider2D.transform.position, CircleCollider2D.radius, mask);

            // Nếu có đối tượng nào chặn, đưa ra cảnh báo và thực hiện hành động
            if (overlappingColliders.Length > 0)
            {
                Debug.LogWarning("Screw bị chặn bởi một đối tượng, ngoại trừ connectedBody.");
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
            Debug.Log("Screw: Reset Click Flag After Delay after" + delay);
            yield return new WaitForSeconds(delay);  // Wait for the specified delay
            isClicked = false;   
            CircleCollider2D.enabled = !isClicked;
            // Reset the flag
            Debug.Log("Screw: Reset Click Flag After Delay Done" + isClicked);
        }


        private void ShakeScrew()
        {
            if (isShaking) return;
            Tween t = render.transform.DOShakePosition(1f, new Vector3(0.15f, 0, 0)).SetEase(Ease.OutBounce);
            t.OnPlay(() => isClicked = isShaking= true);
            t.OnComplete(() =>
            {
                render.transform.localPosition = Vector3.zero;
                isClicked = isShaking = false;
            });
        }

        // private IEnumerator CompleteAction()
        // {
        //     // Giả sử có một hành động nào đó
        //     yield return new WaitForSeconds(1f);
        //     IsActionComplete = true; // Hành động hoàn tất
        //     Player.instance. = IsActionComplete;
        // }


        private bool CheckTrueColorBox()
        {
            // get color box queue if contain box same color return true
            //else put the screw to default queue
            return true;
        }

        //further add move with DOTween
        public virtual void DoMoveToHold(HoldScrew holdScrew)
        {
            isClicked = isMoving = true;
            DoMoveScrewUp(() =>
            {
                _circleCollider2D.enabled = false;
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
            Vector3 toPos = holdScrew.Transf.position + new Vector3(0, 0.25f);
            Debug.Log("DO move to hold " + toPos);

            // Lưu lại vị trí offset giữa render và cha trước khi di chuyển
            Vector3 offset = toPos - _transform.position;

            // Tạo một Sequence để sắp xếp các tweens
            Sequence sequence = DOTween.Sequence();

            // Di chuyển render trước, đồng thời di chuyển cha
            sequence.Append(render.transform.DOJump(toPos, 2, 1, 0.5f, false));
            sequence.Join(_transform.DOMove(toPos - offset, 0.5f)); // Di chuyển cha cùng với offset để giữ render ở đúng vị trí
            sequence.OnPlay(() => isMoving = true);
            // free joint to release wood and joint
            FreeHinge();

            // Khi cả hai di chuyển xong
            sequence.OnComplete(() =>
            {
                // _transform.SetParent(holdScrew.Transf);
                MoveScrewDown(holdScrew);
            });
        }

        public void DoMoveScrewUp(Action callback)
        {
            var targetPos = render.transform.position;
            targetPos+= new Vector3(0, 0.25f,0);
            cross.transform.DORotate(new Vector3(0, 0, 360), 0.7f, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad);
            render.transform.DOMove(targetPos, 0.5f).OnComplete(()=>callback?.Invoke());
        }
        public virtual void FreeHinge()
        {
            _circleCollider2D.isTrigger = true;
            hingeController.FreeHinges();
        }

        public void ChangeScrewColor(UnityEngine.Color color)
        {
            render.color = color;
        }

        public void ChangeScrewColorByEnum(ColorEnum color)
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
                case ColorEnum.Gray:
                    ChangeScrewColor(UnityEngine.Color.gray);
                    break;
                case ColorEnum.Green:
                    ChangeScrewColor(UnityEngine.Color.green);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }
        }

        public virtual void MoveScrewDown(HoldScrew holdScrew)
        {
            var targetPos = render.transform.position;
            targetPos-= new Vector3(0, 0.25f,0);
            cross.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360);
             
            render.transform.DOMove(targetPos, 0.5f).OnComplete(()=> { isMoving = false; });
            _transform.SetParent(holdScrew.transform);

        }
        public virtual void CreateHinge(Rigidbody2D targetScrew)
        {
            HingeObject hinge = HingePool.Instance.pool.SpawnNonGravity();
            GameObject newHingeChild = hinge.gameObject;
            newHingeChild.transform.SetParent(transform);
            newHingeChild.transform.localPosition = Vector3.zero;
            newHingeChild.transform.position = targetScrew.transform.position;

            // Tạo đối tượng HingeJoint2D mới và thêm vào đối tượng này
            HingeJoint2D hingeJoint = newHingeChild.AddComponent<HingeJoint2D>();
            newHingeChild.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            hingeJoint.connectedBody = targetScrew; // Kết nối hinge với đối tượng screw mục tiêu
            // Lưu HingeJoint2D vào danh sách nếu cần
            hingeController.HingeJoint2D.Add(hingeJoint);
            hingeController.BodyConnect.Add(targetScrew); // Thêm Rigidbody2D vào danh sách bodyConnect
            hingeJoint.autoConfigureConnectedAnchor = true;
        
        }
        public void Reset()
        {
            isClicked = false;
            color = ColorEnum.Clear;
            CircleCollider2D.enabled = true;
            render.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            SetSortingOrderAndLayer(0, LayerEnum.Default.ToString());
            hingeController.Reset();
        }
    }
}
