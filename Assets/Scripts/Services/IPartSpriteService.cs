using UnityEngine;

public interface IPartSpriteService
{
    Sprite GetPartSprite(int levelId, string spriteName, bool outline);
}
