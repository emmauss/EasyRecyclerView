using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Annotation;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Java.Lang;
using Android.Widget;
using Android.Support.Annotation;
using Android.Util;
using Android.Support.V7.Widget;

namespace EasyRecyclerView
{
    public class EasyRecyclerView : RecyclerView
    {


        private static readonly bool HAS_ACTIVATED = Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb;

        private Adapter adapter;

        private bool inChoiceMode;
        private ChoiceState choiceState;
        private ChoiceObserver choiceObserver;
        private IChoiceModeListener choiceModeListener;

        public IOnItemClickListener OnItemClickListener;
        public IOnItemLongClickListener OnItemLongClickListener;

        public OnClickListener ItemOnClickListener;

        public OnLongClickListener ItemOnLongClickListener;

        public class OnClickListener : Java.Lang.Object, IOnClickListener
        {
            EasyRecyclerView recyclerView;
            public OnClickListener(EasyRecyclerView recyclerView)
            {
                this.recyclerView = recyclerView;
            }

            public void OnClick(View v)
            {
                RecyclerView.ViewHolder holder = recyclerView.GetChildViewHolder(v);
                if (holder != null)
                {
                    recyclerView.PerformItemClick(holder);
                }
            }
        }

        public class OnLongClickListener : Java.Lang.Object, IOnLongClickListener
        {
            EasyRecyclerView recyclerView;

            public OnLongClickListener(EasyRecyclerView recyclerView)
            {
                this.recyclerView = recyclerView;
            }

            public bool OnLongClick(View v)
            {
                RecyclerView.ViewHolder holder = recyclerView.GetChildViewHolder(v);
                if (holder != null)
                {
                    return recyclerView.PerformItemLongClick(holder);
                }
                else
                {
                    return false;
                }
            }
        }

        public EasyRecyclerView(Context context) : base(context)
        {
            init();
        }

        public EasyRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            init();
        }

        public EasyRecyclerView(Context context, IAttributeSet attrs, int defStyle) 
            : base(context, attrs, defStyle)
        {
            init();
        }

        void init()
        {
            ItemOnClickListener = new OnClickListener(this);
            ItemOnLongClickListener = new OnLongClickListener(this);
        }

        public override void SetAdapter(Adapter adapter)
        {
            if (adapter != null && !(adapter is EasyAdapter)) {
                throw new IllegalStateException("Only EasyAdapter can be set to EasyRecyclerView");
            }

            if (inChoiceMode)
            {
                OutOfChoiceMode();
            }

            this.adapter = adapter;

            base.SetAdapter(adapter);

        }

        public void SetOnItemClickListener(IOnItemClickListener listener)
        {
            OnItemClickListener = listener;
        }

        public void SetOnItemLongClickListener(IOnItemLongClickListener listener)
        {
            OnItemLongClickListener = listener;
        }

        public void SetChoiceModeListener(IChoiceModeListener listener)
        {
            choiceModeListener = listener;
        }

        public void IntoChoiceMode()
        {
            if (!inChoiceMode)
            {
                if (adapter == null)
                {
                    throw new IllegalStateException("Please set adapter first");
                }

                inChoiceMode = true;

                if (choiceState == null)
                {
                    choiceState = new ChoiceState();
                }

                if (choiceObserver == null)
                {
                    choiceObserver = new ChoiceObserver(this);
                }
                adapter.RegisterAdapterDataObserver(choiceObserver);

                if (choiceModeListener != null)
                {
                    choiceModeListener.OnIntoChoiceMode(this);
                }
            }

        }

        public void OutOfChoiceMode()
        {
            if (inChoiceMode)
            {
                inChoiceMode = false;

                choiceState.Clear();

                adapter.UnregisterAdapterDataObserver(choiceObserver);

                UncheckOnScreenViews();

                if (choiceModeListener != null)
                {
                    choiceModeListener.OnOutOfChoiceMode(this);
                }
            }
        }


        public bool IsInChoiceMode()
        {
            return inChoiceMode;
        }

