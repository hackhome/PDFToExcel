using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF2Excel
{
    public class PdfTableHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hBorders"> 横的边框线</param>
        /// <param name="vBorders">竖的边框线</param>
        /// <returns></returns>
        public static List<PdfTable> ConvertTables(List<TableBorder> hBorders , List<TableBorder> vBorders)
        {
            List<PdfTable> pdfTables = new List<PdfTable>();
            List<TableBorderCollection> hvTableBorders = new List<TableBorderCollection>();
            bool isbreak;
            ///分表格 一个表格内任意一条线条都必须于其他线条有一个交叉点
            foreach (var hBorder in hBorders)
            {
                TableBorderCollection hVTableBorder= hvTableBorders.Where(p => p.VBorders.Any(t => t.Overlaps(hBorder))).FirstOrDefault();
                if (hVTableBorder == null)
                {
                   var vIntersects = vBorders.Where(p => p.Overlaps(hBorder)).ToList();
                    TableBorderCollection hVTableBorder1 = new TableBorderCollection() { HBorders = new List<TableBorder>() { hBorder }, VBorders = vIntersects };
                    hvTableBorders.Add(hVTableBorder1);
                }
                else
                {
                    hVTableBorder.HBorders.Add(hBorder);
                }
            }
            foreach (var vBorder in vBorders)
            {
                TableBorderCollection hVTableBorder = hvTableBorders.Where(p => p.HBorders.Any(t => t.Overlaps(vBorder))).FirstOrDefault();
                if (hVTableBorder == null)
                {
                    var hIntersects = hBorders.Where(p => p.Overlaps(vBorder)).ToList();
                    TableBorderCollection hVTableBorder1 = new TableBorderCollection() { HBorders = hIntersects, VBorders = new List<TableBorder>() { vBorder } };
                    hvTableBorders.Add(hVTableBorder1);
                }
                else
                {
                    hVTableBorder.VBorders.Add(vBorder);
                }
            }
            //移除线条小于4的
            for (int i = hvTableBorders.Count-1; i >=0; i--)
            {
                if (hvTableBorders[i].HBorders == null || hvTableBorders[i].VBorders == null)
                {
                    hvTableBorders.RemoveAt(i);
                }
                hvTableBorders[i].HBorders= hvTableBorders[i].HBorders.GroupBy(p => p).Select(p => p.Key).ToList();
                hvTableBorders[i].VBorders= hvTableBorders[i].VBorders.GroupBy(p => p).Select(p => p.Key).ToList();
                if (hvTableBorders[i].HBorders.Count + hvTableBorders[i].VBorders.Count < 4)
                {
                    hvTableBorders.RemoveAt(i);
                }
            }
            //重新排序
            foreach (var item in hvTableBorders)
            {
                item.VBorders.Sort(delegate (TableBorder border1, TableBorder border2)
                {
                    if (border1 == null && border2 == null) return 0;
                    else if (border1 == null) return -1;
                    else if (border2 == null) return 1;
                    else
                    {
                        int comparerInt = border1.Border.GetX().CompareTo(border2.Border.GetX());
                        if (comparerInt == 0)
                        {
                            comparerInt = border1.Border.GetY().CompareTo(border2.Border.GetY());
                        }
                        return comparerInt;
                    }
                });
                item.HBorders.Sort(delegate (TableBorder border1, TableBorder border2)
                {
                    if (border1 == null && border2 == null) return 0;
                    else if (border1 == null) return -1;
                    else if (border2 == null) return 1;
                    else
                    {
                        int comparerInt = border2.Border.GetY().CompareTo(border1.Border.GetY());
                        if (comparerInt == 0)
                        {
                            comparerInt = border2.Border.GetX().CompareTo(border1.Border.GetX());
                        }
                        return comparerInt;
                    }
                });

                pdfTables.Add(ConvertTable(item));
            }

            return pdfTables;
        }

        public static PdfTable ConvertTable(TableBorderCollection  tableBorderCollection)
        {
            PdfTable pdfTable = new PdfTable();
            List<List<TableBorder>> HBorders = new List<List<TableBorder>>();
            List<List<TableBorder>> VBorders = new List<List<TableBorder>>();

            List<TableBorder> lastBorders = null;
            for (int i = 0; i < tableBorderCollection.HBorders.Count; i++)
            {
                var hborder = tableBorderCollection.HBorders[i];
                if (i == 0 || !hborder.IsEqHBorder(lastBorders[0]) )
                {
                    lastBorders = new List<TableBorder>() { hborder };
                    HBorders.Add(lastBorders);
                }
                else
                {
                    lastBorders.Add(hborder);
                }
            }
            for (int j = 0; j < tableBorderCollection.VBorders.Count; j++)
            {
                var vborder = tableBorderCollection.VBorders[j];
                if (j == 0 || !vborder.IsEqVBorder(lastBorders[0]))
                {
                    lastBorders = new List<TableBorder>() { vborder };
                    VBorders.Add(lastBorders);
                }
                else
                {
                    lastBorders.Add(vborder);
                }
            }
            Rectangle[,] points = new Rectangle[HBorders.Count, VBorders.Count];
            for (int i = 0; i < HBorders.Count; i++)
            {
                for (int j = 0; j < VBorders.Count; j++)
                {
                    points[i, j] = new Rectangle(VBorders[j][0].Border.GetX()-0.25f, HBorders[i][0].Border.GetY()-0.25f, VBorders[j][0].Border.GetWidth()+0.25f, HBorders[i][0].Border.GetHeight()+0.25f);
                }
            }
            Rectangle[,] cells= new Rectangle[HBorders.Count, VBorders.Count];
            for (int i = 0; i < HBorders.Count-1; i++)
            {
                PdfRow pdfRow = new PdfRow();
                pdfTable.Rows.Add(pdfRow);
                for (int j = 0; j < VBorders.Count-1; j++)
                {
                    bool HIntersect = HBorders[i].Any(p => p.Border.Overlaps(points[i, j]) && p.Border.Overlaps(points[i , j + 1]));
                    bool VIntersect = VBorders[j].Any(p => p.Border.Overlaps(points[i, j]) && p.Border.Overlaps(points[i + 1, j ]));
                    PdfCell pdfCell = new PdfCell();
                    pdfCell.ColIndex = j;
                    pdfCell.ColSpan = 0;
                    pdfCell.Rectangle = new Rectangle(points[i, j].GetX(), points[i+1, j].GetY()
                        , points[i , j + 1].GetX() - points[i, j].GetX(), points[i , j ].GetY() - points[i + 1, j ].GetY());
                    pdfRow.AddCell(pdfCell);
                    if (HIntersect && VIntersect)
                    {

                    }
                    else if (!HIntersect)
                    {
                        pdfCell.IsVMerge = true;
                    }
                    else if (!VIntersect)
                    {
                        pdfCell.IsHMerge = true;
                    }
                    else
                    {
                        pdfCell.IsVMerge = true;
                        pdfCell.IsHMerge = true;
                    }
                }
            }
            for (int i = pdfTable.Rows.Count-1; i >=0; i--)
            {
                for (int j = pdfTable.Rows[i].Cells.Count-1; j >=0; j--)
                {
                    var cell = pdfTable.Rows[i].Cells[j];
                    if (cell.IsHMerge || cell.IsVMerge)
                    {
                        
                       
                        if (cell.IsHMerge)
                        {
                            var PrevHCell = pdfTable.Rows[i].Cells[j - 1];
                            PrevHCell.Rectangle.SetWidth(PrevHCell.Rectangle.GetWidth() + cell.Rectangle.GetWidth());
                            PrevHCell.ColSpan += cell.ColSpan + 1;
                        }
                        if (cell.IsVMerge )
                        {
                            if (i > 0)
                            {
                                var PrevVCell = pdfTable.Rows[i - 1].Cells[j];
                                PrevVCell.Rectangle.SetY(cell.Rectangle.GetY());
                                PrevVCell.Rectangle.SetHeight(PrevVCell.Rectangle.GetHeight() + cell.Rectangle.GetHeight());
                                PrevVCell.RowSpan += cell.RowSpan + 1;
                            }
                            
                        }
                       
                    }
                }
            }

            return pdfTable;

        }




        public class TableBorderCollection
        {
            public List<TableBorder> HBorders { get; set; }
            public List<TableBorder> VBorders { get; set; }
        }


    

    }
}
