public interface IPuzzleRule
{
    bool CanMove(PuzzleBlock block);
    bool CheckWin();
}
