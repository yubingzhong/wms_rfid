using System;
using System.Collections.Generic;
using System.Text;
using THOK.Util;
using System.Data;
using System.Threading;
using THOK.WMS.DownloadWms.Dao;

namespace THOK.WMS.DownloadWms.Bll
{
    public class DownOutBillBll
    {
        private string Employee = "";

        #region 手动从营系统据下载数据

        /// <summary>
        /// 手动下载
        /// </summary>
        /// <param name="billno"></param>
        /// <returns></returns>
        public bool GetOutBillManual(string billno, string EmployeeCode)
        {
            bool tag = true;
            Employee = EmployeeCode;
            using (PersistentManager dbPm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                billno = "ORDER_ID IN(" + billno + ")";
                DataTable masterdt = this.GetOutBillMaster(billno);
                DataTable detaildt = this.GetOutBillDetail(billno);
                if (masterdt.Rows.Count > 0 && detaildt.Rows.Count > 0)
                {
                    DataSet detailds = this.OutBillDetail(detaildt);
                    //DataSet masterds = this.OutBillMaster(masterdt);
                    //this.Insert(masterds, detailds);
                }
                else
                    tag = false;
            }
            return tag;
        }
        #endregion

        #region 自动从营销系统下载数据

        /// <summary>
        /// 自动下载
        /// </summary>
        /// <returns></returns>
        public bool DownOutBillInfoAuto(string EmployeeCode)
        {
            bool tag = true;
            Employee = EmployeeCode;
            using (PersistentManager dbpm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                DataTable outBillNoTable = this.GetOutBillNo();
                string billnolist = UtinString.StringMake(outBillNoTable, "BILLNO");
                billnolist = UtinString.StringMake(billnolist);
                billnolist = "ORDER_ID NOT IN(" + billnolist + ")";
                DataTable masterdt = this.GetOutBillMaster(billnolist);
                DataTable detaildt = this.GetOutBillDetail(billnolist);
                if (masterdt.Rows.Count > 0 && detaildt.Rows.Count > 0)
                {
                    //DataSet detailds = this.OutBillDetail(detaildt);
                    //DataSet masterds = this.OutBillMaster(masterdt);
                    //this.Insert(masterds, detailds);
                }
                else
                    tag = false;
            }
            return tag;
        }
        #endregion

        #region 日期从营系统据下载数据

        /// <summary>
        /// 选择日期从营销系统下载出库单据
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public bool GetOutBill(string startDate, string endDate, string EmployeeCode, out string errorInfo)
        {
            bool tag = true;
            Employee = EmployeeCode;
            errorInfo = string.Empty;
            using (PersistentManager dbpm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                DataTable emply = dao.FindEmployee(EmployeeCode);   
                DataTable outBillNoTable = this.GetOutBillNo();
                string outBillList = UtinString.StringMake(outBillNoTable, "bill_no");
                outBillList = UtinString.StringMake(outBillList);
                outBillList = string.Format("ORDER_DATE >='{0}' AND ORDER_DATE <='{1}' AND ORDER_ID NOT IN({2})", startDate, endDate, outBillList);
                DataTable masterdt = this.GetOutBillMaster(outBillList);

                string outDetailList = UtinString.StringMake(masterdt, "ORDER_ID");
                outDetailList = UtinString.StringMake(outDetailList);
                outDetailList = "ORDER_ID IN(" + outDetailList + ")";
                DataTable detaildt = this.GetOutBillDetail(outDetailList);

                if (masterdt.Rows.Count > 0 && detaildt.Rows.Count > 0)
                {
                    DataSet masterds = this.OutBillMaster(masterdt, emply.Rows[0]["employee_id"].ToString());
                    DataSet detailds = this.OutBillDetail(detaildt);
                    this.Insert(masterds, detailds);
                }
                else
                {
                    errorInfo = "没有可下载的出库数据！";
                    tag = false;
                }
            }
            return tag;
        }

        #endregion

        #region 从分拣系统下载出库单

        public DataTable GetOrderGather(string orderDate, string batchNo)
        {
            DataTable ordergather = new DataTable();
            using (PersistentManager pm = new PersistentManager("ServerConnection"))
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.SetPersistentManager(pm);
                ordergather = dao.GetOrderGather(orderDate, batchNo);
            }
            return ordergather;
        }

