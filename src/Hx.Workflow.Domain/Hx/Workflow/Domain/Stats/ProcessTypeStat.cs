using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.Stats
{
    public class ProcessTypeStat
    {
        /// <summary>
        /// 一级分类
        /// </summary>
        public string PClassification { get; set; }
        /// <summary>
        /// 二级分类
        /// </summary>
        public string SClassification { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }
    }
}
