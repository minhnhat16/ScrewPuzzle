using Level;
using UnityEngine;

public class PartSpriteService : IPartSpriteService
{
    public Sprite GetPartSprite(int levelId, string spriteName, string layer, bool outline)
    {
        SpriteGroup spriteGroup = outline ? SpriteGroup.Outline : SpriteGroup.Main;

        if (outline) spriteName = spriteName.Replace("_a", "_b");
     
        var sprite =  SpriteLibControl.Instance.GetSpritePSB(
            levelId,
            spriteGroup,
            layer,
            spriteName
        );

        if(sprite == null)
            Debug.LogWarning($"[PartSpriteService] Sprite not found: {spriteName} (level {levelId}, layer {layer}, outline {outline})");
        return sprite;
    }
}
