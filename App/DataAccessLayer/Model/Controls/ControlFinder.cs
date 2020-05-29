using System;
using System.Collections.Generic;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    public class ControlFinder
    {
        public BizControl Control { get; private set; }

        public ControlFinder(BizControl control)
        {
            Control = control;
        }

        public BizControl Find(Guid id)
        {
            if (Control.Id == id) return Control;

            if (Control.Children != null)
                return FindIn(Control.Children, id);

            return null;
        }

        public void ForEach(Action<BizControl> action)
        {
            if (action == null || Control == null) return;

            action.Invoke(Control);

            if (Control.Children != null)
                ForEachIn(Control.Children, action);
        }

        public BizControl FirstOrDefault(Predicate<BizControl> predicate)
        {
            if (predicate == null || Control == null) return null;

            if (predicate.Invoke(Control)) return Control;

            if (Control.Children != null)
                return FirstOrDefaultIn(Control.Children, predicate);
            return null;
        }

        public static void ForEachIn(ICollection<BizControl> controls, Action<BizControl> action)
        {
            if (action == null) return;

            foreach (var child in controls)
            {
                action.Invoke(child);

                if (child.Children != null)
                    ForEachIn(child.Children, action);
            }
        }

        public static BizControl FirstOrDefaultIn(ICollection<BizControl> controls, Predicate<BizControl> predicate)
        {
            if (predicate == null) return null;

            foreach (var child in controls)
            {
                if (predicate.Invoke(child)) return child;

                if (child.Children != null)
                {
                    var cc = FirstOrDefaultIn(child.Children, predicate);
                    if (cc != null) return cc;
                }
            }
            return null;
        }

        public static BizControl FindIn(ICollection<BizControl> controls, Guid id)
        {
            foreach (var child in controls)
            {
                if (child.Id == id)
                    return child;

                if (child.Children != null)
                {
                    var childOfChild = FindIn(child.Children, id);

                    if (childOfChild != null) 
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
