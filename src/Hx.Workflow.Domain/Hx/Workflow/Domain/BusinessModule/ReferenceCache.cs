namespace Hx.Workflow.Domain.BusinessModule
{
    public class ReferenceCache(string businessType, int count)
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string BusinessType { get; set; } = businessType;
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; } = count;
        public void Next()
        {
            Count++;
        }
    }
}