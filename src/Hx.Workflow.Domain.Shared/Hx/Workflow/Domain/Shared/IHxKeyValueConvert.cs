using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Domain.Shared
{
    public interface IHxKeyValueConvert
    {
        string Key { get; }
        string Value { get; }
    }
}