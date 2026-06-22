using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain.Stats
{
    public class BusinessStat : Entity<Guid>
    {
        public BusinessStat() { }
        public BusinessStat(
            Guid id,
            string statType,
            double statistics,
            string primaryClassification = null,
            string secondaryClassification = null,
            string threeLevelClassification = null,
            Guid? owner = null)
        {
            Id = id;
            StatType = statType;
            PrimaryClassification = primaryClassification;
            SecondaryClassification = secondaryClassification;
            ThreeLevelClassification = threeLevelClassification;
            Statistics = statistics;
            Owner = owner;
        }
        /// <summary>
        /// 统计类型
        /// </summary>
        public virtual string StatType { get; protected set; }
        /// <summary>
        /// 一级分类
        /// </summary>
        public virtual string PrimaryClassification { get; protected set; }
        /// <summary>
        /// 二级分类
        /// </summary>
        public virtual string SecondaryClassification { get; protected set; }
        /// <summary>
        /// 三级分类
        /// </summary>
        public virtual string ThreeLevelClassification { get; protected set; }
        /// <summary>
        /// 统计
        /// </summary>
        public virtual double Statistics { get; protected set; }
        /// <summary>
        /// 拥有者
        /// </summary>
        public virtual Guid? Owner { get; protected set; }
        /// <summary>
        /// 设置统计数据
        /// </summary>
        /// <param name="statistics"></param>
        /// <returns></returns>
        public Task SetStatistics(double statistics)
        {
            Statistics = statistics;
            return Task.CompletedTask;
        }
    }
}