        public DataTable GetBatchNo()
        {
            DataTable batchnodt = new DataTable();
            using (PersistentManager pm = new PersistentManager("ServerConnection"))
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.SetPersistentManager(pm);
                batchnodt = dao.GetBatchNo();
            }
            return batchnodt;
        }

        public bool GetOrderGather(string billno, string billdate, string batchno, string billtype, string warehouse, decimal quantity)
        {
            bool tag = true;
            string memo = "此主单由分拣系统日期为" + billdate + "，批次号为" + batchno + "合成！";
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.SetPersistentManager(pm);
                dao.InsertOutBillMaster(billno, billdate, batchno, billtype, warehouse, memo, quantity);
            }
            return tag;
        }

        public bool GetOrderGather(string billno, string billdate, string batchno)
        {
            bool tag = true;
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.SetPersistentManager(pm);
                DataTable detaildt = this.GetOrderGather(billdate, batchno);
                DataRow[] dr = detaildt.Select("ORDERDATE='" + billdate + "' and BATCHNO ='" + batchno + "'");
                DataSet detailds = this.GetOrderGather(dr, billno);
                if (detailds.Tables["WMS_OUT_BILLDETAIL"].Rows.Count > 0)
                {
                    dao.InsertOutBillDetail(detailds);
                }
            }
            return tag;
        }

        public DataSet GetOrderGather(DataRow[] dr, string billno)
        {
            string storeid = this.GetStoreBillDetailId();
            DataSet ds = this.GenerateEmptyTables();
            DataTable dt = new DataTable();
            foreach (DataRow row in dr)
            {
                DataRow detailrow = ds.Tables["WMS_OUT_BILLDETAIL"].NewRow();
                DataRow storerow = ds.Tables["DWV_IWMS_OUT_STORE_BILL_DETAIL"].NewRow();

                //2011.3.3 异形烟
                DataTable prodt = this.ProductRate(row["CIGARETTECODE"].ToString());
                decimal rate = Convert.ToDecimal(prodt.Rows[0]["TIAORATE"]) / Convert.ToDecimal(prodt.Rows[0]["JIANRATE"].ToString());//this.Quantity(row["CIGARETTECODE"].ToString());
                decimal quantity = Convert.ToDecimal(row["QUANTITY"].ToString());//
                quantity = quantity * rate;

                detailrow["BILLNO"] = billno;
                detailrow["PRODUCTCODE"] = row["CIGARETTECODE"];
                detailrow["PRICE"] = 0.00M;
                detailrow["UNITCODE"] = prodt.Rows[0]["JIANCODE"];//
                detailrow["MEMO"] = "";
                detailrow["QUANTITY"] = quantity;//Convert.ToDecimal(row["QUANTITY"].ToString()) / 50;//
                detailrow["OUTPUTQUANTITY"] = quantity;//Convert.ToDecimal(row["QUANTITY"].ToString()) / 50;//

                ds.Tables["WMS_OUT_BILLDETAIL"].Rows.Add(detailrow);


                storerow["STORE_BILL_DETAIL_ID"] = storeid + "SD";
                storerow["STORE_BILL_ID"] = billno.Substring(0, 12);
                storerow["BRAND_CODE"] = row["CIGARETTECODE"];
                storerow["BRAND_NAME"] = row["CIGARETTENAME"];
                storerow["IS_IMPORT"] = "0";
                storerow["QUANTITY"] = quantity;//Convert.ToDecimal(row["QUANTITY"].ToString()) / 50;//
                ds.Tables["DWV_IWMS_OUT_STORE_BILL_DETAIL"].Rows.Add(storerow);

                int i = Convert.ToInt32(storeid.Substring(8, 4));
                i++;
                string newcode = i.ToString();
                for (int j = 0; j < 4 - i.ToString().Length; j++)
                {
                    newcode = "0" + newcode;
                }
                storeid = DateTime.Now.ToString("yyyyMMdd") + newcode;
            }
            return ds;
        }


        /// <summary>
        /// 为从分拣系统下载单据生成编号
        /// </summary>
        /// <returns></returns>
        public string GetStoreBillDetailId()
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.SetPersistentManager(pm);
                DataTable dt = dao.GetOutStoreDetailId();
                if (dt.Rows.Count == 0)
                {
                    return DateTime.Now.ToString("yyyyMMdd") + "0001";
                }
                else
                {
                    int i = Convert.ToInt32(dt.Rows[0][0].ToString().Substring(8, 4));
                    i++;
                    string newcode = i.ToString();
                    for (int j = 0; j < 4 - i.ToString().Length; j++)
                    {
                        newcode = "0" + newcode;
                    }
                    return DateTime.Now.ToString("yyyyMMdd") + newcode;
                }
            }
        }

        #endregion

        #region 其他下载查询方法

        /// <summary>
        /// 查询营销系统出库主表数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public DataTable GetOutBillMaster(int pageIndex, int pageSize, string startDate, string endDate)
        {
            using (PersistentManager dbpm = new PersistentManager("YXConnection"))
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.SetPersistentManager(dbpm);
                DataTable outBillNoTable = this.GetOutBillNo();
                string billnolist = UtinString.StringMake(outBillNoTable, "BILLNO");
                billnolist = UtinString.StringMake(billnolist);
                billnolist = "ORDER_ID NOT IN(" + billnolist + ")";

                DataTable unitedt = this.UniteBillNo();
                string unitelist = UtinString.StringMake(unitedt, "DOWNBILLINO");
                unitelist = UtinString.StringMake(unitelist);
                return dao.GetOutBillMaster(billnolist, unitelist);
            }
        }

        /// <summary>
        /// 查询营销系统出库明细表数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="inBillNo"></param>
        /// <returns></returns>
        public DataTable GetOutBillDetail(int pageIndex, int pageSize, string inBillNo)
        {
            using (PersistentManager dbpm = new PersistentManager("YXConnection"))
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.SetPersistentManager(dbpm);
                return dao.GetOutBillDetailInfo(inBillNo);
            }
        }

        /// <summary>
        /// 查询合单的单据号
        /// </summary>
        /// <returns></returns>
        public DataTable UniteBillNo()
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                return dao.UniteBillNo();
            }
        }

        /// <summary>
        /// 查询数字仓储7天之内的单据号
        /// </summary>
        /// <returns></returns>
        public DataTable GetOutBillNo()
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                return dao.GetOutBillNo();
            }
        }

        /// <summary>
        /// 下载出库主表信息
        /// </summary>
        /// <returns></returns>
        public DataTable GetOutBillMaster(string billno)
        {
            using (PersistentManager dbpm = new PersistentManager("YXConnection"))
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.SetPersistentManager(dbpm);
                return dao.GetOutBillMaster(billno);
            }
        }

        /// <summary>
        /// 下载出库明细表信息
        /// </summary>
        /// <returns></returns>
        public DataTable GetOutBillDetail(string billno)
        {
            using (PersistentManager dbpm = new PersistentManager("YXConnection"))
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.SetPersistentManager(dbpm);
                return dao.GetOutBillDetail(billno);
            }
        }

        /// <summary>
        /// 保存主表信息到虚拟表
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public DataSet OutBillMaster(DataTable dt,string emplcode)
        {
            DataSet ds = this.GenerateEmptyTables();
            Guid eid = new Guid(emplcode);
            foreach (DataRow row in dt.Rows)
            {
                string createdate = row["ORDER_DATE"].ToString();
                createdate = createdate.Substring(0, 4) + "-" + createdate.Substring(4, 2) + "-" + createdate.Substring(6, 2);
                DataRow masterrow = ds.Tables["WMS_OUT_BILLMASTER"].NewRow();
                masterrow["bill_no"] = row["ORDER_ID"].ToString().Trim();
                masterrow["bill_date"] = Convert.ToDateTime(createdate);
                masterrow["bill_type_code"] = "2001";
                masterrow["warehouse_code"] = "0101";// row["DIST_CTR_CODE"].ToString().Trim();
                masterrow["operate_person_id"] = eid;
                masterrow["status"] = "1";
                masterrow["is_active"] = "1";
                masterrow["update_time"] = DateTime.Now;
                masterrow["origin"] = "1";
                ds.Tables["WMS_OUT_BILLMASTER"].Rows.Add(masterrow);

            }
            return ds;
        }

        /// <summary>
        /// 保存订单明细到虚拟表
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public DataSet OutBillDetail(DataTable outDetailTable)
        {
            DataSet ds = this.GenerateEmptyTables();           
            foreach (DataRow row in outDetailTable.Rows)
            {
                DataTable prodt = FindProductCodeInfo(row["BRAND_CODE"].ToString());               
                DataRow detailrow = ds.Tables["WMS_OUT_BILLDETAILA"].NewRow();
                detailrow["bill_no"] = row["ORDER_ID"].ToString().Trim();
                detailrow["product_code"] = prodt.Rows[0]["product_code"];
                detailrow["price"] = row["PRICE"] == "" ? 0 : row["PRICE"];
                detailrow["bill_quantity"] = Convert.ToDecimal(row["QUANTITY"]);
                detailrow["allot_quantity"] = 0;
                detailrow["unit_code"] = prodt.Rows[0]["unit_code"];
                detailrow["real_quantity"] = 0;
                ds.Tables["WMS_OUT_BILLDETAILA"].Rows.Add(detailrow);               
            }
            return ds;
        }

        public DataTable FindProductCodeInfo(string productCode)
        {
            using (PersistentManager dbPm = new PersistentManager())
            {
                DownProductDao dao = new DownProductDao();
                return dao.FindProductCodeInfo(productCode);
            }
        }

        /// <summary>
        /// 把下载的数据添加到数据库。
        /// </summary>
        /// <param name="masterds"></param>
        /// <param name="detailds"></param>
        public void Insert(DataSet masterds, DataSet detailds)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                try
                {
                    if (masterds.Tables["WMS_OUT_BILLMASTER"].Rows.Count > 0)
                    {
                        dao.InsertOutBillMaster(masterds);
                    }

                    if (detailds.Tables["WMS_OUT_BILLDETAILA"].Rows.Count > 0)
                    {
                        dao.InsertOutBillDetail(detailds);
                    }                   
                }
                catch (Exception exp)
                {
                    throw new Exception(exp.Message);
                }
            }
        }

        /// <summary>
        /// 获取产品单位比例
        /// </summary>
        /// <param name="productCode"></param>
        /// <returns></returns>
        public DataTable DownProductRate(string productCode)
        {
            using (PersistentManager dbPm = new PersistentManager())
            {
                DownProductDao dao = new DownProductDao();
                return dao.LcDownProductRate(productCode);
            }
        }

        /// <summary>
        /// 创建虚拟表
        /// </summary>
        /// <returns></returns>
        private DataSet GenerateEmptyTables()
        {
            DataSet ds = new DataSet();
            DataTable mastertable = ds.Tables.Add("WMS_OUT_BILLMASTER");
            mastertable.Columns.Add("bill_no");
            mastertable.Columns.Add("bill_date");
            mastertable.Columns.Add("bill_type_code");
            mastertable.Columns.Add("warehouse_code");
            mastertable.Columns.Add("status");
            mastertable.Columns.Add("verify_person_id");
            mastertable.Columns.Add("verify_date");
            mastertable.Columns.Add("description");
            mastertable.Columns.Add("is_active");
            mastertable.Columns.Add("update_time");
            mastertable.Columns.Add("operate_person_id");
            mastertable.Columns.Add("lock_tag");
            mastertable.Columns.Add("row_version");
            mastertable.Columns.Add("move_bill_master_bill_no");
            mastertable.Columns.Add("origin");
            mastertable.Columns.Add("target_cell_code");

            DataTable detailtable = ds.Tables.Add("WMS_OUT_BILLDETAILA");
            detailtable.Columns.Add("id");
            detailtable.Columns.Add("bill_no");
            detailtable.Columns.Add("product_code");
            detailtable.Columns.Add("unit_code");
            detailtable.Columns.Add("price");
            detailtable.Columns.Add("bill_quantity");
            detailtable.Columns.Add("allot_quantity");
            detailtable.Columns.Add("real_quantity");
            detailtable.Columns.Add("description");


            DataTable storemaster = ds.Tables.Add("DWV_IWMS_OUT_STORE_BILL");
            storemaster.Columns.Add("STORE_BILL_ID");
            storemaster.Columns.Add("DIST_CTR_CODE");
            storemaster.Columns.Add("AREA_TYPE");
            storemaster.Columns.Add("QUANTITY_SUM");
            storemaster.Columns.Add("AMOUNT_SUM");
            storemaster.Columns.Add("DETAIL_NUM");
            storemaster.Columns.Add("CREATOR_CODE");
            storemaster.Columns.Add("CREATE_DATE");
            storemaster.Columns.Add("BILL_TYPE");
            storemaster.Columns.Add("BILL_STATUS");
            storemaster.Columns.Add("IN_OUT_TYPE");
            storemaster.Columns.Add("IS_IMPORT");
            storemaster.Columns.Add("DISUSE_STATUS");


            DataTable storedetail = ds.Tables.Add("DWV_IWMS_OUT_STORE_BILL_DETAIL");
            storedetail.Columns.Add("STORE_BILL_DETAIL_ID");
            storedetail.Columns.Add("STORE_BILL_ID");
            storedetail.Columns.Add("BRAND_CODE");
            storedetail.Columns.Add("BRAND_NAME");
            storedetail.Columns.Add("QUANTITY");
            storedetail.Columns.Add("IS_IMPORT");
            storedetail.Columns.Add("BILL_TYPE");

            return ds;
        }

        #endregion

        #region 出库单合单

        #region 数字仓储合单

        /// <summary>
        /// 合单主表和细表数据到出库表
        /// </summary>
        /// <param name="billno"></param>
        public void GetByOutBill(string billno, string date, string Employee, string Condition)
        {
            try
            {
                //添加主表
                string datetime = Convert.ToDateTime(date).ToString("yyyyMMdd");
                DataTable masterTable = this.QueryNullTable("WMS_OUT_BILLMASTER");
                DataRow newRow = masterTable.NewRow();
                newRow["BILLNO"] = billno;
                newRow["BILLDATE"] = date;
                newRow["BILLTYPE"] = "1003";
                newRow["WH_CODE"] = "0101";
                newRow["OPERATEPERSON"] = Employee;
                newRow["STATUS"] = "1";
                newRow["VALIDATEPERSON"] = "";
                newRow["VALIDATEDATE"] = date;
                //newRow["MEMO"] = "此单是由营销系统单据为: " + Condition + " 合成！";
                newRow["MEMO"] = "此单是由营销系统其他单据合成！";
                newRow["ISCLOSEMONAD"] = "2";
                masterTable.Rows.Add(newRow);
                this.InsertOutBillMaster("WMS_OUT_BILLMASTER", masterTable);

                //合单的明细
                // DataTable detaildt = this.GetDetailByBillNo(Condition);
                DataTable detaildt = this.GetOutDetailClose(date, Condition);
                //添加细表
                DataTable detailTable = this.QueryNullTable("WMS_OUT_BILLDETAIL");
                foreach (DataRow row in detaildt.Rows)
                {
                    string id = DateTime.Now.ToString("yyMMddHHmmssfff");
                    id = id.Substring(1, 14);
                    DataRow detailRow = detailTable.NewRow();
                    detailRow["ID"] = id;
                    detailRow["BILLNO"] = billno;
                    detailRow["BILLTYPE"] = "1003";
                    detailRow["PRODUCTCODE"] = row["PRODUCTCODE"];
                    detailRow["PRICE"] = row["PRICE"];
                    detailRow["QUANTITY"] = row["QUANTITY"];
                    detailRow["OUTPUTQUANTITY"] = row["OUTPUTQUANTITY"];
                    detailRow["UNITCODE"] = row["UNITCODE"];
                    detailRow["MEMO"] = "";
                    detailTable.Rows.Add(detailRow);
                    Thread.Sleep(1);
                }
                this.InsertOutBillMaster("WMS_OUT_BILLDETAIL", detailTable);
                this.UpdateOutBillClose(date, Condition, "1", "0");
            }
            catch (Exception e)
            {
                this.DeleteOutBillInfo(billno);
                this.UpdateOutBillClose(date, Condition, "0", "1");
                throw new Exception(e.Message);
            }
        }


        /// <summary>
        /// 根据单据号查询合单明细数据
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public DataTable GetDetailByBillNo(string billNoList)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                return dao.GetOutDetailByBillNo(billNoList);
            }
        }

        /// <summary>
        /// 根据日期和状态查询合单数据
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public DataTable GetOutDetailClose(string date,string billType)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                return dao.GetOutDetailClose(date,billType);
            }
        }


        /// <summary>
        /// 合单添加数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="table"></param>
        public void InsertOutBillMaster(string tableName, DataTable table)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.InsertTable(tableName, table);
            }
        }

        /// <summary>
        /// 查询一个空表
        /// </summary>
        /// <param name="outTable"></param>
        /// <returns></returns>
        public DataTable QueryNullTable(string outTable)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                return dao.QueryOutMaterTable(outTable);
            }
        }

        /// <summary>
        /// 清除一个星期以前合单后没有作业的数据
        /// </summary>
        public void DeleteOutBillInfo()
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.DeleteOutBillInfo();
            }
        }

        /// <summary>
        /// 根据单据删除主表和细表数据
        /// </summary>
        /// <param name="billno"></param>
        public void DeleteOutBillInfo(string billno)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.DeleteOutBillInfo(billno);
            }
        }

        /// <summary>
        /// 修改是否合单的状态，参数（时间，出库类型，是否合单，是否合单）
        /// </summary>
        /// <param name="billno"></param>
        public void UpdateOutBillClose(string date, string billType, string isClose, string Close)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                dao.UpdateOutBillClose(date,billType,isClose,Close);
            }
        }

        #endregion

        #region 中烟上报合单

        /// <summary>
        /// 合单插入上报中烟表
        /// </summary>
        /// <param name="date"></param>
        /// <param name="billno"></param>
        /// <param name="Employee"></param>
        public void zyGetOutTable(string billno, string date, string Employee,string billNolist)
        {
            //添加中烟主表
            string datetime = Convert.ToDateTime(date).ToString("yyyyMMdd");
            DataTable zymasterTable = this.QueryNullTable("DWV_IWMS_OUT_STORE_BILL");
            DataTable quantityTable = this.GetCountQuantity(billNolist);
            DataRow storerow = zymasterTable.NewRow();
            storerow["STORE_BILL_ID"] = billno;
            storerow["DIST_CTR_CODE"] = quantityTable.Rows[0]["DIST_CTR_CODE"].ToString().Trim();
            storerow["QUANTITY_SUM"] = Convert.ToDecimal(quantityTable.Rows[0]["QUANTITY"]);
            storerow["AMOUNT_SUM"] = Convert.ToDecimal(quantityTable.Rows[0]["AMOUNT_SUM"]);
            storerow["DETAIL_NUM"] = Convert.ToDecimal(this.GetDetailByBillNo(billNolist).Rows.Count);
            storerow["CREATE_DATE"] = datetime;
            storerow["CREATOR_CODE"] = Employee;
            storerow["BILL_TYPE"] = "1003";
            storerow["AREA_TYPE"] = "0901";
            storerow["IN_OUT_TYPE"] = "1203";
            storerow["DISUSE_STATUS"] = "0";
            storerow["BILL_STATUS"] = "0";
            storerow["IS_IMPORT"] = "0";
            zymasterTable.Rows.Add(storerow);
            this.InsertOutBillMaster("DWV_IWMS_OUT_STORE_BILL", zymasterTable);

            //查询合单后的明细数据
            DataTable detaildt = this.QueryOutDetailTable(billno);
            //添加中烟细表
            DataTable zydetailTable = this.QueryNullTable("DWV_IWMS_OUT_STORE_BILL_DETAIL");
            foreach (DataRow row in detaildt.Rows)
            {
                DataTable protable = this.ProductRate(row["PRODUCTCODE"].ToString());
                DataRow zydetailRow = zydetailTable.NewRow();
                decimal quantity = Convert.ToDecimal(Convert.ToDecimal(row["QUANTITY"]) * Convert.ToDecimal(protable.Rows[0]["JIANRATE"].ToString()));
                quantity = quantity / Convert.ToDecimal(protable.Rows[0]["TIAORATE"].ToString());
                zydetailRow["STORE_BILL_DETAIL_ID"] = row["ID"].ToString().Trim();
                zydetailRow["STORE_BILL_ID"] = billno;
                zydetailRow["BRAND_CODE"] = row["PRODUCTCODE"].ToString().Trim();
                zydetailRow["BRAND_NAME"] = protable.Rows[0]["PRODUCTNAME"].ToString().Trim();
                zydetailRow["QUANTITY"] = quantity;
                zydetailRow["IS_IMPORT"] = "0";
                zydetailRow["BILL_TYPE"] = "1003";
                zydetailTable.Rows.Add(zydetailRow);
            }
            this.InsertOutBillMaster("DWV_IWMS_OUT_STORE_BILL_DETAIL", zydetailTable);
        }

        /// <summary>
        /// 根据单据号查询总数量和总金额
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public DataTable GetCountQuantity(string billNolist)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                return dao.GetCountQuantity(billNolist);
            }
        }

        /// <summary>
        /// 根据时间查询总数量和总金额
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public DataTable GetDateCountQuantity(string date)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                return dao.GetDateCountQuantity(date);
            }
        }

        /// <summary>
        /// 计量单位转换
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public DataTable ProductRate(string product)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownProductDao dao = new DownProductDao();
                return dao.LcDownProductRate(product);
            }
        }

        /// <summary>
        /// 根据单号查询合单后的明细数据
        /// </summary>
        /// <param name="billno"></param>
        /// <returns></returns>
        public DataTable QueryOutDetailTable(string billno)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                DownOutBillDao dao = new DownOutBillDao();
                return dao.QueryOutDetailTable(billno);
            }
        }

        #endregion

        #endregion
    }
}
