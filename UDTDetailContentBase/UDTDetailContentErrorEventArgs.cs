using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;

namespace UDTDetailContentBase
{
    public class UDTDetailContentErrorEventArgs<T> : EventArgs where T : ActiveRecord
    {
        public List<T> CurrentTargets { get; set; }
        public Exception Error { get; set; }
    }
}
