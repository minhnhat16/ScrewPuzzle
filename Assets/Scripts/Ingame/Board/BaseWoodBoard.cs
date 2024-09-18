using Ingame;
using Ingame.Screw;
using UnityEngine;

public class BaseWoodBoard : FSMSystem
{
    [SerializeField] protected SpriteRenderer render;  // Renderer cho sprite của bảng gỗ
    [SerializeField] protected ScrewManager screwManager; // Quản lý các screw trên bảng gỗ
    [SerializeField] protected bool isDropped = false;  // Trạng thái thả của bảng gỗ
    [SerializeField] protected Collider2D _collider;     // Collider của bảng gỗ
    [SerializeField] protected LayerMask layerMask;     // Layer mà bảng gỗ tương tác
    
    protected Vector3 initialPosition; // Lưu vị trí ban đầu của bảng gỗ
    public Collider2D _Collider2D
    {
        get => _collider;
        set => _collider = value;
    }

    // Hàm Awake ảo để có thể override từ lớp con
    public virtual void Awake()
    {
        // Lưu vị trí ban đầu
        initialPosition = transform.position;
    }

    // Hàm Start ảo
    public virtual void Start()
    {
        // Kiểm tra xem đã set render chưa
        if (render == null)
            render = GetComponent<SpriteRenderer>();

        if (_collider == null)
            _collider = GetComponent<Collider2D>();
    }

    // Hàm Update ảo
    public virtual void Update()
    {
        // Kiểm tra xem bảng gỗ có được thả xuống đúng vị trí hay không
        if (isDropped)
        {
            // Cập nhật logic khi bảng gỗ đã được thả
            OnBoardDropped();
        }
    }

    // Hàm để thay đổi sprite của bảng gỗ
    public virtual void SetSprite(Sprite sprite)
    {
        this.render.sprite = sprite;
    }

    // Hàm để thay đổi màu của bảng gỗ
    public virtual void SetColor(Color color)
    {
        this.render.color = color;
    }

    // Hàm gọi khi bảng gỗ được thả
    public virtual void DropBoard(Vector3 dropPosition)
    {
        isDropped = true;
        transform.position = dropPosition;  // Cập nhật vị trí thả
    }

    // Xử lý khi bảng gỗ đã được thả
    protected virtual void OnBoardDropped()
    {
        // Kiểm tra va chạm với các đối tượng khác
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.5f, layerMask);

        if (hit != null)
        {
            Debug.Log("Board dropped on a valid position: " + hit.name);
            // Thực hiện các thao tác khi thả vào đúng vị trí, ví dụ gắn với screw
            screwManager.AttachScrewsToBoard(this);
        }
        else
        {
            Debug.Log("Board dropped in an invalid position, returning to initial position.");
            ResetPosition();
        }
    }

    // Hàm reset lại vị trí nếu thả sai
    public virtual void ResetPosition()
    {
        transform.position = initialPosition;
        isDropped = false;
    }

    // Hàm thêm screw vào bảng gỗ
    public virtual void AddScrew(Screw screw)
    {
        screwManager.AddScrew(screw);
    }
}
