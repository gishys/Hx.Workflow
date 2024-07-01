using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkNodeTreeDto
    {
        public string Title { get; set; }
        public Guid Key { get; set; }
        public bool Selected { get; set; }
        public string Name {  get; set; }
        public string Receiver {  get; set; }
    }
}
