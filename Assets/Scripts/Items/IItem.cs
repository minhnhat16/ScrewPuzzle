public interface IItem
{
    ItemType ItemType { get; }
    bool IsHandling { get; }

    /// <summary>
    /// Kích hoạt item — một số item execute ngay (AddBox, Magnet),
    /// một số chỉ set selected và chờ player tương tác (Breaker).
    /// </summary>
    void Use(UnityEngine.Vector3 targetPos = default);

    void HandlingItem();
    void Discard();
}