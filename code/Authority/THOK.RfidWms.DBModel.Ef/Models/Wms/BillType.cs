﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THOK.RfidWms.DBModel.Ef.Models.Wms
{
    public class BillType
    {
        public BillType()
        {
        }
        public string BillType { get; set; }
        public string BillTypeName { get; set; }
        public string BillClass { get; set; }
        public string Description { get; set; }
        public string IsActive { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
