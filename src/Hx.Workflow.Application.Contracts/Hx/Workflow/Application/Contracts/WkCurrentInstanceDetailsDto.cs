﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkCurrentInstanceDetailsDto
    {
        public Guid Id { get; set; }
        public string BusinessNumber { get; set; }
        public string RegistrationCategory { get; set; }
        public string Receiver { get; set; }
        public string ReceiveTime { get; set; }
        public string BusinessCommitmentDeadline { get; set; }
        public WkExecutionPointerDto CurrentExecutionPointer { get; set; }
    }
}