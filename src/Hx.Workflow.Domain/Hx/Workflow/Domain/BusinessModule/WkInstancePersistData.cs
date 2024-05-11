using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Users;

namespace Hx.Workflow.Domain.BusinessModule
{
    public class WkInstancePersistData
    {
        public WkInstancePersistData(string businessNumber, string userName, Guid userId)
        {
            BusinessNumber = businessNumber;
            UserName = userName;
            UserId = userId;
        }
        public string BusinessNumber { get; set; }
        public string UserName { get; set; }
        public Guid UserId { get; set; }
    }
}