using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.BusinessModule
{
    public class ReferenceCache
    {
        public ReferenceCache(string businessType, int count)
        {
            BusinessType = businessType;
            Count = count;
        }
        /// <summary>
        /// 日期
        /// </summary>
        public string BusinessType { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }
        public void Next()
        {
            Count++;
        }
    }
}