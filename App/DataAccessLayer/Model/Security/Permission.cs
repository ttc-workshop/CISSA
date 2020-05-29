namespace Intersoft.CISSA.DataAccessLayer.Model.Security
{
    public class BizPermission
    {
        public bool AllowSelect { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowInsert { get; set; }
        public bool AllowUpdate { get; set; }

        public static BizPermission operator +(BizPermission per1, BizPermission per2)
        {
            var tmp = new BizPermission
                          {
                              AllowSelect = per1.AllowSelect || per2.AllowSelect,
                              AllowDelete = per1.AllowDelete || per2.AllowDelete,
                              AllowUpdate = per1.AllowUpdate || per2.AllowUpdate,
                              AllowInsert = per1.AllowInsert || per2.AllowInsert
                          };
            return tmp;
        }
        
        public BizPermission()
        {
        }

        public BizPermission(bool defValue)
        {
            AllowSelect = AllowDelete = AllowInsert = AllowUpdate = defValue;
        }
    }
}
