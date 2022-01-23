using System;
using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Colors;
using PDF2Excel;

namespace TableExtractionFromPDF
{

    public class FilterTableEventListener : IEventListener
    {
        private PdfPage pdfPage = null;
        private int borderCounter = 0, tableCounter = 0;
        private List<TableBorder> sqBorders = new List<TableBorder>();
        private List<TableBorder> hBorders = new List<TableBorder>();
        private List<TableBorder> vBorders = new List<TableBorder>();
       
        private IList<IEventData> textRenderList = new List<IEventData>();

        private List<PdfTable> tables = new List<PdfTable>();

        /// <summary>
        /// Process a PDF page to retrieve tables data from it.
        /// </summary>
        /// <param name="pdfPage">the pdf page which to process</param>
        /// <param name="withBorder">true if tables have fully borders, false otherwise</param>
        public FilterTableEventListener(PdfPage pdfPage, bool withBorder)
        {
            if (withBorder)
            {
                this.pdfPage = pdfPage;
                PdfCanvasProcessor processor = new PdfCanvasProcessor(this);
                processor.ProcessPageContent(pdfPage);
                GetTablesFromborders();
            }
        }

        private void GetTablesFromborders()
        {

            if (hBorders.Count == 0 && vBorders.Count == 0)
            {
                PdfTable pdfTable = new PdfTable();
                pdfTable.IsText = true;
                tables.Add(pdfTable);
                PdfRow pdfRow = new PdfRow();
                PdfCell pdfCell = new PdfCell();
                pdfRow.AddCell(pdfCell);
                pdfTable.Rows.Add(pdfRow);
                pdfCell.Rectangle = pdfPage.GetPageSize();
                pdfCell.Text =  PdfTextExtractor.GetTextFromPage(pdfPage);//读取第i页的文档内
                return;
            }
     
            hBorders.Sort(delegate (TableBorder border1, TableBorder border2)
            {
                if (border1 == null && border2 == null) return 0;
                else if (border1 == null) return -1;
                else if (border2 == null) return 1;
                else
                {
                    int comparerInt = border2.Border.GetY().CompareTo(border1.Border.GetY());
                    if (comparerInt == 0)
                    {
                        comparerInt = border1.Border.GetX().CompareTo(border2.Border.GetX());
                    }
                    return comparerInt;
                }
            });
            if (hBorders.Count > 0 && vBorders.Count == 0)
            {
                vBorders= FillVerticalBorder(hBorders);
            }

            vBorders.Sort(delegate (TableBorder border1, TableBorder border2)
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
            tables = PdfTableHelper.ConvertTables(hBorders, vBorders);

            Rectangle AllTableRect = new Rectangle(0,0);
            foreach (var tab in tables)
            {
                AllTableRect.Add(tab.GetTableSize());
                foreach (var row in tab.Rows)
                {
                    foreach (var cell in row.Cells)
                    {
                        if (!cell.IsHMerge && !cell.IsVMerge)
                        {
                            cell.Text = GetTextFromRectangle(cell.Rectangle).Trim();
                        }
                    }
                }
            }

            //表格重新排序
            tables.Sort(delegate (PdfTable table1, PdfTable table2)
            {
                if (table1 == null && table2 == null) return 0;
                else if (table1 == null) return -1;
                else if (table2 == null) return 1;
                else
                {
                    var rect1 = table1.GetTableSize();
                    var rect2 = table2.GetTableSize();
                    int comparerInt = rect2.GetY().CompareTo(rect1.GetY());
                    if (comparerInt == 0)
                    {
                        comparerInt = rect2.GetX().CompareTo(rect1.GetX());
                    }
                    return comparerInt;
                }
            });
            //获取表头

            for (int i = 0; i < tables.Count; i++)
            {
                var pagesize = pdfPage.GetPageSize();
                if (i == 0)
                {
                    var table = tables[i];
                    var tableRect = table.GetTableSize();
                    Rectangle headerRect = new Rectangle(0, tableRect.GetY() + tableRect.GetHeight()
                        , pagesize.GetWidth(), pagesize.GetHeight() - tableRect.GetY()- tableRect.GetHeight());
                    table.Header = GetTextFromRectangle(headerRect);
                }
                else
                {
                    var tableRect = tables[i].GetTableSize();
                    var prevTableRect= tables[i-1].GetTableSize();
                    Rectangle headerRect = new Rectangle(0, tableRect.GetY() + tableRect.GetHeight()
                        , pagesize.GetWidth(), prevTableRect.GetY() - tableRect.GetY()- tableRect.GetHeight());
                    tables[i].Header = GetTextFromRectangle(headerRect);
                }
                

            }
        }
        /// <summary>
        /// 当只有横向没有竖线的时候 填充竖线
        /// </summary>
        /// <param name="hBorders"></param>
        /// <returns></returns>
        public List<TableBorder> FillVerticalBorder(List<TableBorder> hBorders )
        {
            float borderWidth = 1;
            List<List<Rectangle>> list = new List<List<Rectangle>>();
            float y = hBorders[hBorders.Count - 1].Border.GetY();
            float x= hBorders[hBorders.Count - 1].Border.GetX();
            float MaxX = hBorders[hBorders.Count - 1].Border.GetX() + hBorders[hBorders.Count - 1].Border.GetWidth() - hBorders[hBorders.Count - 1].Border.GetHeight() ;
            float VlineHeight = hBorders[0].Border.GetY() + hBorders[0].Border.GetHeight() - hBorders[hBorders.Count - 1].Border.GetY();
            for (int i = 1; i < hBorders.Count; i++)
            {
                if (i == 1)
                {
                    borderWidth = hBorders[i].Border.GetHeight();
                }
                if (hBorders[i].Border.GetX() > x)
                {
                    x = hBorders[i].Border.GetX();
                }
                Rectangle rectangle = new Rectangle(hBorders[i].Border.GetX(), hBorders[i].Border.GetY()
                    , hBorders[i].Border.GetWidth(), hBorders[i - 1].Border.GetY() - hBorders[i].Border.GetY());

                list.Add(GetTextRectFromRectangle(rectangle));
                
            }

            List<Rectangle> VRects = CreateVBorder(list);
            VRects.Sort(delegate (Rectangle react1, Rectangle react2)
            {
                if (react1 == null && react2 == null) return 0;
                else if (react1 == null) return -1;
                else if (react2 == null) return 1;
                else
                {
                    int comparerInt = react1.GetX().CompareTo(react2.GetX());
                    if (comparerInt == 0)
                    {
                        comparerInt = react1.GetY().CompareTo(react2.GetY());
                    }
                    return comparerInt;
                }
            });

            List<TableBorder> vBorders = new List<TableBorder>();
            TableBorder startTableBorder = new TableBorder(10000 , new Rectangle(x, y, borderWidth, VlineHeight));
            vBorders.Add(startTableBorder);
            for (int i = 1; i < VRects.Count; i++)
            {
                var prevRect = VRects[i - 1];
                var rect= VRects[i];
                float x1= VRects[i - 1].GetX() + VRects[i - 1].GetWidth()+(rect.GetX() - VRects[i - 1].GetX() - VRects[i - 1].GetWidth())/2;
                TableBorder tableBorder = new TableBorder(10000 + i, new Rectangle(x1, y, borderWidth, VlineHeight));
                vBorders.Add(tableBorder);

            }
            TableBorder endTableBorder = new TableBorder(10000+ VRects.Count, new Rectangle(MaxX, y, borderWidth, VlineHeight));
            vBorders.Add(endTableBorder);
            return vBorders;
        }


        public string GetTextFromRectangle(Rectangle rectangle)
        {
            string rectText = String.Empty;

            TextRegionEventFilter textRegionEventFilter = new TextRegionEventFilter(rectangle);
            LocationTextExtractionStrategy extractionStrategy = new LocationTextExtractionStrategy();

            foreach (IEventData textRender in textRenderList)
            {
                if (textRegionEventFilter.IsInsideRectangle(textRender, EventType.RENDER_TEXT))
                {
                    extractionStrategy.EventOccurred(textRender, EventType.RENDER_TEXT);
                }
                else if (textRegionEventFilter.Accept(textRender, EventType.RENDER_TEXT))
                {
                    TextRenderInfo textRenderInfo = (TextRenderInfo)textRender;
                    IList<TextRenderInfo> renderInfoList = textRenderInfo.GetCharacterRenderInfos();
                    for (int index = 0; index < renderInfoList.Count(); index++)
                    {
                        if (textRegionEventFilter.IsInsideRectangle(renderInfoList[index], EventType.RENDER_TEXT))
                        {
                            extractionStrategy.EventOccurred(renderInfoList[index], EventType.RENDER_TEXT);
                        }
                    }
                }
            }
            rectText = extractionStrategy.GetResultantText();
            if (rectText.StartsWith("\""))
            { 
                rectText.Substring(1); 
            }
            if (rectText.EndsWith("\""))
            { rectText.Substring(0, rectText.Length-1); }
            return rectText.Trim();
        }


        public List<Rectangle> GetTextRectFromRectangle(Rectangle rectangle)
        {
            List<Rectangle> rectangles =new List<Rectangle>();

            TextRegionEventFilter textRegionEventFilter = new TextRegionEventFilter(rectangle);
            LocationTextExtractionStrategy extractionStrategy = new LocationTextExtractionStrategy();

            foreach (IEventData textRender in textRenderList)
            {
                if (textRender is TextRenderInfo)
                {
                    var textRenderInfo = (TextRenderInfo)textRender;
                    if (textRegionEventFilter.IsInsideRectangle(textRenderInfo, EventType.RENDER_TEXT))
                    {
                        IList<TextRenderInfo> renderInfoList = textRenderInfo.GetCharacterRenderInfos();

                        Rectangle rectangle1 = null;
                        for (int index = 0; index < renderInfoList.Count(); index++)
                        {
                            if (textRegionEventFilter.IsInsideRectangle(renderInfoList[index], EventType.RENDER_TEXT))
                            {
                                LineSegment segment = renderInfoList[index].GetBaseline();
                                LineSegment segmengtAsc = renderInfoList[index].GetAscentLine();
                                //文字的左下角坐标
                                Vector startPoint = segment.GetStartPoint();
                                //文字的右上角坐标
                                Vector topRight = segmengtAsc.GetEndPoint();
                                float x1 = startPoint.Get(Vector.I1);
                                float y1 = startPoint.Get(Vector.I2);
                                float x2 = topRight.Get(Vector.I1);
                                float y2 = topRight.Get(Vector.I2);
                                if (rectangle1 == null)
                                    rectangle1 = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                                else
                                {
                                     rectangle1.Add(new Rectangle(x1, y1, x2 - x1, y2 - y1));
                                }
                            }
                        }

                        rectangles.Add(rectangle1);
                    }
                }


             
            }

            return rectangles;
        }

        /// <summary>
        /// 通过字体rect 返回垂直TableBordes
        /// </summary>
        /// <param name="fontRects"></param>
        public List<Rectangle> CreateVBorder(List<List<Rectangle>> fontRects)
        {
            if (fontRects == null) return null;
            List<List<Rectangle>> newList = new List<List<Rectangle>>();
            List<Rectangle> newList2 = new List<Rectangle>();
            bool noSpace = true;
            for (int i = 0; i < fontRects.Count; i++)
            {
                var rects = fontRects[i];
                newList.Add(CreateHRectange(rects));
                if (newList.Count > 1 && i>0)
                {
                    noSpace &= newList[newList.Count - 1].Count == newList[newList.Count - 2].Count;
                }
            }
           
      
            //
            if (noSpace)
            {
                for (int i = 0; i < newList[0].Count; i++)
                {
                    List<Rectangle> list2 = new List<Rectangle>();
                    for (int j = 0; j < newList.Count; j++)
                    {
                        list2.Add(newList[j][i]);
                    }
                    newList2.Add(RectangleExtension.Merge(list2));
                }
            }
            else
            {
               var maxCount  = newList.Max(p=>p.Count());
               var maxList= newList.Where(p => p.Count == maxCount).ToList();
                for (int i = 0; i < maxList[0].Count; i++)
                {
                    List<Rectangle> list2 = new List<Rectangle>();
                    for (int j = 0; j < maxList.Count; j++)
                    {
                        list2.Add(maxList[j][i]);
                    }
                    newList2.Add(RectangleExtension.Merge(list2));
                }
            }
            return newList2;


        }
        /// <summary>
        /// 分析横向的Rectange
        /// </summary>
        /// <returns></returns>
        public List<Rectangle> CreateHRectange(List<Rectangle> rects)
        {
            rects.Sort(delegate (Rectangle react1, Rectangle react2)
            {
                if (react1 == null && react2 == null) return 0;
                else if (react1 == null) return -1;
                else if (react2 == null) return 1;
                else
                {
                    int comparerInt = react2.GetY().CompareTo(react1.GetY());
                    if (comparerInt == 0)
                    {
                        comparerInt = react1.GetX().CompareTo(react2.GetX());
                    }
                    return comparerInt;
                }
            });
            List<Rectangle> newRects = new List<Rectangle>();
            float maxSpac = 0;
            for (int i = 0; i < rects.Count; i++)
            {
                var rect = rects[i];
                if (i == 0) { newRects.Add(rects[i]); }
                else
                {
                    var  prevRect = newRects[newRects.Count - 1];
                    bool isSamLine = !(prevRect.GetY() > rect.GetY() + rect.GetHeight() || rect.GetY() > prevRect.GetY() + prevRect.GetHeight());
                    if (!isSamLine)
                    {
                        newRects.Add(rects[i]);
                        continue;
                    }
                    maxSpac = maxSpac > rect.GetWidth() ? maxSpac : rect.GetWidth();
                    if (rect.GetX() + rect.GetWidth() > prevRect.GetX() + prevRect.GetWidth())
                    {
                        if (rect.GetX() - (prevRect.GetX() + prevRect.GetWidth()) > maxSpac)
                        {
                            newRects.Add(rects[i]);
                        }
                        else
                        {
                            prevRect.SetWidth(rect.GetX() + rect.GetWidth() - prevRect.GetX());
                            prevRect.SetHeight(rect.GetY() + rect.GetHeight() > prevRect.GetY() + prevRect.GetHeight() ? rect.GetY() + rect.GetHeight() - prevRect.GetY() : prevRect.GetY() + prevRect.GetHeight() - rect.GetY());
                        }
                    }
                    else
                    {
                        if (prevRect.GetX() - (rect.GetX() + rect.GetWidth()) > maxSpac)
                        {
                            newRects.Add(rects[i]);
                        }
                        else
                        {
                            prevRect.SetX(rect.GetX());
                            prevRect.SetHeight(rect.GetY() + rect.GetHeight() > prevRect.GetY() + prevRect.GetHeight() ? rect.GetY() + rect.GetHeight() : prevRect.GetY() + prevRect.GetHeight());
                        }
                    }
                }

            }
            if (newRects.Count == 0) return null;
            List<Rectangle> endRects = new List<Rectangle>();
            Rectangle maxRect= null;
            while (true)
            {
                if (newRects.Count == 0)
                {
                    break;
                }
                if (maxRect == null)
                {
                    maxRect = newRects[0];
                }
                var list1=  newRects.Where(p => p.XOverlaps(maxRect)).ToList();// 获取全部X轴相交的
                if (list1 == null || list1.Count <= 1)
                {
                    endRects.Add(maxRect);
                    newRects.Remove(maxRect);
                    maxRect = null;
                }
                else
                {
                    maxRect = RectangleExtension.Merge(list1);
                    foreach (var item in list1)
                    {
                        newRects.Remove(item);
                    }
                    endRects.Add(maxRect);
                    maxRect = null;
                }
                
            }
            endRects.Sort(delegate (Rectangle react1, Rectangle react2)
            {
                if (react1 == null && react2 == null) return 0;
                else if (react1 == null) return -1;
                else if (react2 == null) return 1;
                else
                {
                    int comparerInt = react1.GetX().CompareTo(react2.GetX());
                    if (comparerInt == 0)
                    {
                        comparerInt = react1.GetY().CompareTo(react2.GetY());
                    }
                    return comparerInt;
                }
            });
            return endRects;

        }

        public virtual void EventOccurred(IEventData iEventData, EventType eventType)
        {
            if (eventType == EventType.RENDER_PATH)
            {
                PathRenderInfo pathInfo = (PathRenderInfo)iEventData;
                Color colorFill = iEventData.GetGraphicsState().GetFillColor();
                Color colorStroke = iEventData.GetGraphicsState().GetStrokeColor();
                int pathOperation = pathInfo.GetOperation();

                if (!pathInfo.IsPathModifiesClippingPath() && pathOperation != PathRenderInfo.NO_OP)
                {
                    if (pathOperation == PathRenderInfo.STROKE)
                    {
                        if (!Color.IsWhite(colorStroke.GetColorSpace(), colorStroke.GetColorValue()))
                        {
                            Path path = pathInfo.GetPath().TransformPath(pathInfo.GetCtm(), false);
                            ProcessPathAsline(path, pathInfo.GetLineWidth());
                        }
                    }
                    else if (pathOperation == PathRenderInfo.FILL)
                    {
                        if (!Color.IsWhite(colorFill.GetColorSpace(), colorFill.GetColorValue()))
                        {
                            Path path = pathInfo.GetPath().TransformPath(pathInfo.GetCtm(), false);
                            ProcessPathAsRectangle(path);
                        }
                    }
                    else if (pathOperation == (int)(PathRenderInfo.STROKE | PathRenderInfo.FILL))
                    {
                        if (!Color.IsWhite(colorFill.GetColorSpace(), colorFill.GetColorValue()))
                        {
                            Path path = pathInfo.GetPath().TransformPath(pathInfo.GetCtm(), false);
                            ProcessPathAsRectangle(path);
                        }
                        else if (!Color.IsWhite(colorStroke.GetColorSpace(), colorStroke.GetColorValue()) &&
                            Color.IsWhite(colorFill.GetColorSpace(), colorFill.GetColorValue()))
                        {
                            Path path = pathInfo.GetPath().TransformPath(pathInfo.GetCtm(), false);
                            ProcessPathAsline(path, pathInfo.GetLineWidth());
                        }
                    }
                }
            }
            else if (eventType == EventType.RENDER_TEXT)
            {
                if (iEventData is AbstractRenderInfo)
                {
                    ((AbstractRenderInfo)iEventData).PreserveGraphicsState();
                    textRenderList.Add(iEventData);
                }
            }
        }

        private void ProcessPathAsRectangle(Path path)
        {
            foreach (Subpath subpath in path.GetSubpaths())
            {
                IList<IShape> segments = subpath.GetSegments();

                if (segments.Count == 3 && subpath.IsClosed())
                {

                    Line line1 = segments[0].GetType() == typeof(Line) ? (Line)segments[0] : null;
                    Point p11 = line1 != null ? line1.GetBasePoints()[0] : null;
                    Point p12 = line1 != null ? line1.GetBasePoints()[1] : null;

                    Line line2 = segments[1].GetType() == typeof(Line) ? (Line)segments[1] : null;
                    Point p21 = line2 != null ? line2.GetBasePoints()[0] : null;
                    Point p22 = line2 != null ? line2.GetBasePoints()[1] : null;

                    Line line3 = segments[2].GetType() == typeof(Line) ? (Line)segments[2] : null;
                    Point p31 = line3 != null ? line3.GetBasePoints()[0] : null;
                    Point p32 = line3 != null ? line3.GetBasePoints()[1] : null;

                    if (line1 != null && line2 != null && line3 != null
                        && (p11.x == p12.x || p11.y == p12.y)
                        && (p21.x == p22.x || p21.y == p22.y)
                        && (p31.x == p32.x || p31.y == p32.y))
                    {
                        Rectangle rect = Rectangle.CreateBoundingRectangleFromQuadPoint(
                            new PdfArray(new double[] { p11.x, p11.y, p12.x, p12.y, p22.x, p22.y, p32.x, p32.y }));

                        ProcessBorder(rect);
                    }
                }
            }
        }

        private Rectangle FullJoinRectangle(Rectangle tempRect, Rectangle border)
        {
            try
            {
                double minX, minY, maxX, maxY;
                minX = Math.Min(tempRect.GetX(), border.GetX());
                minY = Math.Min(tempRect.GetY(), border.GetY());
                maxX = Math.Max(tempRect.GetX() + tempRect.GetWidth(), border.GetX() + border.GetWidth());
                maxY = Math.Max(tempRect.GetY() + tempRect.GetHeight(), border.GetY() + border.GetHeight());

                return Rectangle.CreateBoundingRectangleFromQuadPoint(
                        new PdfArray(new double[] { minX, minY, maxX, minY, minX, maxY, maxX, maxY }));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void ProcessPathAsline(Path path, float lineWidth)
        {
            foreach (Subpath subpath in path.GetSubpaths())
            {
                if (subpath.IsClosed())
                {
                    path.ReplaceCloseWithLine();
                }

                IList<IShape> segments = subpath.GetSegments();
                double resltWidth = lineWidth <= 0 ? 0.2d : lineWidth;

                foreach (IShape segment in segments)
                {
                    Line line = segment.GetType() == typeof(Line) ? (Line)segment : null;
                    Point p1 = line != null ? line.GetBasePoints()[0] : null;
                    Point p2 = line != null ? line.GetBasePoints()[1] : null;

                    if (line != null)
                    {
                        if (p1.x == p2.x)
                        {
                            Rectangle rect = Rectangle.CreateBoundingRectangleFromQuadPoint(
                            new PdfArray(new double[] { p1.x - (resltWidth / 2), p1.y, p2.x - (resltWidth / 2), p2.y,
                                p1.x + (resltWidth / 2), p1.y, p2.x + (resltWidth / 2), p2.y }));
                            ProcessBorder(rect);
                        }
                        else if (p1.y == p2.y)
                        {
                            Rectangle rect = Rectangle.CreateBoundingRectangleFromQuadPoint(
                            new PdfArray(new double[] { p1.x, p1.y - (resltWidth / 2), p2.x, p2.y - (resltWidth / 2),
                                p1.x, p1.y + (resltWidth / 2), p2.x, p2.y + (resltWidth / 2) }));
                            ProcessBorder(rect);
                        }
                    }
                }
            }
        }

        private void ProcessBorder(Rectangle rect)
        {
            Rectangle mRect = new Rectangle(rect.GetX() - 1, rect.GetY() - 1, rect.GetWidth() + 2, rect.GetHeight() + 2);
            //////////////////////////VB///////////////////
            if (rect.GetHeight() > rect.GetWidth() * 1.5)
            {
                List<TableBorder> sqList = sqBorders.Where(sq => mRect.Overlaps(sq.Border)).ToList();
                List<TableBorder> tempSqList = new List<TableBorder>();

                foreach (TableBorder sq in sqList)
                {
                    rect = VJoinRectangle(rect, sq.Border);
                    //TODO m SQ
                    TableBorder tempSq = sq;
                    sqBorders.Remove(sq);

                    if (IsHInside(rect, tempSq.Border))
                    {
                        tempSq.Border = HJoinRectangle(tempSq.Border, rect);
                        tempSq.Border = ProcessPartialyJoinRect(tempSq.Border, ref tempSqList, "F", "Sq");
                        tempSqList.Add(tempSq);
                    }
                }
                ////////hl///////
                mRect = new Rectangle(rect.GetX() - 1, rect.GetY() - 1, rect.GetWidth() + 2, rect.GetHeight() + 2);
                List<TableBorder> hList = hBorders.Where(hb => mRect.Overlaps(hb.Border)).ToList();
                List<TableBorder> tempHlList = new List<TableBorder>();

                foreach (TableBorder hl in hList)
                {
                    rect = VJoinRectangle(rect, hl.Border);
                    //TODO m hl
                    TableBorder tempHl = hl;
                    hBorders.Remove(hl);

                    if (IsHInside(rect, tempHl.Border))
                    {
                        tempHl.Border = HJoinRectangle(tempHl.Border, rect);
                        tempHl.Border = ProcessPartialyJoinRect(tempHl.Border, ref tempHlList, "F", "HL");
                        tempHl.Border = ProcessPartialyJoinRect(tempHl.Border, ref tempSqList, "H", "Sq");
                        tempHlList.Add(tempHl);
                    }
                }
                ///////////VL////////////
                mRect = new Rectangle(rect.GetX() - 1, rect.GetY() - 1, rect.GetWidth() + 2, rect.GetHeight() + 2);
                List<TableBorder> vList = vBorders.Where(vb => mRect.Overlaps(vb.Border)).ToList();
                AddPBorder(ref rect, ref vList, ref tempHlList, ref tempSqList, false);
            }
            //////////////////////////HB///////////////////
            else if (rect.GetWidth() > rect.GetHeight() * 1.5)
            {
                /////SQ/////
                List<TableBorder> sqList = sqBorders.Where(sq => mRect.Overlaps(sq.Border)).ToList();
                List<TableBorder> tempSqList = new List<TableBorder>();

                foreach (TableBorder sq in sqList)
                {
                    rect = HJoinRectangle(rect, sq.Border);
                    //TODO m SQ
                    TableBorder tempSq = sq;
                    sqBorders.Remove(sq);

                    if (IsVInside(rect, tempSq.Border))
                    {
                        tempSq.Border = VJoinRectangle(tempSq.Border, rect);
                        tempSq.Border = ProcessPartialyJoinRect(tempSq.Border, ref tempSqList, "F", "Sq");
                        tempSqList.Add(tempSq);
                    }
                }
                /////VL///////
                mRect = new Rectangle(rect.GetX() - 1, rect.GetY() - 1, rect.GetWidth() + 2, rect.GetHeight() + 2);
                List<TableBorder> vList = vBorders.Where(vb => mRect.Overlaps(vb.Border)).ToList();
                List<TableBorder> tempVlList = new List<TableBorder>();

                foreach (TableBorder vl in vList)
                {
                    rect = HJoinRectangle(rect, vl.Border);
                    //TODO m hl
                    TableBorder tempVl = vl;
                    vBorders.Remove(vl);

                    if (IsVInside(rect, tempVl.Border))
                    {
                        tempVl.Border = VJoinRectangle(tempVl.Border, rect);
                        tempVl.Border = ProcessPartialyJoinRect(tempVl.Border, ref tempVlList, "F", "VL");
                        tempVl.Border = ProcessPartialyJoinRect(tempVl.Border, ref tempSqList, "V", "Sq");
                        tempVlList.Add(tempVl);
                    }
                }
                //////HL////////
                mRect = new Rectangle(rect.GetX() - 1, rect.GetY() - 1, rect.GetWidth() + 2, rect.GetHeight() + 2);
                List<TableBorder> hList = hBorders.Where(hb => mRect.Overlaps(hb.Border)).ToList();

                AddPBorder(ref rect, ref hList, ref tempVlList, ref tempSqList, true);
            }
            /////////////////////////SqB///////////////////
            else
            {
                ProcessSQBorder(ref rect, mRect);
            }
        }

        private Rectangle ProcessPartialyJoinRect(Rectangle rect, ref List<TableBorder> bList, string joinType, string listType)
        {
            Rectangle mR = new Rectangle(rect.GetX() - 1, rect.GetY() - 1, rect.GetWidth() + 2, rect.GetHeight() + 2);

            foreach (TableBorder bl in bList)
            {
                if (mR.Overlaps(bl.Border))
                {
                    rect = joinType.Equals("F") ? FullJoinRectangle(rect, bl.Border) : joinType.Equals("V") ?
                        VJoinRectangle(rect, bl.Border) : HJoinRectangle(rect, bl.Border);

                    if (joinType.Equals("F") || (joinType.Equals("V") && !IsHInside(rect, bl.Border))
                       || (joinType.Equals("H") && !IsVInside(rect, bl.Border)))
                    {
                        bList.Remove(bl);
                        if (listType.Equals("VL"))
                        {
                            if (vBorders.Any(t => t.BorderId == bl.BorderId))
                            {
                                vBorders.Remove(bl);
                            }
                        }
                        else if (listType.Equals("HL"))
                        {
                            if (hBorders.Any(t => t.BorderId == bl.BorderId))
                            {
                                hBorders.Remove(bl);
                            }
                        }
                        else
                        {
                            if (sqBorders.Any(t => t.BorderId == bl.BorderId))
                            {
                                sqBorders.Remove(bl);
                            }
                        }
                        break;
                    }
                }
            }
            return rect;
        }

        private bool IsVInside(Rectangle rect1, Rectangle rect2)
        {
            return !(rect1.GetY() <= rect2.GetY() && rect2.GetY() <= rect1.GetY() + rect1.GetHeight()) ||
            !(rect1.GetY() <= rect2.GetY() + rect2.GetHeight() && rect2.GetY() + rect2.GetHeight() <= rect1.GetY() + rect1.GetHeight());
        }

        private bool IsHInside(Rectangle rect1, Rectangle rect2)
        {
            return !(rect1.GetX() <= rect2.GetX() && rect2.GetX() <= rect1.GetX() + rect1.GetWidth()) || !(rect1.GetX() <=
                rect2.GetX() + rect2.GetWidth() && rect2.GetX() + rect2.GetWidth() <= rect1.GetX() + rect1.GetWidth());
        }

        private Rectangle HJoinRectangle(Rectangle hB, Rectangle jB)
        {
            float rx = hB.GetX();
            hB.SetX(Math.Min(rx, jB.GetX()));
            hB.SetWidth(Math.Max(rx + hB.GetWidth() - hB.GetX(), jB.GetX() + jB.GetWidth() - hB.GetX()));

            return hB;
        }

        private Rectangle VJoinRectangle(Rectangle vB, Rectangle jB)
        {
            float sry = vB.GetY();
            vB.SetY(Math.Min(jB.GetY(), sry));
            vB.SetHeight(Math.Max(jB.GetY() + jB.GetHeight() - vB.GetY(), sry + vB.GetHeight() - vB.GetY()));

            return vB;
        }

        private void ProcessSQBorder(ref Rectangle rect, Rectangle mRect)
        {
            List<TableBorder> sqList = sqBorders.Where(sq => mRect.Overlaps(sq.Border)).ToList();
            List<TableBorder> tempSqList = new List<TableBorder>();

            foreach (TableBorder sq in sqList)
            {
                rect = FullJoinRectangle(rect, sq.Border);
                sqBorders.Remove(sq);
            }

            mRect = new Rectangle(rect.GetX() - 1, rect.GetY() - 1, rect.GetWidth() + 2, rect.GetHeight() + 2);
            List<TableBorder> hList = hBorders.Where(hb => mRect.Overlaps(hb.Border)).ToList();

            if (hList != null && hList.Count > 1)
            {
                Rectangle rHl = hList[0].Border;

                foreach (TableBorder hl in hList)
                {
                    rHl = FullJoinRectangle(rHl, hl.Border);
                    hBorders.Remove(hl);
                }
                hBorders.Add(new TableBorder(borderCounter++, rHl));
            }

            List<TableBorder> vList = vBorders.Where(vb => mRect.Overlaps(vb.Border)).ToList();
            if (vList != null && vList.Count > 1)
            {
                Rectangle rvl = vList[0].Border;

                foreach (TableBorder vl in vList)
                {
                    rvl = FullJoinRectangle(rvl, vl.Border);
                    vBorders.Remove(vl);
                }
                vBorders.Add(new TableBorder(borderCounter++, rvl));
            }
            sqBorders.Add(new TableBorder(borderCounter++, rect));
        }

        private void AddPBorder(ref Rectangle rect, ref List<TableBorder> mList, ref List<TableBorder> pList, ref List<TableBorder> sqList, bool isVOrH)
        {
            foreach (TableBorder ml in mList)
            {
                rect = FullJoinRectangle(rect, ml.Border);

                if (isVOrH)
                {
                    hBorders.Remove(ml);
                }
                else
                {
                    vBorders.Remove(ml);
                }
            }
            if (isVOrH)
            {
                hBorders.Add(new TableBorder(borderCounter++, rect));
            }
            else
            {
                vBorders.Add(new TableBorder(borderCounter++, rect));
            }


            foreach (TableBorder pl in pList)
            {
                if (isVOrH)
                {
                    vBorders.Add(pl);
                }
                else
                {
                    hBorders.Add(pl);
                }

            }
            foreach (TableBorder sq in sqList)
            {
                sqBorders.Add(sq);
            }
        }

        public virtual ICollection<EventType> GetSupportedEvents()
        {
            return null;
        }

        /// <summary>
        /// Returns Dataset of retrieved tables from the PDF page.
        /// </summary>
        /// <returns></returns>
        public List<PdfTable> GetTables()
        {
            return tables;
        }
    }

}
