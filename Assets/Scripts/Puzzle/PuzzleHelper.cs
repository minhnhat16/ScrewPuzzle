
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PuzzleHelper : IPuzzleRule
{
    public bool CanMove(PuzzleBlock block)
        => block.IsUnlocked();

    public bool CheckWin()
        => true;
}


public static class PuzzleBlockFactory
{
    public static PuzzleBlock Create(
        int blockId,
        List<PuzzleCellRecord> records,
        UnityEngine.Transform parent)
    {
        int screw = records[0].ScrewRequired;

        var go = new GameObject($"Block_{blockId}");
        var block = go.AddComponent<ScrewBlock>();
        go.transform.SetParent(parent);

        // cells sẽ được gán sau
        return block;
    }
}


public enum BlockOrientation
{
    Up,
    Down,
    Left,
    Right
}
public enum BlockShape
{
    Single,
    Square,
    Square2,
    Rectangle3,
    Rectangle2,
    L3,
    L4,
    Irregular
}
public struct ShapeResult
{
    public BlockShape shape;
    public BlockOrientation orientation;
}



public static class BlockShapeAnalyzer
{
    public static void ApplySprite(
        ShapeResult result,
        PuzzleBlock blockView,
        SpriteLibControl lib)
    {
        string name = "Block " + result.shape.ToString();
        Sprite sprite = lib.GetSprite(name);
        blockView.SetSprite(sprite);
        Debug.Log($"Sprite name {name} is null {sprite is null}");
        if (result.shape == BlockShape.L3 || result.shape == BlockShape.L4)
            blockView.SetRotation(result.orientation);
    }
    public static ShapeResult Analyze(List<PuzzleCellUI> cells)
    {
        var positions = cells.Select(c => c.Pos).ToList();

        int minX = positions.Min(p => p.x);
        int maxX = positions.Max(p => p.x);
        int minY = positions.Min(p => p.y);
        int maxY = positions.Max(p => p.y);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        int count = cells.Count;

        // normalize to local space
        var local = positions
            .Select(p => new Vector2Int(p.x - minX, p.y - minY))
            .ToHashSet();

        // =====================
        // SINGLE
        // =====================
        if (count == 1)
        {
            return new ShapeResult
            {
                shape = BlockShape.Square,
                orientation = BlockOrientation.Up
            };
        }

        // =====================
        // FULL RECT / SQUARE
        // =====================
        if (width * height == count)
        {
            // square
            if (width == height)
            {
                if(width == 2)
                {
                    return new ShapeResult
                    {
                        shape = BlockShape.Square2,
                        orientation = BlockOrientation.Up // irrelevant
                    };
                }
                return new ShapeResult
                {
                    shape = BlockShape.Square,
                    orientation = BlockOrientation.Up // irrelevant
                };
            }

            // rectangle
            if(width<=2)
            {
                return new ShapeResult
                {
                    shape = BlockShape.Rectangle2,
                    orientation = width > height
                  ? BlockOrientation.Right   // horizontal
                  : BlockOrientation.Up      // vertical
                };
            }
            else
            {
                return new ShapeResult
                {
                    shape = BlockShape.Rectangle3,
                    orientation = width > height
                  ? BlockOrientation.Right   // horizontal
                  : BlockOrientation.Up      // vertical
                };
            }
          
        }

        // =====================
        // L SHAPES
        // =====================
        if (count == 3 || count == 4)
        {
            var orientation = DetectLOrientation(local, width, height);
            return new ShapeResult
            {
                shape = count == 3 ? BlockShape.L3 : BlockShape.L4,
                orientation = orientation
            };
        }

        // =====================
        // FALLBACK
        // =====================
        return new ShapeResult
        {
            shape = BlockShape.Irregular,
            orientation = BlockOrientation.Up
        };
    }


    private static BlockOrientation DetectLOrientation(
        HashSet<Vector2Int> cells,
        int width,
        int height)
    {
        // check missing corner
        bool missingTL = !cells.Contains(new Vector2Int(0, height - 1));
        bool missingTR = !cells.Contains(new Vector2Int(width - 1, height - 1));
        bool missingBL = !cells.Contains(new Vector2Int(0, 0));
        bool missingBR = !cells.Contains(new Vector2Int(width - 1, 0));

        if (missingTL) return BlockOrientation.Up;
        if (missingTR) return BlockOrientation.Right;
        if (missingBR) return BlockOrientation.Down;
        return BlockOrientation.Left;
    }

   

}


public static class ShapeScrewRule
{
    public static int GetScrew(BlockShape shape)
    {
        return shape switch
        {
            BlockShape.Square => 1,
            BlockShape.Rectangle2 => 2,
            BlockShape.Rectangle3 => 3,
            BlockShape.L3 => 3,
            BlockShape.L4 => 3, // hoặc 4 nếu bạn muốn khó hơn
            _ => 1
        };
    }
}
