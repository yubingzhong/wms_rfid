﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using THOK.Wms.Bll.Interfaces;

namespace Authority.Controllers.Wms.ComplexSearch
{
    public class StockCheckSearchController : Controller
    {
        [Dependency]
        public IStockCheckSearchService StockCheckSearchService { get; set; }
        [Dependency]
        public ICheckSearchDetailService CheckSearchDetailService { get; set; }
        //
        // GET: /StockCheckSearch/

        public ActionResult Index(string moduleID)
        {
            ViewBag.hasSearch = true;
            ViewBag.hasHelp = true;
            ViewBag.hasPrint = true;
            ViewBag.ModuleID = moduleID;
            return View();
        }

        //
        // GET: /StockCheckSearch/Details/

        public ActionResult Details(int page, int rows, FormCollection collection)
        {
            string BillNo = collection["BillNo"] ?? "";
            string WarehouseCode = collection["WarehouseCode"] ?? "";
            string BeginDate = collection["BeginDate"] ?? "";
            string EndDate = collection["EndDate"] ?? "";
            string OperatePersonCode = collection["OperatePerson"] ?? "";
            string CheckPersonCode = collection["CheckPerson"] ?? "";
            string Operate_Status = collection["Operate_Status"] ?? "";
            var checkBillMaster = StockCheckSearchService.GetDetails(page, rows, BillNo, WarehouseCode, BeginDate, EndDate, OperatePersonCode, CheckPersonCode, Operate_Status);
            return Json(checkBillMaster, "text", JsonRequestBehavior.AllowGet);
        }
        //
        // GET: /StockCheckSearch/InfoDetails/

        public ActionResult InfoDetails(int page, int rows, string BillNo)
        {
            var checkBillDetail = CheckSearchDetailService.GetDetails(page, rows, BillNo);
            return Json(checkBillDetail, "text", JsonRequestBehavior.AllowGet);
        }
    }
}
