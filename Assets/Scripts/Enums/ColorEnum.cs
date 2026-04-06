using Unity.Jobs;

using UnityEngine;

namespace Enums
{
    public enum ColorEnum
    {
        Clear = 0,
        Empty,
        Red,           
        Blue,
        Gray,
        Magenta,
        White,
        Green,
        Orange,
        Pink,
        Purple,
        Cyan,
        Brown,
        Rainbow,    
    }

    public static class ColorEnumExtensions
    {
        public static string ToColorString(this ColorEnum colorEnum)
        {
            return colorEnum.ToString();
        }
        public static Sprite ToScrewSprite(this ColorEnum colorEnum)
        {
            string path = $"Screw_{colorEnum.ToColorString()}";
            var sprite = SpriteLibControl.Instance.GetSprite(0,SpriteGroup.UI,path);

            Debug.Assert(sprite != null, $"Sprite not found for color: {colorEnum.ToColorString()} at path: {path}");
            return sprite;
        }


        public static Sprite ToBoxSprite(this ColorEnum colorEnum)
        {
            if(colorEnum == ColorEnum.Clear ) return null;
            string path = $"{colorEnum.ToColorString()}";

            var sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, path);


            if (sprite == null) sprite = Resources.Load<Sprite>($"{GameConstants.BOX_SPRITE_PATH}/Green");


            return sprite;
        }
        public static Color ToColor(this ColorEnum colorEnum)
        {
            return colorEnum switch
            {
                ColorEnum.Clear or ColorEnum.Empty => Color.clear,
                ColorEnum.Red => Color.red,
                ColorEnum.Blue => Color.blue,
                ColorEnum.Gray => Color.gray,
                ColorEnum.Magenta => Color.magenta,
                ColorEnum.White => Color.white,
                ColorEnum.Green => Color.green,
                ColorEnum.Orange => new Color(1f, 0.5f, 0f, 1f),// RGB for orange
                ColorEnum.Pink => new Color(1f, 0.4f, 0.7f, 1f),// RGB for pink
                ColorEnum.Purple => new Color(0.5f, 0f, 0.5f, 1f),// RGB for purple
                ColorEnum.Brown => new Color(0.60f, 0.30f, 0.10f, 1f), // Brown
                ColorEnum.Cyan => Color.cyan,
                _ => Color.clear,
            };
        }
    }
}