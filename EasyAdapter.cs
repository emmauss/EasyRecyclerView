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
using Java.Lang;
using VH = Android.Support.V7.Widget.RecyclerView.ViewHolder;

namespace EasyRecyclerView
{
    public abstract class EasyAdapter: RecyclerView.Adapter
    {
        EasyRecyclerView recyclerView;

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            base.OnAttachedToRecyclerView(recyclerView);
            if (!(recyclerView is EasyRecyclerView)) {
                throw new IllegalStateException("EasyAdapter can only be attached to EasyRecyclerView");
            }
            if (this.recyclerView != null)
            {
                throw new IllegalStateException("The EasyAdapter is already attached a EasyRecyclerView");
            }
            this.recyclerView = (EasyRecyclerView)recyclerView;
        }

        public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
        {
            base.OnDetachedFromRecyclerView(recyclerView);
            this.recyclerView = null;
        }

        public sealed override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (recyclerView == null)
            {
                throw new IllegalStateException("The EasyAdapter is not attached a EasyRecyclerView");
            }
            VH viewHolder = OnCreateViewHolder2(parent, viewType);
            View view = viewHolder.ItemView;
            view.SetOnClickListener(recyclerView.ItemOnClickListener);
            view.SetOnLongClickListener(recyclerView.ItemOnLongClickListener);
            // Let EasyRecyclerView handle SoundEffects and HapticFeedback
            view.SoundEffectsEnabled = false;
            view.HapticFeedbackEnabled = false;
            return viewHolder;
        }

        public abstract RecyclerView.ViewHolder OnCreateViewHolder2(ViewGroup parent, int viewType);
    }
}