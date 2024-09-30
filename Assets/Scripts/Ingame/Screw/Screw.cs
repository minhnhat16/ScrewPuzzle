using System;
using DG.Tweening;
using Enum;
using Managers;
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
        [SerializeField] private int layerMask;
        [SerializeField] private int sortingOrder;
        [SerializeField] private bool isMultipleJoint;
        [SerializeField] private CircleCollider2D _circleCollider2D;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer render ;

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
            yield return new WaitUntil(()=>ConfigFileManager.Instance.isDone );
            SetScrewColor();
           
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
        public Collider2D[] GetOverlappingColliders(CircleCollider2D circleCollider, float radius, LayerMask mask)
        {
            // Lấy vị trí của CircleCollider2D
            Vector2 colliderPosition = circleCollider.transform.position;

            // Lấy Layer của GameObject chứa CircleCollider2D
            int colliderLayer = hingeController.GetIntBodyLayer(0);

            // Tạo LayerMask chứa tất cả layer từ 10-26
            LayerMask layersInRange = IngameController.Instance.GetLayerMaskForRange(10, colliderLayer - 1 );

            // Thực hiện phép kiểm tra va chạm
            Collider2D[] colliders = Physics2D.OverlapCircleAll(colliderPosition, radius, layersInRange);

            return colliders;
        }

        public void OnScrewClicked()
        {
            // Prevent multiple clicks using the flag
            if (isClicked) return;

            // Set the flag to true right at the start to prevent any more clicks during processing
            // isClicked = true;

            // Ensure you have a reference to the correct CircleCollider2D
            CircleCollider2D myCollider = GetComponent<CircleCollider2D>();

            // Get the LayerMask and overlapping colliders
            LayerMask mask = hingeController.GetIntBodyLayer(0);
            var overlappingColliders = GetOverlappingColliders(myCollider, myCollider.radius, mask);

            // Start the coroutine for completing the action (assuming it's part of the logic)
            // StartCoroutine(CompleteAction());

            // If the screw is blocked by overlapping colliders, perform some action
            if (overlappingColliders.Length > 0)
            {
                Debug.LogWarning("Screw bị chặn bởi cái gì đó rồi");
                ShakeScrew();
                // Reset the flag after handling the overlap situation
                isClicked = false;
            }
            else
            {
                isClicked = ArrayScrew.instance.AddScrew(this);;
            }
        }

        private IEnumerator ResetClickFlagAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);  // Wait for the specified delay
            isClicked = false;                       // Reset the flag
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
                MoveScrewDown();
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

        public virtual void MoveScrewDown()
        {
            var targetPos = render.transform.position;
            targetPos-= new Vector3(0, 0.25f,0);
            cross.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360);
             
            render.transform.DOMove(targetPos, 0.5f).OnComplete(()=> { isMoving = false; });
            
        }
    }
}
