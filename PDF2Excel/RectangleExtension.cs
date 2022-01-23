using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF2Excel
{
    public static class RectangleExtension
    {

        public static void Add(this Rectangle rectangle, Rectangle rectangle2)
        {
            List<Rectangle> list = new List<Rectangle>() { rectangle , rectangle2 };
            var rect= RectangleExtension.Merge(list);
            rectangle.SetX(rect.GetX());
            rectangle.SetY(rect.GetY());
            rectangle.SetWidth(rect.GetWidth());
            rectangle.SetHeight(rect.GetHeight());
        }

        public static bool XOverlaps(this Rectangle rectangle, Rectangle rectangle2)
        {
            if ((rectangle2.GetX() >= rectangle.GetX() && (rectangle.GetX() + rectangle.GetWidth()) >= rectangle2.GetX())
                || (rectangle.GetX() >= rectangle2.GetX() && (rectangle2.GetX() + rectangle2.GetWidth()) >= rectangle.GetX()))
            {
                return true;
            }
            return false;
        }

        public static Rectangle Merge(List<Rectangle> rectangles)
        {
            float minX=0, minY=0, maxX=0, maxY=0;
            for (int i = 0; i < rectangles.Count; i++)
            {
                var rect = rectangles[i];
                if (i == 0)
                {
                    minX = rect.GetX();
                    minY = rect.GetY();
                    maxX = rect.GetX() + rect.GetWidth();
                    maxY = rect.GetY() + rect.GetHeight();
                }
                else
                {
                    if (minX > rect.GetX())
                    {
                        minX = rect.GetX();
                    }
                    if (minY > rect.GetY())
                    {
                        minY = rect.GetY();
                    }
                    if (maxX < rect.GetX() + rect.GetWidth())
                    {
                        maxX = rect.GetX() + rect.GetWidth();
                    }
                    if (maxY < rect.GetY() + rect.GetHeight())
                    {
                        maxY = rect.GetY() + rect.GetHeight();
                    }
                }

            }
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
