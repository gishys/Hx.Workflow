using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.Persistence
{
    public class WkExecutionPointerMaterials
    {
        public WkExecutionPointerMaterials() { }
        public WkExecutionPointerMaterials(string reference, int attachReceiveType, int referenceType, string catalogueName, int sequenceNumber, bool isRequired, bool isStatic, bool isVerification, bool verificationPassed)
        {
            Reference = reference;
            AttachReceiveType = attachReceiveType;
            ReferenceType = referenceType;
            CatalogueName = catalogueName;
            SequenceNumber = sequenceNumber;
            IsRequired = isRequired;
            IsStatic = isStatic;
            IsVerification = isVerification;
            VerificationPassed = verificationPassed;
        }

        /// <summary>
        /// 业务号
        /// </summary>
        public string Reference { get; protected set; }
        /// <summary>
        /// 附件收取类型
        /// </summary>
        public int AttachReceiveType { get; protected set; }
        /// <summary>
        /// 业务类型标识
        /// </summary>
        public int ReferenceType { get; protected set; }
        /// <summary>
        /// 目录名称
        /// </summary>
        public string CatalogueName { get; protected set; }
        /// <summary>
        /// 顺序号
        /// </summary>
        public int SequenceNumber { get; protected set; }
        /// <summary>
        /// 是否必收
        /// </summary>
        public bool IsRequired { get; protected set; }
        /// <summary>
        /// 静态标识
        /// </summary>
        public bool IsStatic { get; protected set; }
        /// <summary>
        /// 是否核验
        /// </summary>
        public bool IsVerification { get; protected set; }
        /// <summary>
        /// 核验通过
        /// </summary>
        public bool VerificationPassed { get; protected set; }
        /// <summary>
        /// 子文件夹
        /// </summary>
        public ICollection<WkExecutionPointerMaterials> Children { get; protected set; } = new List<WkExecutionPointerMaterials>();
        public void AddChild(WkExecutionPointerMaterials child) { Children.Add(child); }
    }
}
