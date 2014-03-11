using System;
using System.Collections.Generic;
using System.Text;
using FISCA.UDT;

namespace Testing
{
    [FISCA.UDT.TableName("test.udt_detailcontent_base")]
   public  class MyUDT: FISCA.UDT.ActiveRecord
    {
        [FISCA.UDT.Field(Field = "ref_student_id", Indexed = false)]
        public int StudentID { get; set; }

        [FISCA.UDT.Field(Field = "first_name", Indexed = false)]
        public string FirstName { get; set; }

        [FISCA.UDT.Field(Field = "gender", Indexed = false)]
        public bool Gender { get; set; }

        [FISCA.UDT.Field(Field = "country", Indexed = false)]
        public string Country { get; set; }
    }
}
