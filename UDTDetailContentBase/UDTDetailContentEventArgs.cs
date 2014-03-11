using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;

namespace UDTDetailContentBase
{
    public class UDTDetailContentEventArgs<T> : EventArgs where T : ActiveRecord
    {
        public List<T> CurrentTargets { get; set; }
        public bool Canceled { get; set; }
        
    }
}
