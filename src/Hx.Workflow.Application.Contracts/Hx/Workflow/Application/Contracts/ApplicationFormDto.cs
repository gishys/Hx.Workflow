﻿using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class ApplicationFormDto
    {
        /// <summary>
        /// if null is root
        /// </summary>
        public Guid? ParentId { get; set; }
        /// <summary>
        /// example:001.001.001
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// application name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// application node type
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// application type
        /// </summary>
        public ApplicationType ApplicationType { get; set; }
        public int SequenceNumber { get; set; }
        public virtual ICollection<WkParamDto> Params { get; set; }
    }
}
