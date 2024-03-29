﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class WkDefinitionCreateDto
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 流程定义名称
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// get or set icon
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// get or set color
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// get or set color
        /// </summary>
        public int SortNumber { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Discription { get; set; }
        public ICollection<WkNodeCreateDto> Nodes { get; set; }
    }
}