using System.Collections.Generic;

namespace SetGame.Core
{
    public static class SetValidator
    {
        public static bool IsValidSet(CardData a, CardData b, CardData c)
        {
            return AllSameOrAllDiff((int)a.Shape,   (int)b.Shape,   (int)c.Shape)
                && AllSameOrAllDiff((int)a.Color,   (int)b.Color,   (int)c.Color)
                && AllSameOrAllDiff((int)a.Count,   (int)b.Count,   (int)c.Count)
                && AllSameOrAllDiff((int)a.Shading, (int)b.Shading, (int)c.Shading);
        }

        static bool AllSameOrAllDiff(int x, int y, int z)
        {
            if (x == y && y == z) return true;
            if (x != y && y != z && x != z) return true;
            return false;
        }

        public static bool BoardContainsSet(IList<CardData> board)
        {
            int n = board.Count;
            for (int i = 0; i < n - 2; i++)
            for (int j = i + 1; j < n - 1; j++)
            for (int k = j + 1; k < n; k++)
                if (IsValidSet(board[i], board[j], board[k])) return true;
            return false;
        }

        public static (int i, int j, int k)? FindHint(IList<CardData> board)
        {
            int n = board.Count;
            for (int i = 0; i < n - 2; i++)
            for (int j = i + 1; j < n - 1; j++)
            for (int k = j + 1; k < n; k++)
                if (IsValidSet(board[i], board[j], board[k]))
                    return (i, j, k);
            return null;
        }
    }
}
