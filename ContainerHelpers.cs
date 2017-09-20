using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace EasyRecyclerView
{
    class ContainerHelpers
    {
        public static int IdealIntArraySize(int need)
        {
            return IdealByteArraySize(need * 4) / 4;
        }

        public static int IdealByteArraySize(int need)
        {
            for (int i = 4; i < 32; i++)
                if (need <= (1 << i) - 12)
                    return (1 << i) - 12;

            return need;
        }

        public static int BinarySearch(int[] array, int size, int value)
        {
            int lo = 0;
            int hi = size - 1;

            while (lo <= hi)
            {
                int mid = (int)(((uint)(lo + hi)) >> 1);
                int midVal = array[mid];

                if (midVal < value)
                {
                    lo = mid + 1;
                }
                else if (midVal > value)
                {
                    hi = mid - 1;
                }
                else
                {
                    return mid;  // value found
                }
            }
            return ~lo;  // value not present
        }

        public static int[] Insert(int[] array, int currentSize, int index, int element)
        {
            if (currentSize + 1 <= array.Length)
            {
                
                Array.Copy(array, index, array, index + 1, currentSize - index);
                array[index] = element;
                return array;
            }

            int[] newArray = new int[IdealIntArraySize(GrowSize(currentSize))];
            Array.Copy(array, 0, newArray, 0, index);
            newArray[index] = element;
            Array.Copy(array, index, newArray, index + 1, array.Length - index);
            return newArray;
        }

        public static int GrowSize(int currentSize)
        {
            return currentSize <= 4 ? 8 : currentSize * 2;
        }
    }
}