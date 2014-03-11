using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testing
{
    public class Program
    {
        [FISCA.MainMethod()]
        public static void Main()
        {
            //System.Windows.Forms.MessageBox.Show("Hi!");
            foreach (FISCA.Presentation.INCPanel pnl in FISCA.Presentation.MotherForm.Panels)
            {
                if (pnl.Group == "學生")
                {
                    FISCA.Presentation.NLDPanel pnlTarget = (FISCA.Presentation.NLDPanel)pnl;
                    pnlTarget.AddDetailBulider<MyDetailContent>();
                    pnlTarget.AddDetailBulider<MyMultiDetailContent>();
                }
            }
        }


    }
}
