using System;

namespace Hx.Workflow.Application.Contracts
{
    public class BusinessStatDto
    {
        /// <summary>
        /// 统计类型
        /// </summary>
        public string StatType { get; set; }
        /// <summary>
        /// 一级分类
        /// </summary>
        public string PrimaryClassification { get; set; }
        /// <summary>
        /// 二级分类
        /// </summary>
        public string SecondaryClassification { get; set; }
        /// <summary>
        /// 三级分类
        /// </summary>
        public string ThreeLevelClassification { get; set; }
        /// <summary>
        /// 统计
        /// </summary>
        public double Statistics { get; set; }
        /// <summary>
        /// 拥有者
        /// </summary>
        public Guid? Owner { get; set; }
    }
}