        public void SetItemChecked(int position, bool value)
        {
            if (!inChoiceMode)
            {
                throw new IllegalStateException("Must call intoChoiceMode() first");
            }
            int count = adapter.ItemCount;
            if (position < 0 || position >= count)
            {
                throw new IllegalStateException("Out of range: position = " + position + ", count = " + count);
            }

            // Check old value and new value
            if (choiceState.IsChecked(position) == value)
            {
                return;
            }

            choiceState.SetChecked(position, value);

            SetViewChecked(position, value);

            if (choiceModeListener != null)
            {
                long id = adapter.GetItemId(position);
                choiceModeListener.OnItemCheckedStateChanged(this, position, id, value);
            }
        }

        public void ToggleItemChecked(int position)
        {
            if (inChoiceMode)
            {
                SetItemChecked(position, !choiceState.IsChecked(position));
            }
            else
            {
                throw new IllegalStateException("Must call intoChoiceMode() first");
            }
        }

        public void CheckAll()
        {
            if (!inChoiceMode)
            {
                throw new IllegalStateException("Must call intoChoiceMode() first");
            }

            Adapter adapter = this.adapter;
            for (int i = 0, n = adapter.ItemCount; i < n; i++)
            {
                // Skip checked item
                if (choiceState.IsChecked(i))
                {
                    continue;
                }

                choiceState.SetChecked(i, true);

                if (choiceModeListener != null)
                {
                    long id = GetAdapter().GetItemId(i);
                    choiceModeListener.OnItemCheckedStateChanged(this, i, id, true);
                }
            }

            UpdateOnScreenViews();
        }

        public bool IsItemChecked(int position)
        {
            if (!inChoiceMode)
            {
                throw new IllegalStateException("Must call intoChoiceMode() first");
            }
            return choiceState.IsChecked(position);
        }

        public int[] GetCheckedItemPositions()
        {
            if (!inChoiceMode)
            {
                throw new IllegalStateException("Must call intoChoiceMode() first");
            }
            return choiceState.GetCheckedItemPositions();
        }

        private void SetViewChecked(int position, bool check)
        {
            ViewHolder holder = FindViewHolderForAdapterPosition(position);
            if (holder != null)
            {
                SetViewChecked(holder.ItemView, check);
            }
        }

        private void UpdateOnScreenViews()
        {
            int count = ChildCount;
            for (int i = 0; i < count; i++)
            {
                View child = GetChildAt(i);
                int position = GetChildAdapterPosition(child);
                if (position >= 0)
                {
                    SetViewChecked(child, choiceState.IsChecked(position));
                }
                else
                {
                    //Log.e(LOG_TAG, "Can't get adapter position for a child in updateOnScreenViews()");
                }
            }
        }

        private void UncheckOnScreenViews()
        {
            int count = ChildCount;
            for (int i = 0; i < count; i++)
            {
                View child = GetChildAt(i);
                int position = GetChildAdapterPosition(child);
                if (position >= 0)
                {
                    SetViewChecked(child, false);
                }
                else
                {
                    //Log.e(LOG_TAG, "Can't get adapter position for a child in updateOnScreenViews()");
                }
            }
        }

        public override void OnChildAttachedToWindow(View child)
        {
            base.OnChildAttachedToWindow(child);
            if (choiceState != null)
            {
                int position = GetChildAdapterPosition(child);
                if (position >= 0)
                {
                    SetViewChecked(child, choiceState.IsChecked(position));
                }
            }
        }

        public bool PerformItemClick(int position)
        {
            ViewHolder holder = FindViewHolderForAdapterPosition(position);
            if (holder != null)
            {
                return PerformItemClick(holder);
            }
            else
            {
                return false;
            }
        }

        public bool PerformItemLongClick(int position)
        {
            ViewHolder holder = FindViewHolderForAdapterPosition(position);
            if (holder != null)
            {
                return PerformItemLongClick(holder);
            }
            else
            {
                return false;
            }
        }

        bool PerformItemClick(ViewHolder holder)
        {
            if (OnItemClickListener != null)
            {
                OnItemClickListener.OnItemClick(this, holder);
                PlaySoundEffect(SoundEffects.Click);
                return true;
            }
            else
            {
                return false;
            }
        }

