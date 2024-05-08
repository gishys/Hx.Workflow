using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Domain.JsonDefinition
{
    public class JDefinitionSource
    {
        public JDefinitionSource()
        {
            Steps = new List<JStepSource>();
        }
        public string Id { get; set; }
        public int Version { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public List<JStepSource> Steps { get; set; }
    }
}