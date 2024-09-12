using System;
using System.Collections;
using DG.Tweening;
using Enum;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace Ingame
{
    public  class Screw : MonoBehaviour
    {   
        [SerializeField]  private ColorEnum color;
        [SerializeField] private bool isPartiallyVisible;
        [SerializeField] private bool isClicked;
        [SerializeField] private int layerMask;

        

        [SerializeField] private bool isMultipleJoint;
        [SerializeField] private CircleCollider2D _circleCollider2D;
        [SerializeField] private SpriteRenderer render ;

        [SerializeField] private SpriteRenderer cross ;
        [SerializeField] private HingeJoint2D _hingeJoint2D;

        protected Transform _transform { get; set; }
        protected Vector3 position
        {
            get => transform.position;
            set => transform.position = value;
            
        }

        protected HingeJoint2D HingeJoint2D
        {
            get => _hingeJoint2D;
            set => _hingeJoint2D = value;
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
        public IEnumerator Init()
        {
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
            position = GetComponent<Transform>().position;
            _hingeJoint2D = GetComponent<HingeJoint2D>();
            _circleCollider2D = GetComponentInChildren<CircleCollider2D>();
            render = GetComponentInChildren<SpriteRenderer>();
            layerMask = gameObject.layer;
        }

        public bool IsPartiallyVisible()
        {
            // Get the collider's position and radius
            Vector2 colliderPosition = _circleCollider2D.bounds.center;
            float radius = _circleCollider2D.radius;

            // The number of raycasts we'll shoot along the top of the collider
            int raycastCount = 5;
            float angleStep = 180f / (raycastCount - 1); // Angle between raycasts

            for (int i = 0; i < raycastCount; i++)
            {
                // Calculate the angle and position on the top of the circle
                float angle = -90f + i * angleStep; // Raycasts along the top of the collider
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 raycastOrigin = (Vector2)colliderPosition + direction * radius;

                // Raycast upwards (Vector2.up) from the calculated position
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.up, Mathf.Infinity, layerMask);

                // If no object is detected or the hit object is not fully covering the collider
                if (hit.collider == null)
                {
                    Debug.LogWarning("Colider không bị chặn bởi cái gì cả");
                    return true; // The object is partially visible
                }
            }

            // If all raycasts detect objects, the collider is fully covered
            Debug.LogWarning("Colider  bị chặn bởi cái gì đó đ biết");
            return false;
        }

        // To visualize raycasts in the editor
        private void OnDrawGizmos()
        {
          
        }
        private void DrawCircle(){
          /*if (_circleCollider2D == null) return;
                    Vector2 colliderPosition = _circleCollider2D.bounds.center;
                    float radius = _circleCollider2D.radius;
                    int raycastCount = 5;
                    float angleStep = 180f / (raycastCount - 1);
        
                    for (int i = 0; i < raycastCount; i++)
                    {
                        float angle = -90f + i * angleStep;
                        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                        Vector2 raycastOrigin = (Vector2)colliderPosition + direction * radius;
        
                        Gizmos.color = UnityEngine.Color.blue;
                        Gizmos.DrawLine(raycastOrigin, raycastOrigin + Vector2.up * 10f); // Adjust length for visualization
                    }*/    
        }   

        private void SetScrewColor()
        {
            if (color == ColorEnum.Clear) return;
            var targetColor = ConfigFileManager.Instance.ColorConfig.GetRecordByKeySearch(color).Color;
            render.color = targetColor;
        }
        // Hàm thuc thi khi screww duoc click;
        public void OnScrewClicked()
        {
            isPartiallyVisible  = IsPartiallyVisible();
            StartCoroutine(CompleteAction());
            if (isPartiallyVisible && !isClicked)
            {
                isClicked = true; 
                BoxQueue.instance.HasBoxWithSameColor(this);
                 
            }
            else if(!isPartiallyVisible)
            {
                Debug.LogWarning("Screw bị chặn bởi cái gì đó rồi");
                isClicked = false;
                ShakeScrew();
            }
            else{
                Debug.LogWarning("Screw không thể click nữa");
            }
        }

        private void ShakeScrew()
        {
            Tween t = render.transform.DOShakePosition(1f, new Vector3(0.25f, 0, 0));
        }

        private IEnumerator CompleteAction()
        {
            // Giả sử có một hành động nào đó
            yield return new WaitForSeconds(1f);
            IsActionComplete = true; // Hành động hoàn tất
            Player.instance.CanClick = IsActionComplete;
        }


        private bool CheckTrueColorBox()
        {
            // get color box queue if contain box same color return true
            //else put the screw to default queue
            return true;
        }

        //further add move with DOTween
        public virtual void DoMoveToHold(HoldScrew holdScrew)
        {
            DoMoveScrewUp(() =>
            {
                JumpScrewToHold(holdScrew);
            });
        }

        public void JumpScrewToHold(HoldScrew holdScrew)
        {
            Vector3 toPos = holdScrew.Transf.position + new Vector3(0,0.25f);
            Debug.Log("DO move to hold " + toPos);

            // Lưu lại vị trí offset giữa render và cha trước khi di chuyển
            Vector3 offset = toPos - _transform.position ;

            // Di chuyển render trước, đồng thời di chuyển cha
            var renderMove = render.transform.DOJump(toPos, 2,1,0.5f,false);
            var parentMove = _transform.DOMove(toPos - offset, 0.5f); // Di chuyển cha cùng với offset để giữ render ở đúng vị trí
            
            //free joint to releas wood and joint
            FreeHinge();

            // Khi cả hai di chuyển xong
            renderMove.OnComplete(()=>
            {
                _transform.SetParent(holdScrew.Transf);
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
            _hingeJoint2D.connectedBody = null;
        }

        public virtual void MoveScrewDown()
        {
            var targetPos = render.transform.position;
            targetPos-= new Vector3(0, 0.25f,0);
            cross.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360);
            render.transform.DOMove(targetPos, 0.5f);
        }
    }
}
