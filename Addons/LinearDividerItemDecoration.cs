using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Java.Lang;
using Android.Widget;
using Android.Graphics;
using Android.Support.V4.View;
using Android.Support.V7.Widget;

namespace EasyRecyclerView.Addons
{
    public class LinearDividerItemDecoration : RecyclerView.ItemDecoration
    {
        public static readonly int HORIZONTAL = LinearLayoutManager.Horizontal;
        public static readonly int VERTICAL = LinearLayoutManager.Vertical;

        private bool showFirstDivider = false;
        private bool showLastDivider = false;

        private readonly Rect rect;
  private readonly Paint paint;

  private int orientation;
        private int thickness;
        private int paddingStart = 0;
        private int paddingEnd = 0;

        private bool overlap = false;

        private ShowDividerHelper showDividerHelper;

        public LinearDividerItemDecoration(int orientation, int color, int thickness)
        {
            rect = new Rect();
            paint = new Paint();
            paint.SetStyle(Paint.Style.Fill);
            SetOrientation(orientation);
            SetColor(color);
            SetThickness(thickness);
        }

        /**
         * Let {@code ShowDividerHelper} decide whether divider.
         */
        public void SetShowDividerHelper(ShowDividerHelper helper)
        {
            showDividerHelper = helper;
        }

        /**
         * Orientation of the {@link android.support.v7.widGet.LinearLayoutManager}.
         */
        public void SetOrientation(int orientation)
        {
            if (orientation != HORIZONTAL && orientation != VERTICAL)
            {
                throw new IllegalArgumentException("invalid orientation");
            }
            this.orientation = orientation;
        }

        /**
         * Color of dividers.
         */
        public void SetColor(int color)
        {
            paint.Color = new Color(color);
        }

        /**
         * Thickness of dividers.
         */
        public void SetThickness(int thickness)
        {
            this.thickness = thickness;
        }

        /**
         * Whether draw divider before the first item.
         */
        public void SetShowFirstDivider(bool showFirstDivider)
        {
            this.showFirstDivider = showFirstDivider;
        }

        /**
         * Whether draw divider after the last item.
         */
        public void SetShowLastDivider(bool showLastDivider)
        {
            this.showLastDivider = showLastDivider;
        }

        /**
         * Padding of divider.
         */
        public void SetPadding(int padding)
        {
            SetPaddingStart(padding);
            SetPaddingEnd(padding);
        }

        /**
         * Left padding for {@link #VERTICAL}.
         * Top padding for {@link #HORIZONTAL}.
         * Supports RTL.
         */
        public void SetPaddingStart(int paddingStart)
        {
            this.paddingStart = paddingStart;
        }

        /**
         * Right padding for {@link #VERTICAL}.
         * Bottom padding for {@link #HORIZONTAL}.
         * Supports RTL.
         */
        public void SetPaddingEnd(int paddingEnd)
        {
            this.paddingEnd = paddingEnd;
        }

        /**
         * Whether draw divider over views.
         */
        public void SetOverlap(bool overlap)
        {
            this.overlap = overlap;
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            if (parent.GetAdapter() == null)
            {
                // Can't Get view position, return empty rect
                outRect.Set(0, 0, 0, 0);
                return;
            }

            if (overlap)
            {
                // Overlap, return empty rect
                outRect.Set(0, 0, 0, 0);
                return;
            }

            int position = parent.GetChildLayoutPosition(view);
            int itemCount = parent.GetAdapter().ItemCount;

            outRect.Set(0, 0, 0, 0);
            if (showDividerHelper != null)
            {
                if (orientation == VERTICAL)
                {
                    if (position == 0 && showDividerHelper.ShowDivider(0))
                    {
                        outRect.Top = thickness;
                    }
                    if (showDividerHelper.ShowDivider(position + 1))
                    {
                        outRect.Bottom = thickness;
                    }
                }
                else
                {
                    if (position == 0 && showDividerHelper.ShowDivider(0))
                    {
                        outRect.Left = thickness;
                    }
                    if (showDividerHelper.ShowDivider(position + 1))
                    {
                        outRect.Right = thickness;
                    }
                }
            }
            else
            {
                if (orientation == VERTICAL)
                {
                    if (position == 0 && showFirstDivider)
                    {
                        outRect.Top = thickness;
                    }
                    if ((position != itemCount - 1) || showLastDivider)
                    {
                        outRect.Bottom = thickness;
                    }
                }
                else
                {
                    if (position == 0 && showFirstDivider)
                    {
                        outRect.Left = thickness;
                    }
                    if ((position != itemCount - 1) || showLastDivider)
                    {
                        outRect.Right = thickness;
                    }
                }
            }

        }

