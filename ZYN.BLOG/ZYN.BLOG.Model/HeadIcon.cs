//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZYN.BLOG.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class HeadIcon
    {
        public HeadIcon()
        {
            this.Visitors = new HashSet<Visitor>();
        }
    
        public int Id { get; set; }
        public string IconName { get; set; }
        public string IconRawName { get; set; }
        public string IconURL { get; set; }
        public short Status { get; set; }
        public System.DateTime UploadTime { get; set; }
    
        public virtual ICollection<Visitor> Visitors { get; set; }
    }
}