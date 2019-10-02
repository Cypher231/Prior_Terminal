using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace StageSoakTest
{
    public static class xthread
    {
        // ICA = InvokeControlAction
        public static void ICA<t>(t cont, Action<t> action) where t : Control
        {
            if (cont.InvokeRequired)
            {
                cont.Invoke(new Action<t, Action<t>>(ICA),
                          new object[] { cont, action });
            }
            else
            {
                action(cont);
            }
        }
    }
}
