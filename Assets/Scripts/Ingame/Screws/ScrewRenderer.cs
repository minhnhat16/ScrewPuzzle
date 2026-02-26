using UnityEngine;
using Enums;
using System;

namespace Ingame.Screw
{
    public class ScrewRender : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer render;
        [SerializeField] private ColorEnum color;
        private string basePartLayerID;

        public ColorEnum Color { get => color; set => color = value; }

        public int GetSortingOrder()
        {
            return render.sortingOrder;
        }
        public string GetSortingLayer()
        {
            return render != null ? render.sortingLayerName : string.Empty;
        }


        public void SetSortingOrderAndLayer(int order, string layer)
        {
            basePartLayerID = layer;
            render.sortingLayerName = layer;
            render.sortingOrder = order + 1;

            int layerIndex = SortingLayer.GetLayerValueFromName(layer);
            float z = 0.2f * (layerIndex + 1);
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }

        public void SetColor(ColorEnum newColor)
        {
            color = newColor;

            Debug.Log("[Screw render ]SetColor: " + color);
            if (color != ColorEnum.Clear)
                render.color = ConfigFileManager.Instance.GetColor(newColor);
        }
        public void SetSpriteBy(ColorEnum newColor) {
            color = newColor;
            if (color != ColorEnum.Clear)
                render.sprite = newColor.ToScrewSprite();
        }
        public void ChangeSprite(Sprite newSprite)
        {
            render.sprite = newSprite;
        }

        public void ResetRender()
        {
            render.transform.localPosition = Vector3.zero;
            render.transform.localRotation = Quaternion.identity;
            render.transform.localScale = Vector3.one;
            render.color = UnityEngine.Color.white;
        }

        internal void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}