        bool PerformItemLongClick(ViewHolder holder)
        {
            if (OnItemLongClickListener != null)
            {
                bool handled = OnItemLongClickListener.OnItemLongClick(this, holder);
                if (handled)
                {
                    PerformHapticFeedback(FeedbackConstants.LongPress);
                }
                return handled;
            }
            else
            {
                return false;
            }
        }
        

        public class ISavedState : Java.Lang.Object, IParcelable
        {
            public static readonly ISavedState EMPTY_STATE = new ISavedState();

            EasyRecyclerView recyclerView = null;

            public bool inChoiceMode;
            public ChoiceState choiceState;

            // This keeps the parent(RecyclerView)'s state
            public IParcelable mSuperState;

            public ISavedState()
            {
                mSuperState = null;
            }

            public ISavedState(EasyRecyclerView recyclerView)
            {
                this.recyclerView = recyclerView;
                mSuperState = null;
            }

            /**
             * Constructor called from {@link #onSaveInstanceState()}
             */
            public ISavedState(IParcelable superState, EasyRecyclerView recyclerView)
            {
                this.recyclerView = recyclerView;
                mSuperState = superState != EMPTY_STATE ? superState : null;
            }

            /**
             * Constructor called from {@link #CREATOR}
             */
            public ISavedState(Parcel ind, EasyRecyclerView recyclerView)
            {
                this.recyclerView = recyclerView;
                // Parcel 'in' has its parent(RecyclerView)'s saved state.
                // To restore it, class loader that loaded RecyclerView is required.
                IParcelable superState = (IParcelable)ind.ReadParcelable(recyclerView.Class.ClassLoader);
                mSuperState = superState != null ? superState : EMPTY_STATE;
                inChoiceMode = (ind.ReadInt() != 0);
                choiceState = ChoiceState.ReadFromParcel(ind);
            }
            

            public int DescribeContents()
            {
                return 0;
            }

            public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
            {
                dest.WriteParcelable(mSuperState, flags);
                dest.WriteInt(inChoiceMode ? 1 : 0);
                ChoiceState.WriteToParcel(choiceState, dest);
            }

            public IParcelable GetSuperState()
            {

                return mSuperState;
            }
            
        }

        protected override IParcelable OnSaveInstanceState()
        {
            ISavedState ss = new ISavedState(base.OnSaveInstanceState(),this);

            ss.inChoiceMode = inChoiceMode;
            ss.choiceState = choiceState;

            return ss;
        }


        

        protected override void OnRestoreInstanceState(IParcelable state)
        {
            ISavedState ss = (ISavedState)state;
            base.OnRestoreInstanceState(ss.GetSuperState());

            if (ss.inChoiceMode)
            {
                IntoChoiceMode();

                int[] positions = ss.choiceState.GetCheckedItemPositions();
                foreach (int position in positions)
                {
                    choiceState.SetChecked(position, true);
                    if (choiceModeListener != null)
                    {
                        long id = adapter.GetItemId(position);
                        choiceModeListener.OnItemCheckedStateChanged(this, position, id, true);
                    }
                }

                UpdateOnScreenViews();
            }
        }

        public class ChoiceObserver : RecyclerView.AdapterDataObserver
        {
            EasyRecyclerView recyclerView;

            public ChoiceObserver(EasyRecyclerView recyclerView)
            {
                this.recyclerView = recyclerView;
            }

            public override void OnChanged()
            {
                if (recyclerView.inChoiceMode)
                {
                    if (recyclerView.choiceState.OnChanged())
                    {
                        recyclerView.UpdateOnScreenViews();
                        if (recyclerView.choiceModeListener != null)
                        {
                            recyclerView.choiceModeListener.OnItemsCheckedStateChanged(recyclerView);
                        }
                    }
                }
            }

            public override void OnItemRangeChanged(int positionStart, int itemCount)
            {
                if (itemCount < 1)
                {
                    return;
                }

                if (recyclerView.inChoiceMode)
                {
                    if (recyclerView.choiceState.OnItemRangeChanged(positionStart, itemCount))
                    {
                        recyclerView.UpdateOnScreenViews();
                        if (recyclerView.choiceModeListener != null)
                        {
                            recyclerView.choiceModeListener.OnItemsCheckedStateChanged(recyclerView);
                        }
                    }
                }
            }

