using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocViewerDemo.DrawEntity
{
    /// <summary>
    /// 绘制Path
    /// Path中包含Line、Arc、Bezier（三次），类似于SVG中的Path
    /// 坐标点使用绝对坐标系
    /// </summary>
    public class DrawEntity_Path : DrawEntity
    {
        //曲线数据
        public PathData PathData = new PathData();

        Color color = Color.White;
        int width = 1;
        Vector offset = new Vector();

        public DrawEntity_Path(Vector offset,PathData pathData, Color color, int width)
        {
            this.PathData = pathData;
            this.color = color;
            this.width = width;
            this.offset = offset;
        }

        //绘制函数
        public override void Draw()
        {
            int index = 0;
            //遍历曲线数据
            foreach (var item in PathData.listDatas)
            {
                index++;
                //绘制直线
                if (item is PathItemLine)
                {
                    var lineData = item as PathItemLine;
                    DrawLine(new Vector(lineData.StartPoint.x + offset.x, lineData.StartPoint.y + offset.y, 0), new Vector(lineData.EndPoint.x + offset.x, lineData.EndPoint.y + offset.y, 0), width, color);
                    //DrawText(new Vector(lineData.StartPoint.x + offset.x, lineData.StartPoint.y + offset.y, 0), index.ToString(), new Font("宋体", 1), 1, Color.Green);
                }
                //绘制圆弧
                else if(item is PathItemArc)
                {
                    var arcData = item as PathItemArc;
                    //计算凸度
                    double bulge = Math.Tan(arcData.SweepAngle / 180 * Math.PI / 4);
                    //计算圆弧圆心
                    double centerX, centerY, radius, startAngle, sweepAngle;
                    TranslateArcFromBulge(arcData.StartPoint.x, arcData.StartPoint.y, arcData.EndPoint.x, arcData.EndPoint.y, bulge, out centerX, out centerY, out radius, out startAngle, out sweepAngle);

                    DrawArc(new Vector(centerX + offset.x, centerY + offset.y, 0), radius, startAngle, sweepAngle, width, color);
                }
                //绘制Bezier
                else if(item is PathItemBezier)
                {
                    var bezierData = item as PathItemBezier;
                    PointF p1, p2, p3, p4;
                    p1 = new PointF((float)(bezierData.StartPoint.x + offset.x), (float)(bezierData.StartPoint.y + offset.y));
                    p2 = new PointF((float)(bezierData.ControlPoint1.x + offset.x), (float)(bezierData.ControlPoint1.y + offset.y));
                    p3 = new PointF((float)(bezierData.ControlPoint2.x + offset.x), (float)(bezierData.ControlPoint2.y + offset.y));
                    p4 = new PointF((float)(bezierData.EndPoint.x + offset.x), (float)(bezierData.EndPoint.y + offset.y));

                    DrawBezier(p1, p2, p3, p4,color,width);
                }
            }

            //是否闭合
            if(PathData.isClosed && PathData.listDatas.Count>1)
            {
                int count = PathData.listDatas.Count;
                //添加闭合直线
                DrawLine(new Vector(PathData.listDatas[0].StartPoint.x, PathData.listDatas[0].StartPoint.y, 0), new Vector(PathData.listDatas[count - 1].EndPoint.x, PathData.listDatas[count - 1].EndPoint.y, 0), width, color);
            }
        }

        //转换通过《起点、终点、凸度》定义的圆弧
        // b = 0.5*(1/bulge - bulge)
        // centerX = 0.5*((x1+x2)-b*(y2-y1))
        // centerY = 0.5*((y1+y2)+b*(x2-x1))
        public void TranslateArcFromBulge(double startX, double startY, double endX, double endY, double bulge, out double centerX, out double centerY, out double radius, out double startAngle, out double sweepAngle)
        {

            double b = 0.5 * (1 / bulge - bulge);
            //计算圆心
            centerX = 0.5 * ((startX + endX) - b * (endY - startY));
            centerY = 0.5 * ((startY + endY) + b * (endX - startX));
            //计算圆心
            radius = Math.Sqrt(Math.Pow(centerX - startX, 2) + Math.Pow(centerY - startY, 2));

            //计算起始角
            Vector vectorCentorToStart = new Vector(startX - centerX, startY - centerY, 0);
            startAngle = Math.Atan2(vectorCentorToStart.y / radius, vectorCentorToStart.x / radius) / Math.PI * 180;

            //计算终止角
            Vector vectorCentorToEnd = new Vector(endX - centerX, endY - centerY, 0);
            double endAngle = Math.Atan2(vectorCentorToEnd.y / radius, vectorCentorToEnd.x / radius) / Math.PI * 180;

            //计算扫描角度
            sweepAngle = Math.Atan(bulge) * 4 / Math.PI * 180;


            if (bulge > 0)
            {
                sweepAngle = Math.Abs(sweepAngle);
            }
            else
            {
                sweepAngle = Math.Abs(sweepAngle) * -1;
            }

        }
    }

    /// <summary>
    /// Path数据
    /// </summary>
    public class PathData
    {
        /// <summary>
        /// 曲线数据，列表中的曲线第一点与前一条曲线的最后一点重合
        /// </summary>
        public List<PathItemBase> listDatas = new List<PathItemBase>();

        /// <summary>
        /// 是否闭合
        /// </summary>
        public bool isClosed = false;

        /// <summary>
        /// 判断轮廓时顺时针还是逆时针
        /// 使用Green公式判断
        /// </summary>
        /// <returns></returns>
        public bool IsCloseWise()
        {
            if(listDatas.Count<2)
            {
                return true;
            }

            double[] x = new double[listDatas.Count + 1];
            double[] y = new double[listDatas.Count + 1];

            x[0] = listDatas[0].StartPoint.x;
            y[0] = listDatas[0].StartPoint.y;

            for (int i = 0; i < listDatas.Count; i++)
            {
                x[i + 1] = listDatas[i].EndPoint.x;
                y[i + 1] = listDatas[i].EndPoint.y;
            }

            double d = 0;
            for (int i = 0; i < x.Length - 1; i++)
            {
                d += -0.5 * (y[i + 1] + y[i]) * (x[i + 1] - x[i]);
            }

            if (d>0)
            {
                return false;
            }
            else
            {
                return true;
            }

            
        }
    }


    /// <summary>
    /// 数据基类
    /// </summary>
    abstract public class PathItemBase
    {
        public Vector StartPoint = new Vector();
        public Vector EndPoint = new Vector();
    }


    public class PathItemLine : PathItemBase
    {
        
    }

    /// <summary>
    /// 两个端点和扫描角度，类似于CAD中的两个端点加凸度
    /// </summary>
    public class PathItemArc : PathItemBase
    {
        public double SweepAngle = 0;
    }

    /// <summary>
    /// 三次贝塞尔曲线，包含两个端点和两个控制点
    /// </summary>
    public class PathItemBezier : PathItemBase
    {
        public Vector ControlPoint1 = new Vector();
        public Vector ControlPoint2 = new Vector();

        
    }

    public class PathItemBeziers : PathItemBase
    {
        public List<Vector> points = new List<Vector>();
    }

}//namespace
