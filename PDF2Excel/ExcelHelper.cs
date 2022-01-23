using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Data;
using NPOI.SS.Util;

namespace PDF2Excel
{
    public class ExcelHelper 
    {


        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static void PdfTableToExcel(List<PdfTable> tables, string fileName, string sheetName = "sheet1")
        {

            int startIndex=0;
            int count = 0;
            IWorkbook workbook = null;
            ISheet sheet = null;

            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook();
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook();
                if (workbook != null)
                {
                    sheet = workbook.CreateSheet(sheetName);
                }
                else
                {
                    return ;
                }
                ICellStyle notesStyle = workbook.CreateCellStyle();
                notesStyle.WrapText = true;//设置换行这个要先设置
                foreach (var table in tables)
                {
                    if (!string.IsNullOrEmpty(table.Header)) count++;
                    count += table.Rows.Count;
                }

                foreach (var table in tables)
                {
                    float pageWitdh = table.GetTableSize().GetWidth();
                    bool isHeader = !string.IsNullOrEmpty(table.Header);
                    if (isHeader)
                    {
                        IRow row = sheet.CreateRow(startIndex);
                        row.CreateCell(0).SetCellValue(table.Header);
                        ///标题头部合并
                        if (table.Rows[0].Cells.Count > 0)
                        {
                            CellRangeAddress region = new CellRangeAddress(startIndex, startIndex, 0, table.Rows[0].Cells.Count-1);
                            sheet.AddMergedRegion(region);
                        }
                        startIndex++;
                    }
                    for (int i = 0; i < table.Rows.Count; ++i)
                    {
                        IRow row = sheet.CreateRow(startIndex);
                        startIndex++;
                        for (int j = 0; j < table.Rows[i].Cells.Count; ++j)
                        {
                           
                            var cell = table.Rows[i].Cells[j];
                            if (i == 0)
                            {
                                sheet.SetColumnWidth(j , (int)(cell.Rectangle.GetWidth()*120* 256/pageWitdh));
                            }
                            if (!cell.IsHMerge && !cell.IsVMerge)
                            {
                                ICell excelCell = row.CreateCell(j);
                                string text = cell.Text;
                                if (!table.IsText)
                                {
                                    text = text.Replace("\r", "").Replace("\n", "");
                                }
                                excelCell.SetCellValue(text);
                                excelCell.CellStyle = notesStyle;
                            }

                        }
                    }
                    int start = startIndex - table.Rows.Count+1;
                    if (isHeader) start--;
                   
                    //合并单元格
                    for (int i = 0; i < table.Rows.Count; ++i)
                    {
                      
                        for (int j = 0; j < table.Rows[i].Cells.Count; ++j)
                        {
                            var cell = table.Rows[i].Cells[j];
                            if (!cell.IsHMerge && cell.ColSpan > 0)
                            {
                                CellRangeAddress region = new CellRangeAddress(start + i, start + i, j, j + cell.ColSpan);
                                sheet.AddMergedRegion(region);
                            }
                            if (!cell.IsVMerge && cell.RowSpan > 0)
                            {
                                CellRangeAddress region = new CellRangeAddress(start + i, start + i + cell.RowSpan, j, j);
                                sheet.AddMergedRegion(region);
                            }
                        }
                    }

                }
              
              

                workbook.Write(fs); //写入到excel 
           
                fs.Close();
            }

        }


    }
}
