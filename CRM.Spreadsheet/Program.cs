using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using log4net;
using System.Reflection;
using Autofac;
using CRM.Model;
using CRM.Services;
using System.Configuration;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using Newtonsoft.Json;

namespace CRM.Spreadsheet
{
    class Program
    {


        static void Main(string[] args)
        {
            ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            var builder = new ContainerBuilder();
            builder.RegisterType<OpportunityService>().As<IOpportunityService>().InstancePerLifetimeScope();
      

            using (var container = builder.Build())
            {
                var opportunityService = container.Resolve<IOpportunityService>();

                DataTable table = opportunityService.OpportunityReport();

                string opportunityReportPath = ConfigurationManager.AppSettings["OpportunityReportPath"];
                string OpportunityReportName = ConfigurationManager.AppSettings["OpportunityReportName"];
                string fileName = $"{opportunityReportPath}{OpportunityReportName}.xlsx";
                CreateOpportunityReport(table, opportunityReportPath, fileName);


                table = opportunityService.OpportunityActivityReport();
                opportunityReportPath = ConfigurationManager.AppSettings["OpportunityActivityReportPath"];
                OpportunityReportName = ConfigurationManager.AppSettings["OpportunityActivityReportName"];
                fileName = $"{opportunityReportPath}{OpportunityReportName}.xlsx";
                CreateOpportunityReport(table, opportunityReportPath, fileName);

                table = opportunityService.OpportunityStageProgress();
                opportunityReportPath = ConfigurationManager.AppSettings["OpportunityStageProgressPath"];
                OpportunityReportName = ConfigurationManager.AppSettings["OpportunityStageProgressName"];
                fileName = $"{opportunityReportPath}{OpportunityReportName}.xlsx";
                CreateOpportunityReport(table, opportunityReportPath, fileName);

            }
        }

        private static void CreateOpportunityReport(DataTable table, string opportunityReportPath, string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                IWorkbook workbook = AppendWorkbook(table);

                workbook.Write(fs);
            }

            DataView view = new DataView(table);
            DataTable empTable = view.ToTable(true, "EmpName");

            foreach (DataColumn col in empTable.Columns)
            {
                foreach (DataRow row in empTable.Rows)
                {
                    fileName = $"{opportunityReportPath}{row["EmpName"].ToString()}.xlsx";
                    string EmpName = row["EmpName"].ToString();
                    using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        IWorkbook workbook = AppendWorkbookByEmpNo(table, EmpName);

                        workbook.Write(fs);
                    }
                }
            }

        }

        private static IWorkbook AppendWorkbookByEmpNo(DataTable table, string empName)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet excelSheet = workbook.CreateSheet("Sheet1");

            List<String> columns = new List<string>();
            IRow row = excelSheet.CreateRow(0);
            int columnIndex = 0;

            foreach (DataColumn column in table.Columns)
            {
                columns.Add(column.ColumnName);
                row.CreateCell(columnIndex).SetCellValue(column.ColumnName);
                columnIndex++;
            }

            int rowIndex = 1;

            foreach (DataRow dsrow in table.Select($"EmpName = '{empName}'"))
            {
                row = excelSheet.CreateRow(rowIndex);
                int cellIndex = 0;
                foreach (String col in columns)
                {
                    row.CreateCell(cellIndex).SetCellValue(dsrow[col].ToString());
                    cellIndex++;
                }

                rowIndex++;
            }

            return workbook;
        }

        private static IWorkbook AppendWorkbook(DataTable table)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet excelSheet = workbook.CreateSheet("Sheet1");

            List<String> columns = new List<string>();
            IRow row = excelSheet.CreateRow(0);
            int columnIndex = 0;

            foreach (DataColumn column in table.Columns)
            {
                columns.Add(column.ColumnName);
                row.CreateCell(columnIndex).SetCellValue(column.ColumnName);
                columnIndex++;
            }

            int rowIndex = 1;
            foreach (DataRow dsrow in table.Rows)
            {
                row = excelSheet.CreateRow(rowIndex);
                int cellIndex = 0;
                foreach (String col in columns)
                {
                    row.CreateCell(cellIndex).SetCellValue(dsrow[col].ToString());
                    cellIndex++;
                }

                rowIndex++;
            }

            return workbook;
        }
    }
}
