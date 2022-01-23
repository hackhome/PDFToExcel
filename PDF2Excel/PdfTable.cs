using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF2Excel
{
    public class PdfTable
    {
        public string TableName { get; set; }
        public List<PdfRow> Rows { get; set; } = new List<PdfRow>();
        /// <summary>
        /// 
        /// </summary>
        public string Header { get; set; }

        public string Bottom { get; set; }

        public bool IsText { get; set; }

        /// <summary>
        /// 根据索引获取
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="colIndex"></param>
        /// <returns></returns>
        public PdfCell this[int rowIndex, int colIndex]
        {
            get
            {
                foreach (var row in Rows)
                {
                    foreach (var cell in row.Cells)
                    {
                        if (rowIndex >= row.RowIndex && rowIndex <= row.RowIndex + cell.RowSpan
                   && colIndex >= cell.ColIndex && colIndex <= cell.ColIndex + cell.ColSpan)
                        {
                            return cell;
                        }
                    }
                }
                return null;
            }
     
        }

        public Rectangle GetTableSize()
        {
            if (Rows.Count == 0 || Rows[0].Cells.Count == 0) return new Rectangle(0, 0, 0, 0);
            float width = 0;
            float height = 0;
            foreach (var cell in Rows[0].Cells)
            {
                if(!cell.IsHMerge)
                    width += cell.Rectangle.GetWidth();
            }
            foreach (var row  in Rows)
            {
                if (!row[0].IsVMerge)
                    height += row[0].Rectangle.GetHeight();
            }
            return new Rectangle(Rows[0].Cells[0].Rectangle.GetX(), Rows[Rows.Count - 1].Cells[0].Rectangle.GetY(), width, height);

        }

        public bool MergeTable(PdfTable pdfTable)
        {
            if (pdfTable == null) return false;
            if (Rows.Count == 0 || pdfTable.Rows.Count == 0) return false;
            if (Rows[0].Cells.Count != pdfTable.Rows[0].Cells.Count) return false;
            bool isTtitle = true;
            bool firstIsID = false;
            bool firstRowMege = false;// 第二个表格的第一行是不是第一个表格的换行 （相当于同一行）
            for (int i = 0; i < Rows[0].Cells.Count; i++)
            {
                if (Rows[0].Cells[i].Text?.Trim() != pdfTable.Rows[0].Cells[i].Text?.Trim())
                {
                    isTtitle = false;
                    break;
                }
            }
            if (Rows.Count > 2)
            {
                int id1 = -1;
                int id2 = -1;
                if (Rows[Rows.Count - 2].Cells[0].Text != null && Rows[Rows.Count - 1].Cells[0].Text != null)
                {
                    int.TryParse(Rows[Rows.Count - 2].Cells[0].Text.Trim(), out id1);
                    int.TryParse(Rows[Rows.Count - 1].Cells[0].Text.Trim(), out id2);
                }
                firstIsID = id1 + 1 == id2;
            }
            if (firstIsID )
            {
                int firstDataIndex = 0;
                if (isTtitle) firstDataIndex = 1;
                if (string.IsNullOrEmpty(pdfTable.Rows[firstDataIndex].Cells[0].Text))
                {
                    firstRowMege = true;
                }
                    
            }
            
          
            
         
          
            for (int i = 0; i < pdfTable.Rows.Count; i++)
            {
                var row = pdfTable.Rows[i];
                if (isTtitle)
                {
                    isTtitle = false;
                }
                else
                {
                    if (firstRowMege )
                    {
                        firstRowMege = false;
                        var lastRow = Rows[Rows.Count - 1];
                        for (int j = 0; j < lastRow.Cells.Count; j++)
                        {
                            lastRow.Cells[j].Text += row.Cells[j].Text;
                        }
                    }
                    else
                    {
                        Rows.Add(row);
                    }
                  
                }
            }
        
            return true;

        }


    }

    public class PdfRow
    {

        public int RowIndex { get; set; }
        public List<PdfCell> Cells { get; private set; } 

        public int _count = 0;
        public int Count { get { return _count; } }

        public PdfRow()
        {
            Cells = new List<PdfCell>();
        }

        public PdfCell this[int colIndex]
        {
            get
            {
                return Cells[colIndex];
            }

        }

        public void AddCell(PdfCell pdfCell)
        {
            if (Cells == null) Cells = new List<PdfCell>();
            //if(LastCell()!=null)
            //{
            //    pdfCell.ColIndex = LastCell().ColIndex + LastCell().ColSpan + 1;
            //}
            Cells.Add(pdfCell);
        }

        //public PdfCell LastCell()
        //{
        //    if (Cells.Count == 0) return null;
        //   return Cells[Cells.Count - 1];
        //}
        /// <summary>
        /// 加入点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="isSpan"></param>
        public void AddHPoint(float x, float y,bool isSpan)
        {
            if (Cells.Count == 0)
            {
                PdfCell pdfCell = new PdfCell();
                pdfCell.Rectangle = new Rectangle(x, y, 0, 0);
                AddCell(pdfCell);
            }
            else
            {
                if (isSpan)//合并
                {
                    Cells[Cells.Count - 1].ColSpan++;
                }
                else
                {
                    Cells[Cells.Count - 1].Rectangle.SetWidth(x - Cells[Cells.Count - 1].Rectangle.GetX());
                }
                
            }
        }


     
    }



    

    public class PdfCell
    { 
    
        public string Text { get; set; }

        public int ColIndex { get; set; }
        /// <summary>
        /// 纵向合并的行
        /// </summary>
        public int RowSpan { get; set; }
        /// <summary>
        /// 横向合并的列
        /// </summary>
        public int ColSpan { get; set; }
        /// <summary>
        /// 是否横向合并
        /// </summary>
        public bool IsHMerge { get; set; }

        /// <summary>
        /// 是否竖直合并
        /// </summary>
        public bool IsVMerge { get; set; }

        public Rectangle Rectangle { get; set; }

        /// <summary>
        /// 加入点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="isSpan"></param>
        public void AddVPoint(float x, float y)
        {
            Rectangle.SetHeight(y - Rectangle.GetY());
        }

    }
}
