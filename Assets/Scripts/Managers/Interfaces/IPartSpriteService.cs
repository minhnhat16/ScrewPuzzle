using UnityEngine;

public interface IPartSpriteService
{
    Sprite GetPartSprite(int levelId, string spriteName,string layer, bool outline);
}
