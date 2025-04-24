using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Domain
{
    public class WkDbProperties
    {
        public static string DbTablePrefix { get; set; } = "HX";
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public static string DbSchema { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        public const string ConnectionStringName = "HxWorkflow";
    }
}
