using System;
using System.Collections.Generic;

public static class SortExtensions 
{
    public static void BubbleSort<T>(this List<T> list) where T : IComparable<T>
    {
        int n = list.Count;
        bool swapped;
        do
        {
            swapped = false;
            for (int i = 1; i < n; i++)
            {
                if (list[i - 1].CompareTo(list[i]) > 0)
                {
                    // Swap the elements
                    T temp = list[i - 1];
                    list[i - 1] = list[i];
                    list[i] = temp;
                    swapped = true;
                }
            }
            // Reduce the range for the next pass
            n--;
        } while (swapped);
    }
}
