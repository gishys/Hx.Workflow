using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkExecutionPointerMaterialsDto
    {
        /// <summary>
        /// 业务类型Id
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// 附件收取类型
        /// </summary>
        public int AttachReceiveType { get; set; }
        /// <summary>
        /// 业务类型标识
        /// </summary>
        public int ReferenceType { get; set; }
        /// <summary>
        /// 目录名称
        /// </summary>
        public string CatalogueName { get; set; }
        /// <summary>
        /// 顺序号
        /// </summary>
        public int SequenceNumber { get; set; }
        /// <summary>
        /// 是否必收
        /// </summary>
        public bool IsRequired { get; set; }
        /// <summary>
        /// 静态标识
        /// </summary>
        public bool IsStatic { get; set; }
        /// <summary>
        /// 是否核验
        /// </summary>
        public bool IsVerification { get; set; }
        /// <summary>
        /// 核验通过
        /// </summary>
        public bool VerificationPassed { get; set; }
        /// <summary>
        /// 子文件夹
        /// </summary>
        public ICollection<WkExecutionPointerMaterialsDto> Children { get; set; }
    }
}
