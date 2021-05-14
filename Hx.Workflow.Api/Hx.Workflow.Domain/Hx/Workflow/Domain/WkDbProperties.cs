using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Domain
{
    public class WkDbProperties
    {
        public static string DbTablePrefix { get; set; } = "HX";
        public static string DbSchema { get; set; } = null;

        public const string ConnectionStringName = "HxWorkflow";
    }
}
