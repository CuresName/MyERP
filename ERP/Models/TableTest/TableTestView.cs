using Microsoft.AspNetCore.Mvc.Rendering;
using ERP.TableTest;

namespace ERP.Models.TableTest
{
    public class TableTestView
    {
        public Table_1 Table_1 { get; set; }
        public Table_1 Table_1_preview { get; set; }
        public List<SelectListItem> Table_1_selectitem { get; set; }
        public List<Table_2> Table_2 { get; set; }
        public List<Table_3> Table_3 { get; set; }
        public List<Table_4> Table_4 { get; set; }
        public List<Table_5> Table_5 { get; set; }
    }
}
