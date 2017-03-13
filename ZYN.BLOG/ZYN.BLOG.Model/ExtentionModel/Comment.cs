using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.Model.ExtentionModel
{
    [MetadataType(typeof(CommentValidate))]
    public partial class Comment
    {

    }

    public class CommentValidate
    {
        [StringLength(500,ErrorMessage="评论内容不要超过500个字符")]
        public string CmtText { get; set; }
    }
}
