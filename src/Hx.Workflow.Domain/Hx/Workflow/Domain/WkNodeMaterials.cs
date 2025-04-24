using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain
{
    public class WkNodeMaterials
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkNodeMaterials() { }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkNodeMaterials(int attachReceiveType, int referenceType, string catalogueName, int sequenceNumber, bool isRequired, bool isStatic, bool isVerification, bool verificationPassed)
        {
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
        /// 附件收取类型
        /// </summary>
        public int AttachReceiveType { get;protected set; }
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
        public ICollection<WkNodeMaterials> Children { get; protected set; } = [];
        public void AddChild(WkNodeMaterials child) { Children.Add(child); }
    }
}
