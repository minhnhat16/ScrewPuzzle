using System.Collections;
using DG.Tweening;
using Enum;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace Ingame
{
    public class Screw : MonoBehaviour
    {
        [SerializeField]  private ColorEnum color;
        [SerializeField] private bool isPartiallyVisible;
        [SerializeField] private int layerMask;
        [SerializeField]  private Transform transform { get; set; }
        [SerializeField]  private Vector3 position;
        [SerializeField]  private HingeJoint2D _hingeJoint2D;
        [SerializeField]  private CircleCollider2D _circleCollider2D;
        [SerializeField] private SpriteRenderer render ;
        public ColorEnum Color {get{return color;}set{color = value;}}

        public bool IsActionComplete { get; set; }
        // Start is called before the first frame update
        private Screw()
        {
            IsActionComplete = false;
        }


        private void Awake(){
            // _color = (ColorEnum)System.Enum.Parse(typeof(ColorEnum),"Color");
            transform = GetComponent<Transform>();
            position = GetComponent<Transform>().position;
            _hingeJoint2D = GetComponent<HingeJoint2D>();
            _circleCollider2D = GetComponent<CircleCollider2D>();
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
        // Hàm thuc thi khi screww duoc click;
        public void OnScrewClicked()
        {
            isPartiallyVisible  = IsPartiallyVisible();
            StartCoroutine(CompleteAction());
            if (isPartiallyVisible)
            {
              var box=  BoxQueue.instance.HasBoxWithSameColor(this);
              if (box) box.AddScrew(this);
              else ArrayScrew.instance.AddScrew(this);
            }
            else
            {
                
                Debug.LogWarning("Screw bị chặn bởi cái gì đó rồi");
                ShakeScrew();
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
        public void DoMoveToHold(HoldScrew holdScrew)
        {
            Vector3 toPos = holdScrew.Transf.position;
            Debug.Log("DO move to hold " + toPos);

            // Lưu lại vị trí offset giữa render và cha trước khi di chuyển
            Vector3 offset = toPos - transform.position;

            // Di chuyển render trước, đồng thời di chuyển cha
            var renderMove = render.transform.DOMove(toPos, 0.5f);
            var parentMove = transform.DOMove(toPos - offset, 0.5f); // Di chuyển cha cùng với offset để giữ render ở đúng vị trí

            // Khi cả hai di chuyển xong
            renderMove.OnComplete(() =>
            {
                FreeHinge();
            });
        }
        private void FreeHinge()
        {
            _hingeJoint2D.connectedBody = null;
            _circleCollider2D.isTrigger = true;
        }
    }
}
