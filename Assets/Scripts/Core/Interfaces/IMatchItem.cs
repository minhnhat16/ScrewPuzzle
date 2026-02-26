using UnityEngine;

namespace Core.Match
{
    /// <summary>
    /// Contract cho bất kỳ item nào có thể được collect và match.
    /// Screw, Cat, Card... đều implement interface này.
    ///
    /// Tag là key để match — không dùng ColorEnum cụ thể
    /// để tái sử dụng được cho nhiều game.
    /// </summary>
    public interface IMatchItem
    {
        /// <summary>
        /// Key dùng để match với container.
        /// Screw  → "red", "blue"...
        /// Cat    → "cat_orange", "cat_black"...
        /// Card   → "7", "king"...
        /// </summary>
        string Tag { get; }

        /// <summary>Vị trí world space (dùng cho pathfinding, animation)</summary>
        Vector3 Position { get; }

        /// <summary>Transform để parent/move khi routing</summary>
        Transform Transform { get; }

        /// <summary>Item có thể tương tác không (chưa bị lock, đang idle)</summary>
        bool IsInteractable { get; }
    }
}