using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkPoint : Entity<Guid>
    {
        public Guid WkNodeId { get; protected set; }
        public int Left { get; protected set; }
        public int Right { get; protected set; }
        public int Top { get; protected set; }
        public int Bottom { get; protected set; }
        public WkPoint()
        { }
        public WkPoint(
            int left,
            int right,
            int top,
            int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }
}
