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
using Java.Lang;
using Java.Util;
using Android.Support.V7.Widget;

namespace EasyRecyclerView
{
    public class ChoiceState
    {

        private OrderedIntArray array = new OrderedIntArray();

        public ChoiceState(): this(new OrderedIntArray())
        {
            
        }

        public ChoiceState(OrderedIntArray array)
        {
            this.array = array;
        }

        /**
         * Returns {@code true} if the view in the position is checked.
         */
        public bool IsChecked(int position)
        {
            return array.Contains(position);
        }

        /**
         * Set checked state for special position.
         */
        public void SetChecked(int position, bool check)
        {
            if (check) {
                array.Add(position);
            }
            else
            {
                array.Remove(position);
            }
        }

        /**
         * Clear check state.
         */
        public void Clear()
        {
            array.Clear();
        }

        /**
         * Return the count of checked item.
         */
        public int GetCheckedItemCount()
        {
            return array.size;
        }

        /**
         * Return all position of checked item in array format.
         */
        public int[] GetCheckedItemPositions()
        {
            return array.ToArray();
        }

        /**
         * Calls it when {@link RecyclerView.Adapter#notifyDataSetChanged()} called.
         * Returns {@code true} if check state changes.
         */
        public bool OnChanged()
        {
            if (array.size != 0)
            {
                array.Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

        /**
         * Calls it when {@link RecyclerView.Adapter#notifyItemChanged(int)} called.
         * Returns {@code true} if check state changes.
         */
        public bool OnItemRangeChanged(int positionStart, int itemCount)
        {
            // Get affected position range
            int boundLeft = array.IndexOf(positionStart);
            if (boundLeft < 0)
            {
                boundLeft = ~boundLeft;
            }
            int boundRight = array.IndexOf(positionStart + itemCount - 1);
            if (boundRight < 0)
            {
                boundRight = (~boundRight) - 1;
            }
            if (boundLeft > boundRight || boundLeft >= array.size)
            {
                // No affected position range
                return false;
            }

            // Remove changed range
            array.RemoveRange(boundLeft, boundRight - boundLeft + 1);
            return true;
        }

        /**
         * Calls it when {@link RecyclerView.Adapter#notifyItemInserted(int)} called.
         * Returns {@code true} if check state changes.
         */
        public bool OnItemRangeInserted(int positionStart, int itemCount)
        {
            int index = array.IndexOf(positionStart);
            if (index < 0)
            {
                index = ~index;
            }
            if (index >= array.size)
            {
                return false;
            }

            array.IncreaseRange(index, array.size - index, itemCount);
            return true;
        }

        /**
         * Calls it when {@link RecyclerView.Adapter#notifyItemRemoved(int)} called.
         * Returns {@code true} if check state changes.
         */
        public bool OnItemRangeRemoved(int positionStart, int itemCount)
        {
            bool result = false;

            // Get affected position range
            int boundLeft = array.IndexOf(positionStart);
            if (boundLeft < 0)
            {
                boundLeft = ~boundLeft;
            }
            int boundRight = array.IndexOf(positionStart + itemCount - 1);
            if (boundRight < 0)
            {
                boundRight = (~boundRight) - 1;
            }
            if (boundLeft >= array.size)
            {
                // No affected position range
                return false;
            }

            // Remove removed range
            if (boundLeft <= boundRight)
            {
                result = true;
                array.RemoveRange(boundLeft, boundRight - boundLeft + 1);
            }

            //decrease following position
            if (boundLeft < array.size)
            {
                result = true;
                array.IncreaseRange(boundLeft, array.size - boundLeft, -itemCount);
            }

            return result;
        }

        /**
         * Calls it when {@link RecyclerView.Adapter#notifyItemMoved(int, int)} called.
         * Returns {@code true} if check state changes.
         */
        public bool OnItemRangeMoved(int fromPosition, int toPosition)
        {
            bool result = false;
            int index;
            int boundLeft;
            int boundRight;
            int diff;
            if (fromPosition < toPosition)
            {
                diff = -1;
                boundLeft = array.IndexOf(fromPosition);
                if (boundLeft < 0)
                {
                    index = -1;
                    boundLeft = ~boundLeft;
                }
                else
                {
                    index = boundLeft;
                    boundLeft += 1;
                }
                boundRight = array.IndexOf(toPosition);
                if (boundRight < 0)
                {
                    boundRight = (~boundRight) - 1;
                }

                if (index != -1)
                {
                    result = true;
                    array.RemoveAt(index);
                    --boundLeft;
                    --boundRight;
                }
                if (boundLeft <= boundRight)
                {
                    result = true;
                    array.IncreaseRange(boundLeft, boundRight - boundLeft + 1, diff);
                }
                if (index != -1)
                {
                    result = true;
                    array.Add(toPosition);
                }
            }
            else
            {
                diff = 1;
                boundLeft = array.IndexOf(toPosition);
                if (boundLeft < 0)
                {
                    boundLeft = ~boundLeft;
                }
                boundRight = array.IndexOf(fromPosition);
                if (boundRight <= 0)
                {
                    index = -1;
                    boundRight = (~boundRight) - 1;
                }
                else
                {
                    index = boundRight;
                    boundRight -= 1;
                }

                if (index != -1)
                {
                    result = true;
                    array.RemoveAt(index);
                }
                if (boundLeft <= boundRight)
                {
                    result = true;
                    array.IncreaseRange(boundLeft, boundRight - boundLeft + 1, diff);
                }
                if (index != -1)
                {
                    result = true;
                    array.Add(toPosition);
                }
            }

            return result;
        }

        /**
         * Save {@code ChoiceState} to {@code Parcel}.
         */
        public static void WriteToParcel(ChoiceState state, Parcel outp)
        {
            if (state == null)
            {
      outp.WriteInt(-1);
            }
            else
            {
                int size = state.array.size;
                int[] array = state.array.array;
      outp.WriteInt(state.array.size);
                for (int i = 0; i < size; ++i)
                {
        outp.WriteInt(array[i]);
                }
            }
        }

        /**
         * Read {@code ChoiceState} from {@code Parcel}.
         */
        public static ChoiceState ReadFromParcel(Parcel inp)
        {
            int size = inp.ReadInt();
            if (size == -1)
            {
                return null;
            }
            else
            {
                OrderedIntArray array = new OrderedIntArray(size);
                int[] a = array.array;
                for (int i = 0; i < size; ++i)
                {
                    a[i] = inp.ReadInt();
                }
                return new ChoiceState(array);
            }
        }
        public class OrderedIntArray
        {
            public int[] array;
            public int size;

            public OrderedIntArray():this(10)
            {
                
            }

            public OrderedIntArray(int initialCapacity)
            {
                initialCapacity = ContainerHelpers.IdealIntArraySize(initialCapacity);
                array = new int[initialCapacity];
                size = 0;
            }

            public void Clear()
            {
                size = 0;
            }

            public void Add(int value)
            {
                int index = ContainerHelpers.BinarySearch(array, size, value);
                if (index < 0)
                {
                    index = ~index;
                    array = ContainerHelpers.Insert(array, size, index, value);
                    size++;
                }
            }

            public bool Remove(int value)
            {
                int index = ContainerHelpers.BinarySearch(array, size, value);
                if (index >= 0)
                {
                    RemoveAt(index);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void RemoveAt(int index)
            {
                Array.Copy(array, index + 1, array, index, size - (index + 1));
                size--;
            }

            public void RemoveRange(int index, int count)
            {
                Array.Copy(array, index + count, array, index, size - (index + count));
                size -= count;
            }

            public int IndexOf(int value)
            {
                return ContainerHelpers.BinarySearch(array, size, value);
            }

            public void IncreaseRange(int index, int count, int diff)
            {
                for (int i = index, n = index + count; i < n; i++)
                {
                    array[i] += diff;
                }
            }

            public int[] ToArray()
            {
                return Arrays.CopyOfRange(array, 0, size);
            }

            public bool Contains(int value)
            {
                return ContainerHelpers.BinarySearch(array, size, value) >= 0;
            }
        }
    }

    
}