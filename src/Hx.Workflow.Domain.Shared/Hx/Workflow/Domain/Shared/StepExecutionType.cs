﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.Shared
{
    public enum StepExecutionType
    {
        /// <summary>
        /// 提交
        /// </summary>
        Forward = 1,
        /// <summary>
        /// 退回
        /// </summary>
        RolledBack = 2
    }
}