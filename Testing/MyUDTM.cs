using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testing
{
    /// <summary>
    /// 用來測試多筆紀錄的 ActiveRecord 類別
    /// </summary>
    [FISCA.UDT.TableName("test.udt_multi_records")]
    public class History : FISCA.UDT.ActiveRecord
    {
        [FISCA.UDT.Field(Field = "ref_student_id", Indexed = false)]
        public int StudentID { get; set; }

        [FISCA.UDT.Field(Field = "desc", Indexed = false)]
        public string Description { get; set; }

        [FISCA.UDT.Field(Field = "is_good", Indexed = false)]
        public bool IsGood { get; set; }

        [FISCA.UDT.Field(Field = "occur_date", Indexed = false)]
        public DateTime OccurDate { get; set; }
    }
}
