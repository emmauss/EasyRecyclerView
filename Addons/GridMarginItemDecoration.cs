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
using Android.Support.V7.Widget;
using Android.Graphics;

namespace EasyRecyclerView.Addons
{
    public class GridMarginItemDecoration : RecyclerView.ItemDecoration
    {
        private int margin;

        public GridMarginItemDecoration(int margin)
        {
            this.margin = margin;
        }

        public void SetMargin(int margin)
        {
            this.margin = margin;
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            RecyclerView.LayoutManager layoutManager = parent.GetLayoutManager();
            if (!(layoutManager is GridLayoutManager))
            {
                outRect.Set(0, 0, 0, 0);
                return;
            }

            RecyclerView.Adapter adapter = parent.GetAdapter();
            if (adapter == null)
            {
                outRect.Set(0, 0, 0, 0);
                return;
            }

            int position = parent.GetChildLayoutPosition(view);
            if (position == -1)
            {
                outRect.Set(0, 0, 0, 0);
                return;
            }

            GridLayoutManager glm = (GridLayoutManager)layoutManager;
            int size = adapter.ItemCount;
            int span = glm.SpanCount;
            int spanIndex = position % span;
            int spanGroup = position / span;

            if (spanIndex == 0)
            {
                outRect.Left = margin;
            }
            else
            {
                outRect.Left = (margin + 1) / 2;
            }
            if (spanIndex == span - 1)
            {
                outRect.Right = margin;
            }
            else
            {
                outRect.Right = margin / 2;
            }
            if (spanGroup == 0)
            {
                outRect.Top = margin;
            }
            else
            {
                outRect.Top = (margin + 1) / 2;
            }
            if (spanGroup == (size - 1) / span)
            {
                outRect.Bottom = margin;
            }
            else
            {
                outRect.Bottom = margin / 2;
            }
        }

    }
}