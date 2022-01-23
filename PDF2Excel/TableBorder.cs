using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF2Excel
{
    public class TableBorder
    {
        public int BorderId;
        public Rectangle Border;
        public List<KeyValuePair<int, Rectangle>> IntersectBorders;
        public int TableId;

        public TableBorder(int borderNumber, Rectangle border)
        {
            this.BorderId = borderNumber;
            this.Border = border;
            this.TableId = -1;
            IntersectBorders = new List<KeyValuePair<int, Rectangle>>();
        }

        public TableBorder(int borderNumber, Rectangle border, List<KeyValuePair<int, Rectangle>> intersectBorders) :
            this(borderNumber, border)
        {
            this.IntersectBorders.AddRange(intersectBorders);
        }

        public void AddIntersectBorder(int borderNumber, Rectangle intersectBorder)
        {
            this.IntersectBorders.Add(new KeyValuePair<int, Rectangle>(borderNumber, intersectBorder));
        }

        /// <summary>
        /// 判断是否相交
        /// </summary>
        /// <returns></returns>
        public  bool Overlaps(TableBorder border1)
        {
            Rectangle mRect1 = new Rectangle(Border.GetX() - 1, Border.GetY() - 1,
                  Border.GetWidth() + 2, Border.GetHeight() + 2);
            Rectangle mRect2 = new Rectangle(border1.Border.GetX() - 1, border1.Border.GetY() - 1,
                border1.Border.GetWidth() + 2, border1.Border.GetHeight() + 2);
            return mRect1.Overlaps(mRect2);

        }
        /// <summary>
        /// 是否是同一竖直的线条
        /// </summary>
        /// <param name="border1"></param>
        /// <returns></returns>
        public bool IsEqVBorder(TableBorder border1)
        {
            Rectangle mRect1 = new Rectangle(Border.GetX() - 1, Border.GetY() - 1,
                 Border.GetWidth() + 2, Border.GetHeight() + 2);
            Rectangle mRect2 = new Rectangle(border1.Border.GetX() - 1, Border.GetY() - 1,
                border1.Border.GetWidth() + 2, border1.Border.GetHeight() + 2);
            return mRect1.Overlaps(mRect2);

        }

        /// <summary>
        /// 是否是同一水平的线条
        /// </summary>
        /// <param name="border1"></param>
        /// <returns></returns>
        public bool IsEqHBorder(TableBorder border1)
        {
            Rectangle mRect1 = new Rectangle(Border.GetX() - 1, Border.GetY() - 1,
                 Border.GetWidth() + 2, Border.GetHeight() + 2);
            Rectangle mRect2 = new Rectangle(Border.GetX() - 1, border1.Border.GetY() - 1,
                border1.Border.GetWidth() + 2, border1.Border.GetHeight() + 2);
            return mRect1.Overlaps(mRect2);

        }
        /// <summary>
        /// 返回两条线的相交的矩形
        /// </summary>
        /// <param name="border1"></param>
        /// <returns></returns>
        public Rectangle Intersection(TableBorder border1)
        {
            Rectangle HBorder=null;
            Rectangle VBorder = null;
            if (Border.GetWidth() > Border.GetHeight() * 1.5 && border1.Border.GetHeight() > border1.Border.GetWidth() * 1.5)
            {
                HBorder = this.Border;
                VBorder = border1.Border;
            }
            else if (border1.Border.GetWidth() > border1.Border.GetHeight() * 1.5 && Border.GetHeight() > Border.GetWidth() * 1.5)
            {
                HBorder = border1.Border;
                VBorder = this.Border;
            }

            if (HBorder == null || VBorder == null) return null;

            return new Rectangle(VBorder.GetX(), HBorder.GetY(), HBorder.GetWidth(), VBorder.GetHeight());
        }
    }
}