        public override void OnDrawOver(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            RecyclerView.Adapter adapter = parent.GetAdapter();
            if (adapter == null)
            {
                return;
            }

            int itemCount = adapter.ItemCount;

            if (orientation == VERTICAL)
            {
                 bool isRtl = ViewCompat.GetLayoutDirection(parent) == ViewCompat.LayoutDirectionRtl;
                int paddingLeft;
                int paddingRight;
                if (isRtl)
                {
                    paddingLeft = paddingEnd;
                    paddingRight = paddingStart;
                }
                else
                {
                    paddingLeft = paddingStart;
                    paddingRight = paddingEnd;
                }

                 int left = parent.PaddingLeft + paddingLeft;
                 int right = parent.Width - parent.PaddingRight - paddingRight;
                 int childCount = parent.ChildCount;

                for (int i = 0; i < childCount; i++)
                {
                     View child = parent.GetChildAt(i);
                     RecyclerView.LayoutParams lp = (RecyclerView.LayoutParams)child.LayoutParameters;
                     int position = parent.GetChildLayoutPosition(child);

                    bool show;
                    if (showDividerHelper != null)
                    {
                        show = showDividerHelper.ShowDivider(position + 1);
                    }
                    else
                    {
                        show = (position != itemCount - 1) || showLastDivider;
                    }
                    if (show)
                    {
                        int top = child.Bottom + lp.BottomMargin;
                        if (overlap)
                        {
                            top -= thickness;
                        }
                         int bottom = top + thickness;
                        rect.Set(left, top, right, bottom);
                        c.DrawRect(rect, paint);
                    }

                    if (position == 0)
                    {
                        if (showDividerHelper != null)
                        {
                            show = showDividerHelper.ShowDivider(0);
                        }
                        else
                        {
                            show = showFirstDivider;
                        }
                        if (show)
                        {
                            int bottom = child.Top + lp.TopMargin;
                            if (overlap)
                            {
                                bottom += thickness;
                            }
                             int top = bottom - thickness;
                            rect.Set(left, top, right, bottom);
                            c.DrawRect(rect, paint);
                        }
                    }
                }
            }
            else
            {
                 int top = parent.PaddingTop + paddingStart;
                 int bottom = parent.Height - parent.PaddingBottom - paddingEnd;
                 int childCount = parent.ChildCount;

                for (int i = 0; i < childCount; i++)
                {
                     View child = parent.GetChildAt(i);
                     RecyclerView.LayoutParams lp = (RecyclerView.LayoutParams)child.LayoutParameters;
                     int position = parent.GetChildLayoutPosition(child);

                    bool show;
                    if (showDividerHelper != null)
                    {
                        show = showDividerHelper.ShowDivider(position + 1);
                    }
                    else
                    {
                        show = (position != itemCount - 1) || showLastDivider;
                    }
                    if (show)
                    {
                        int left = child.Right + lp.RightMargin;
                        if (overlap)
                        {
                            left -= thickness;
                        }
                         int right = left + thickness;
                        rect.Set(left, top, right, bottom);
                        c.DrawRect(rect, paint);
                    }

                    if (position == 0)
                    {
                        if (showDividerHelper != null)
                        {
                            show = showDividerHelper.ShowDivider(0);
                        }
                        else
                        {
                            show = showFirstDivider;
                        }
                        if (show)
                        {
                            int right = child.Left + lp.LeftMargin;
                            if (overlap)
                            {
                                right += thickness;
                            }
                             int left = right - thickness;
                            rect.Set(left, top, right, bottom);
                            c.DrawRect(rect, paint);
                        }
                    }
                }
            }
        }


        public interface ShowDividerHelper
        {

            /**
             * Whether draw divider for specialized index.
             * <p>
             * The divider before first item is index 0.
             * The divider before first item is index count.
             */
            bool ShowDivider(int index);
        }

    }
}