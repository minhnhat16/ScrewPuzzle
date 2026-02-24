    using System;
    using UnityEngine;
    using Ingame.Screw;

    public class HoldScrew : MonoBehaviour
    {
        [SerializeField] private ScrewController screw;  // Slot hiện tại chứa Screw nào
        [SerializeField] private string sortingLayerName = "Box";
        [SerializeField] private int sortingOrder = 5;

        public ScrewController Screw { get => screw; set => screw = value; }

        /// <summary>
        /// Thêm một Screw mới vào HoldBox.
        /// </summary>
        /// <param name="newScrew">Screw cần thêm</param>
        /// <param name="isTele">Có teleport trực tiếp hay không (bỏ qua Tween)</param>
        /// <param name="callback">Callback khi move hoàn tất (tru  e = thành công)</param>
        public void AddScrew(ScrewController newScrew, bool isTele = false, Action<bool> callback = null)
        {
            // Nếu đang rỗng → nhận screw mới
            if (screw == null)
            {
                screw = newScrew;

                // Đảm bảo sorting đúng layer "Box"
                screw.SetSortingOrderAndLayer(sortingOrder, sortingLayerName);

                screw.MoveToHold(this, isTele);

                callback?.Invoke(true);
                return;
            }

            // Nếu slot đang có screw → báo callback false (thêm không thành công)
            Debug.LogWarning($"[HoldScrew] Slot {name} đã có screw! Không thể thêm {newScrew.name}");
            callback?.Invoke(false);
        }

        /// <summary>
        /// Kiểm tra slot có đang rỗng không
        /// </summary>
        public bool IsEmpty() => screw == null;

        /// <summary>
        /// Lấy Screw hiện tại (nếu có)
        /// </summary>
        public ScrewController GetScrew() => screw;

        /// <summary>
        /// Xóa Screw ra khỏi slot (nếu cần)
        /// </summary>
        public void RemoveScrew()
        {
            if (screw == null) return;
            screw = null;
        }
    }