            public override void OnItemRangeInserted(int positionStart, int itemCount)
            {
                if (itemCount < 1)
                {
                    return;
                }

                if (recyclerView.inChoiceMode)
                {
                    if (recyclerView.choiceState.OnItemRangeInserted(positionStart, itemCount))
                    {
                        recyclerView.UpdateOnScreenViews();
                        if (recyclerView.choiceModeListener != null)
                        {
                            recyclerView.choiceModeListener.OnItemsCheckedStateChanged(recyclerView);
                        }
                    }
                }
            }

            public override void OnItemRangeRemoved(int positionStart, int itemCount)
            {
                if (itemCount < 1)
                {
                    return;
                }

                if (recyclerView.inChoiceMode)
                {
                    if (recyclerView.choiceState.OnItemRangeRemoved(positionStart, itemCount))
                    {
                        recyclerView.UpdateOnScreenViews();
                        if (recyclerView.choiceModeListener != null)
                        {
                            recyclerView.choiceModeListener.OnItemsCheckedStateChanged(recyclerView);
                        }
                    }
                }
            }

            public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount)
            {
                if (itemCount < 1 || fromPosition == toPosition)
                {
                    return;
                }
                if (itemCount != 1)
                {
                    throw new IllegalArgumentException("Moving more than 1 item is not supported yet");
                }

                if (recyclerView.inChoiceMode)
                {
                    if (recyclerView.choiceState.OnItemRangeMoved(fromPosition, toPosition))
                    {
                        recyclerView.UpdateOnScreenViews();
                        if (recyclerView.choiceModeListener != null)
                        {
                            recyclerView.choiceModeListener.OnItemsCheckedStateChanged(recyclerView);
                        }
                    }
                }
            }


        }

        public static void SetViewChecked(View view, bool check)
        {
            if (view is ICheckable) {
                ((ICheckable)view).Checked = check;
            } else if (HAS_ACTIVATED)
            {
                view.Activated = check;
            }
        }
        

        public interface IOnItemClickListener
        {

            /**
             * Callback method to be invoked when an item in this
             * {@code EasyRecyclerView} has been clicked.
             *
             * @param parent the EasyRecyclerView where the click happened
             * @param holder the ViewHolder of the view within the EasyRecyclerView that was clicked
             */
            void OnItemClick(EasyRecyclerView parent, RecyclerView.ViewHolder holder);
        }

        /**
         * Interface definition for a callback to be invoked when an item in the
         * {@code EasyRecyclerView} has been clicked and held.
         */
        public interface IOnItemLongClickListener
        {

            /**
             * Callback method to be invoked when an item in this
             * {@code EasyRecyclerView} has been clicked and held.
             *
             * @param parent the EasyRecyclerView where the click happened
             * @param holder the ViewHolder of the view within the EasyRecyclerView that was clicked
             */
            bool OnItemLongClick(EasyRecyclerView parent, RecyclerView.ViewHolder holder);
        }

        /**
         * Interface definition for a callback to be invoked when an choice action happened.
         */
        public interface IChoiceModeListener
        {

            /**
             * Callback method to be invoked when action mode starts.
             *
             * @param view the {@code EasyRecyclerView}
             */
            void OnIntoChoiceMode(EasyRecyclerView view);

            /**
             * Callback method to be invoked when action mode ends.
             *
             * @param view the {@code EasyRecyclerView}
             */
            void OnOutOfChoiceMode(EasyRecyclerView view);

            /**
             * Callback method to be invoked when an item checked state changes.
             *
             * @param view the {@code EasyRecyclerView}
             * @param position the position of the item
             * @param id the id of the view
             * @param checked the checked state of the view
             */
            void OnItemCheckedStateChanged(EasyRecyclerView view, int position, long id, bool check);

            /**
             * Callback method to be invoked when multiple item checked state changes.
             * <p>
             * It always caused by {@code Adapter.notifyXXX()}.
             * But {@code Adapter.notifyXXX()} may not cause it.
             *
             * @param view the {@code EasyRecyclerView}
             */
            void OnItemsCheckedStateChanged(EasyRecyclerView view);


        }
    }
}
