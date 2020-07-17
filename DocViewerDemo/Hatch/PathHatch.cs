using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocViewerDemo.DrawEntity;

namespace DocViewerDemo.Hatch
{
    /// <summary>
    /// Path闭合路径的填充
    /// </summary>
    public class PathHatch
    {
        /// <summary>
        /// 使用平行线填充封闭路径
        /// </summary>
        /// <param name="pathData">被填充的路径</param>
        /// <param name="dis">平行线间距</param>
        /// <param name="angle">平行线与X轴的角度</param>
        /// <returns></returns>
        public static List<PathData> FillPath_ByParallelLines(PathData pathData, double dis, double angle)
        {
            //判断被填充路径是否闭合
            if(pathData.isClosed == false)
            {
                return new List<PathData>();
            }

            //申明结果数据
            List<PathData> listResult = new List<PathData>();


            //将被填充路径中的Bezier转换成直线段
            var pathDataWithoutBezier = TransBezierToLines(pathData, 1);
            //加入最后一条闭合线
            pathDataWithoutBezier.listDatas.Add(new PathItemLine()
            {
                StartPoint =  new Vector(pathDataWithoutBezier.listDatas[pathDataWithoutBezier.listDatas.Count - 1].EndPoint.x, pathDataWithoutBezier.listDatas[pathDataWithoutBezier.listDatas.Count - 1].EndPoint.y,0),
                EndPoint = new Vector(pathDataWithoutBezier.listDatas[0].StartPoint.x, pathDataWithoutBezier.listDatas[0].StartPoint.y,0)
            });

            //计算最小包围矩形
            double rectLeft = 0, rectTop = 0, rectRight = 0, rectBottom = 0;
            GetOutRect(pathDataWithoutBezier.listDatas, out rectLeft, out rectTop, out rectRight, out rectBottom);

            //*************************************************************************
            //从矩形左上角开始向右下角方向绘制平行线，平行线角度= angle，平行线间距 = dis
            //*************************************************************************
            Vector v = new Vector();    //平行线的方向向量
            v.x = Math.Cos(angle / 180 * Math.PI);
            v.y = Math.Sin(angle / 180 * Math.PI);

            //计算矩形左上角到右下角的长度
            double diagonalLenght = Math.Sqrt(Math.Pow(rectRight - rectLeft, 2)+ Math.Pow(rectBottom - rectTop, 2));

            double currentOffset = 0;

            

            //生成过rect左上角的的直线
            Vector firstStartPoint = new Vector();
            Vector firstEndPoint = new Vector();
            firstStartPoint.x = rectLeft - v.x * diagonalLenght;
            firstStartPoint.y = rectTop - v.y * diagonalLenght;
            firstEndPoint.x = rectLeft + v.x * diagonalLenght;
            firstEndPoint.y = rectTop + v.y * diagonalLenght;

            //循环偏移直线
            while (currentOffset < diagonalLenght)
            {
                List<Vector> listIntersections = new List<Vector>();

                Vector startPoint = new Vector();
                Vector endPoint = new Vector();
                //生成平行线（从rect的左上角平移得到）
                LineSegmentOffset(currentOffset, firstEndPoint, firstStartPoint, ref startPoint, ref endPoint);

                //计算平行线与填充轮廓的交线
                foreach (var item in pathDataWithoutBezier.listDatas)
                {
                    if (item is PathItemLine)
                    {
                        Vector intersection = new Vector();
                        //计算直线段与直线的交点
                        bool find = LineSegmentIntersection(item.StartPoint, item.EndPoint - item.StartPoint, startPoint, endPoint- startPoint, ref intersection);
                        if(find == true)
                        {
                            if(listIntersections.Exists(t => Math.Sqrt(Math.Pow(intersection.x - t.x, 2) + Math.Pow(intersection.y - t.y, 2))<0.001) == false)
                            {
                                listIntersections.Add(intersection);
                            }
                        }
                    }
                    else if (item is PathItemArc)
                    {
                        var arcData = item as PathItemArc;
                        //计算arc 的圆心
                        //计算凸度
                        double bulge = Math.Tan(arcData.SweepAngle / 180 * Math.PI / 4);
                        //计算圆弧圆心
                        double centerX, centerY, radius, startAngle, sweepAngle;
                        TranslateArcFromBulge(arcData.StartPoint.x, arcData.StartPoint.y, arcData.EndPoint.x, arcData.EndPoint.y, bulge, out centerX, out centerY, out radius, out startAngle, out sweepAngle);


                        //计算直线段与圆弧的交点
                        var findPoints = ArcIntersectionWithLineSegment(new Vector(centerX,centerY,0),radius,startAngle,sweepAngle,startPoint, endPoint);
                        foreach (var itemPoint in findPoints)
                        {
                            if (listIntersections.Exists(t => Math.Sqrt(Math.Pow(itemPoint.x - t.x, 2) + Math.Pow(itemPoint.y - t.y, 2)) < 0.001) == false)
                            {
                                listIntersections.Add(itemPoint);
                            }

                        }
                    }
                }//计算平行线与填充轮廓的交线

                //直线与轮廓的交点 在直线上排序
                if (listIntersections.Count >= 2)
                {
                    IEnumerable<Vector> query = null;
                    query = from items in listIntersections orderby items.x select items;
                    var listOrderPoints = query.ToList();
                    //*********************************************
                    //默认认为1-2  3-4 5-6 为相交线段
                    // <<<<<<<<<<在这里没有考虑直线与轮廓顶点相交时产生的特例>>>>>>>>>>>
                    //*********************************************
                    for (int i = 0; i < listOrderPoints.Count() - 1; i = i + 2)
                    {
                        PathData newPath = new PathData();
                        PathItemLine newLine = new PathItemLine();
                        newLine.StartPoint.x = listOrderPoints[i].x;
                        newLine.StartPoint.y = listOrderPoints[i].y;
                        newLine.EndPoint.x = listOrderPoints[i + 1].x;
                        newLine.EndPoint.y = listOrderPoints[i + 1].y;
                        newPath.listDatas.Add(newLine);
                        listResult.Add(newPath);
                    }
                }


                currentOffset += dis;
            }//循环偏移直线

            return listResult;
        }

        /// <summary>
        /// 使用平行线填充封闭路径
        /// </summary>
        /// <param name="pathData">被填充的路径</param>
        /// <param name="dis">平行线间距</param>
        /// <param name="angle">平行线与X轴的角度</param>
        /// <returns></returns>
        public static List<PathData> FillPath_ByParallelLines(List<PathData> listPathData, double dis, double angle)
        {
            //申明结果数据
            List<PathData> listResult = new List<PathData>();

            if (listPathData.Count == 0)
            {
                return null;
            }

            //申明List，存放Bezier转换成直线段后的PathData
            List<PathData> listPathDataWithoutBezier = new List<PathData>();
            foreach (var itemPath in listPathData)
            {
                //将被填充路径中的Bezier转换成直线段
                var pathDataWithoutBezier = TransBezierToLines(itemPath, 1);

                //加入最后一条闭合线
                pathDataWithoutBezier.listDatas.Add(new PathItemLine()
                {
                    StartPoint = new Vector(pathDataWithoutBezier.listDatas[pathDataWithoutBezier.listDatas.Count - 1].EndPoint.x, pathDataWithoutBezier.listDatas[pathDataWithoutBezier.listDatas.Count - 1].EndPoint.y, 0),
                    EndPoint = new Vector(pathDataWithoutBezier.listDatas[0].StartPoint.x, pathDataWithoutBezier.listDatas[0].StartPoint.y, 0)
                });

                listPathDataWithoutBezier.Add(pathDataWithoutBezier);
            }

            //列出所有线段
            List<PathItemBase> outlines = new List<PathItemBase>();
            foreach (var itemPath in listPathDataWithoutBezier)
            {
                outlines.AddRange(itemPath.listDatas);
            }

            
            

            //计算最小包围矩形
            double rectLeft = 0, rectTop = 0, rectRight = 0, rectBottom = 0;
            GetOutRect(outlines, out rectLeft, out rectTop, out rectRight, out rectBottom);

            //*************************************************************************
            //从矩形左上角开始向右下角方向绘制平行线，平行线角度= angle，平行线间距 = dis
            //*************************************************************************
            Vector v = new Vector();    //平行线的方向向量
            v.x = Math.Cos(angle / 180 * Math.PI);
            v.y = Math.Sin(angle / 180 * Math.PI);

            //计算矩形左上角到右下角的长度
            double diagonalLenght = Math.Sqrt(Math.Pow(rectRight - rectLeft, 2) + Math.Pow(rectBottom - rectTop, 2));

            double currentOffset = 0;



            //生成过rect左上角的的直线
            Vector firstStartPoint = new Vector();
            Vector firstEndPoint = new Vector();

            if(angle>=0)
            firstStartPoint.x = rectLeft - v.x * diagonalLenght;
            firstStartPoint.y = rectTop - v.y * diagonalLenght;
            firstEndPoint.x = rectLeft + v.x * diagonalLenght;
            firstEndPoint.y = rectTop + v.y * diagonalLenght;

            //循环偏移直线
            while (currentOffset < diagonalLenght)
            {
                List<Vector> listIntersections = new List<Vector>();

                Vector startPoint = new Vector();
                Vector endPoint = new Vector();
                //生成平行线（从rect的左上角平移得到）
                LineSegmentOffset(currentOffset, firstEndPoint, firstStartPoint, ref startPoint, ref endPoint);

                //计算平行线与填充轮廓的交线
                foreach (var item in outlines)
                {
                    if (item is PathItemLine)
                    {
                        Vector intersection = new Vector();
                        //计算直线段与直线的交点
                        bool find = LineSegmentIntersection(item.StartPoint, item.EndPoint - item.StartPoint, startPoint, endPoint - startPoint, ref intersection);
                        if (find == true)
                        {
                            if (listIntersections.Exists(t => Math.Sqrt(Math.Pow(intersection.x - t.x, 2) + Math.Pow(intersection.y - t.y, 2)) < 0.001) == false)
                            {
                                listIntersections.Add(intersection);
                            }
                        }
                    }
                    else if (item is PathItemArc)
                    {
                        var arcData = item as PathItemArc;
                        //计算arc 的圆心
                        //计算凸度
                        double bulge = Math.Tan(arcData.SweepAngle / 180 * Math.PI / 4);
                        //计算圆弧圆心
                        double centerX, centerY, radius, startAngle, sweepAngle;
                        TranslateArcFromBulge(arcData.StartPoint.x, arcData.StartPoint.y, arcData.EndPoint.x, arcData.EndPoint.y, bulge, out centerX, out centerY, out radius, out startAngle, out sweepAngle);


                        //计算直线段与圆弧的交点
                        var findPoints = ArcIntersectionWithLineSegment(new Vector(centerX, centerY, 0), radius, startAngle, sweepAngle, startPoint, endPoint);
                        if(findPoints.Count ==1)
                        {
                            int a = 0;
                        }
                        foreach (var itemPoint in findPoints)
                        {
                            if (listIntersections.Exists(t => Math.Sqrt(Math.Pow(itemPoint.x - t.x, 2) + Math.Pow(itemPoint.y - t.y, 2)) < 0.001) == false)
                            {
                                listIntersections.Add(itemPoint);
                            }

                        }
                    }
                }//计算平行线与填充轮廓的交线

                //直线与轮廓的交点 在直线上排序
                if (listIntersections.Count >= 2)
                {
                    IEnumerable<Vector> query = null;
                    query = from items in listIntersections orderby items.x select items;
                    var listOrderPoints = query.ToList();
                    //*********************************************
                    //默认认为1-2  3-4 5-6 为相交线段
                    // <<<<<<<<<<在这里没有考虑直线与轮廓顶点相交时产生的特例>>>>>>>>>>>
                    //*********************************************
                    for (int i = 0; i < listOrderPoints.Count() - 1; i = i + 2)
                    {
                        PathData newPath = new PathData();
                        PathItemLine newLine = new PathItemLine();
                        newLine.StartPoint.x = listOrderPoints[i].x;
                        newLine.StartPoint.y = listOrderPoints[i].y;
                        newLine.EndPoint.x = listOrderPoints[i + 1].x;
                        newLine.EndPoint.y = listOrderPoints[i + 1].y;
                        newPath.listDatas.Add(newLine);
                        listResult.Add(newPath);
                    }
                }


                currentOffset += dis;
            }//循环偏移直线

            return listResult;
        }


        //偏移指定距离
        public static void  LineSegmentOffset(double dis,Vector lineStart,Vector lineEnd,ref Vector resultStart,ref Vector resultEnd)
        {
            //****************************************************************************************
            //计算直线的法向量
            //****************************************************************************************
            //从起点指向终点的向量
            Vector vectorStartToEnd = new Vector(lineEnd.x - lineStart.x, lineEnd.y - lineStart.y, 0);
            //Z轴向量
            Vector vectorZ = new Vector(0, 0, 1);

            //建立坐标系vectorStartToEnd为X轴，vectorZ为Z轴，vectorStartToEnd和vectorZ的叉积位Y轴
            //vectorStartToEnd 和 vectorZ的叉积即为
            Vector vectorY = Vector.CrossProduct(vectorZ, vectorStartToEnd.Normalize());


            //****************************************************************************************
            //计算起点和终点的偏移
            //****************************************************************************************
            Vector newPointStart = new Vector();
            Vector newPointEnd = new Vector();
            resultStart.x = lineStart.x + dis * vectorY.x;
            resultStart.y = lineStart.y + dis * vectorY.y;
            resultEnd.x = lineEnd.x + dis * vectorY.x;
            resultEnd.y = lineEnd.y + dis * vectorY.y;

        }

        /// <summary>
        /// 计算两条线段的交点
        /// </summary>
        /// <param name="P0">第一条线段起点</param>
        /// <param name="D0">第一条线段终点</param>
        /// <param name="P1">第二条线段起点</param>
        /// <param name="D1">第二条线段终点</param>
        /// <param name="intersection">计算出的交点</param>
        /// <returns>返回是否有交点</returns>
        public static bool LineSegmentIntersection(Vector P0,Vector D0,Vector P1,Vector D1,ref Vector intersection)
        {
            Vector E = P1 - P0;
            double kross = D0.x * D1.y - D0.y * D1.x;
            double sqrKross = kross * kross;
            double sqrLen0 = D0.x * D0.x + D0.y * D0.y;
            double sqrLen1 = D1.x * D1.x + D1.y * D1.y;
            double sqrEpsilon = 0.000000000001;
            //判断是否平行
            if (sqrKross > sqrEpsilon * sqrLen0 * sqrLen1)
            {
                // lines of the segments are not parallel
                double s = (E.x * D1.y - E.y * D1.x) / kross;
                if (s < 0 || s > 1)
                {
                    // intersection of lines is not a point on segment P0 + s * D0
                    return false;
                }
                double t = (E.x * D0.y - E.y * D0.x) / kross;
                if (t < 0 || t > 1) 
                {
                    // intersection of lines is not a point on segment P1 + t * D1
                    return false;
                }
                // intersection of lines is a point on each segment
                intersection = P0 + s * D0;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
		/// 计算点到直线的垂直距离
		/// 直线Ax+By+c = 0，则点到直线距离为|Ax+By+c|/Sqrt(A^2+B^2)
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public static double DisFromPointToLine(Vector point,Vector linePoint0,Vector linePoint1)
        {
            //获得直线的参数
            double a = 0, b = 0, c = 0;
            GetLineParam(linePoint0, linePoint1,ref a, ref b, ref c);
            double dis = Math.Abs(a * point.x + b * point.y + c) / Math.Sqrt(a * a + b * b);
            return dis;
        }

        /// <summary>
        /// 计算直线段的参数
        /// </summary>
        /// <param name="linePoint0"></param>
        /// <param name="linePoint1"></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static void GetLineParam(Vector startPoint, Vector endPoint, ref double a, ref double b, ref double c)
        {
            //直线是否平行于X轴,Y + C = 0 
            if (startPoint.y == endPoint.y)
            {
                //平行X轴
                a = 0;
                b = 1;
                c = startPoint.y * -1;

                return;
            }
            else
            {
                //不平行X轴，则a不等于0，设a = 1
                a = 1;
                b = (startPoint.x - endPoint.x) / (endPoint.y - startPoint.y);
                c = -(startPoint.x + b * startPoint.y);
            }
        }

        /// <summary>
        /// 计算圆弧与线段的交点
        /// </summary>
        /// <param name="P">直线段起点</param>
        /// <param name="D">直线段终点</param>
        /// <returns>返回的交点List</returns>
        public  static List<Vector> ArcIntersectionWithLineSegment(Vector center,double radius,double startAngle,double sweepAngle,Vector lineStart, Vector lineEnd)
        {
            if(startAngle<0)
            {
                startAngle += 360;
            }

            List<Vector> intersctPoints = new List<Vector>();
            //计算圆心到线段的距离
            double dis = DisFromPointToLine(center,lineStart, lineEnd);
            //如果dis=radius则没有交点
            if (dis > radius)
            {
                return intersctPoints;
            }

            //获得直线的参数
            double a = 0, b = 0, c = 0;
            GetLineParam(lineStart, lineEnd,ref a, ref b, ref c);

            //计算直线和圆的交点，直线与圆由两个交点，若相切则两交点重合
            //直线公式Ax+By+C=0,圆公式(X-xc)^2+(Y-yc)^2 = r^2
            if (b == 0)
            {
                double x = -c / a;
                double temp = Math.Sqrt(radius * radius - (x - center.x) * (x - center.x));
                double y0 = center.y + temp;
                double y1 = center.y - temp;
                if (Math.Abs(y0 - y1) < 0.00000001)
                {
                    intersctPoints.Add(new Vector(x, y0,0));
                }
                else
                {
                    intersctPoints.Add(new Vector(x, y0,0));
                    intersctPoints.Add(new Vector(x, y1,0));
                }
            }
            else
            {
                double x0, x1, y0, y1;
                double cx = center.x;
                double cy = center.y;
                double r = radius;
                x0 = (-a * b * cy - a * c + b * b * cx - b * Math.Sqrt(-a * a * cx * cx + a * a * r * r - 2 * a * b * cx * cy - 2 * a * c * cx - b * b * cy * cy + b * b * r * r - 2 * b * c * cy - c * c)) / (a * a + b * b);
                y0 = (a * a * cy - a * b * cx + a * Math.Sqrt(-a * a * cx * cx + a * a * r * r - 2 * a * b * cx * cy - 2 * a * c * cx - b * b * cy * cy + b * b * r * r - 2 * b * c * cy - c * c) - b * c) / (a * a + b * b);
                x1 = (-a * b * cy - a * c + b * b * cx + b * Math.Sqrt(-a * a * cx * cx + a * a * r * r - 2 * a * b * cx * cy - 2 * a * c * cx - b * b * cy * cy + b * b * r * r - 2 * b * c * cy - c * c)) / (a * a + b * b);
                y1 = (a * a * cy - a * b * cx - a * Math.Sqrt(-a * a * cx * cx + a * a * r * r - 2 * a * b * cx * cy - 2 * a * c * cx - b * b * cy * cy + b * b * r * r - 2 * b * c * cy - c * c) - b * c) / (a * a + b * b);

                //判断两点的间距
                double ptpDis = Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1));
                if (ptpDis < 0.00000001)
                {
                    intersctPoints.Add(new Vector(x0, y0,0));
                }
                else
                {
                    intersctPoints.Add(new Vector(x0, y0,0));
                    intersctPoints.Add(new Vector(x1, y1,0));
                }
            }

            //计算点是否在线段上
            for (int i = intersctPoints.Count - 1; i >= 0; i--)
            {
                Vector point = intersctPoints[i];
                if ((point.x - lineStart.x) * (point.x - lineEnd.x) > 0
                   || (point.y - lineStart.y) * (point.y - lineEnd.y) > 0)
                {
                    intersctPoints.RemoveAt(i);
                }
            }

            //计算点是否在圆弧范围内
            for (int i = intersctPoints.Count - 1; i >= 0; i--)
            {
                Vector point = intersctPoints[i];
                //计算点的角度
                Vector vectorCenterToStart = new Vector(point.x - center.x, point.y - center.y, 0);
                double currentAngle = vectorCenterToStart.AngleToX();
                if(currentAngle<0)
                {
                    currentAngle += 360;
                }
                //判断角度是否在弧度范围内
                if ((currentAngle >= startAngle && currentAngle <= startAngle + sweepAngle)
                            || (currentAngle + 360 >= startAngle && currentAngle + 360 <= startAngle + sweepAngle))
                {

                }
                else
                {
                    intersctPoints.RemoveAt(i);
                }
            }

            return intersctPoints;
        }

        /// <summary>
        /// 计算路径pathData的最小外接矩形
        /// </summary>
        /// <param name="pathData">输入路径</param>
        /// <param name="left">外接矩形Left</param>
        /// <param name="top">外接矩形Top</param>
        /// <param name="right">外接矩形Right</param>
        /// <param name="bottom">外接矩形Bottom</param>
        public static void GetOutRect(List<PathItemBase> pathData, out double left, out double top, out double right, out double bottom)
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var item in pathData)
            {
                if(item.StartPoint.x>maxX)
                {
                    maxX = item.StartPoint.x;
                }
                if(item.StartPoint.x<minX)
                {
                    minX = item.StartPoint.x;
                }
                if (item.StartPoint.y > maxY)
                {
                    maxY = item.StartPoint.y;
                }
                if (item.StartPoint.y < minY)
                {
                    minY = item.StartPoint.y;
                }

                if(item.EndPoint.x > maxX)
                {
                    maxX = item.EndPoint.x;
                }
                if (item.EndPoint.x < minX)
                {
                    minX = item.EndPoint.x;
                }
                if (item.EndPoint.y > maxY)
                {
                    maxY = item.EndPoint.y;
                }
                if (item.EndPoint.y < minY)
                {
                    minY = item.EndPoint.y;
                }
            }

            left = minX;
            right = maxX;
            bottom = minY;
            top = maxY;
        }


        /// <summary>
        /// 将Path中的Bezier曲线转换成多段线
        /// </summary>
        /// <param name="pathData">被转换的Path</param>
        /// <param name="subLineLength">Bezier分段使用的直线长度</param>
        /// <returns></returns>
        public static PathData TransBezierToLines(PathData pathData,double subLineLength)
        {
            PathData result = new PathData();
            result.isClosed = pathData.isClosed;

            foreach (var item in pathData.listDatas)
            {
                if(item is PathItemBezier == false)
                {
                    result.listDatas.Add(item);
                }
                else
                {
                    var bezier = item as PathItemBezier;
                    var lines = TransBezierToLines(bezier, subLineLength);
                    result.listDatas.AddRange(lines.listDatas);
                }
            }

            return result;
        }

        /// <summary>
        /// 将Bezier曲线转换成多段线
        /// 计算步骤：
        ///     1. 将t:[0-1]细分，并计算每一段细分的长度
        ///     2. 积分得到Bezier曲线的总长
        ///     3. 记录每段多段线分割点出的t值
        /// 3次Bezier曲线公式：
        ///     C(t) = (1-t)^3*P0 + 3t(1-t)^2*P1 + 3t^2(1-t)*P2+t^3*P3
        /// </summary>
        /// <param name="bezier">被转换的Bezier</param>
        /// <param name="subLineLength">Bezier分段使用的直线长度</param>
        /// <returns></returns>
        public static PathData TransBezierToLines(PathItemBezier bezier,double subLineLength)
        {
            //结果
            PathData result = new PathData();
            //t:[0-1]细分
            double divisionInT = 0.001;
            //t
            double t = 0;
            //
            double P0_x = bezier.StartPoint.x;
            double P0_y = bezier.StartPoint.y;
            double P1_x = bezier.ControlPoint1.x;
            double P1_y = bezier.ControlPoint1.y;
            double P2_x = bezier.ControlPoint2.x;
            double P2_y = bezier.ControlPoint2.y;
            double P3_x = bezier.EndPoint.x;
            double P3_y = bezier.EndPoint.y;
            //记录积分的最后一个点坐标
            double lastPointIntegration_x = P0_x;
            double lastPointIntegration_y = P0_y;
            //积分路径长度
            double sumLength = 0;

            //路径积分
            while (t<1)
            {
                double posX = Math.Pow(1 - t, 3) * P0_x + 3 * t * Math.Pow(1 - t, 2) * P1_x + 3 * t * t * (1 - t) * P2_x + Math.Pow(t, 3) * P3_x;
                double posY = Math.Pow(1 - t, 3) * P0_y + 3 * t * Math.Pow(1 - t, 2) * P1_y + 3 * t * t * (1 - t) * P2_y + Math.Pow(t, 3) * P3_y;
                //当前积分段长度
                double currentDivisionLength = 0;
                //计算当前积分段长度
                currentDivisionLength = Math.Sqrt(Math.Pow(posX - lastPointIntegration_x, 2) + Math.Pow(posY - lastPointIntegration_y,2));
                //累加长度
                sumLength += currentDivisionLength;

                //判断当前累计长度是否到达多段线转换分割点
                double currenSubLineLength = (result.listDatas.Count + 1) * subLineLength;
                if ((sumLength - currentDivisionLength < currenSubLineLength) && (sumLength >= currenSubLineLength))
                {
                    //添加新分割
                    if(result.listDatas.Count == 0)
                    {
                        var newLine = new PathItemLine();
                        newLine.StartPoint.x = P0_x;
                        newLine.StartPoint.y = P0_y;
                        newLine.EndPoint.x = posX;
                        newLine.EndPoint.y = posY;
                        result.listDatas.Add(newLine);
                    }
                    else
                    {
                        var newLine = new PathItemLine();
                        newLine.StartPoint.x = result.listDatas[result.listDatas.Count - 1].EndPoint.x;
                        newLine.StartPoint.y = result.listDatas[result.listDatas.Count - 1].EndPoint.y;
                        newLine.EndPoint.x = posX;
                        newLine.EndPoint.y = posY;
                        result.listDatas.Add(newLine);
                    }
                }

                //记录积分最后点
                lastPointIntegration_x = posX;
                lastPointIntegration_y = posY;

                //t累加
                t += divisionInT;
            }

            //添加最后一段线
            if (result.listDatas.Count == 0)
            {
                var newLine = new PathItemLine();
                newLine.StartPoint.x = P0_x;
                newLine.StartPoint.y = P0_y;
                newLine.EndPoint.x = P3_x;
                newLine.EndPoint.y = P3_y;
                result.listDatas.Add(newLine);
            }
            else
            {
                if(result.listDatas[result.listDatas.Count - 1].EndPoint.x != P3_x || result.listDatas[result.listDatas.Count - 1].EndPoint.x != P3_y)
                {
                    var newLine = new PathItemLine();
                    newLine.StartPoint.x = result.listDatas[result.listDatas.Count - 1].EndPoint.x;
                    newLine.StartPoint.y = result.listDatas[result.listDatas.Count - 1].EndPoint.y;
                    newLine.EndPoint.x = P3_x;
                    newLine.EndPoint.y = P3_y;
                    result.listDatas.Add(newLine);
                }
            }


            return result;
        }

        //转换通过《起点、终点、凸度》定义的圆弧
        // b = 0.5*(1/bulge - bulge)
        // centerX = 0.5*((x1+x2)-b*(y2-y1))
        // centerY = 0.5*((y1+y2)+b*(x2-x1))
        public static void TranslateArcFromBulge(double startX, double startY, double endX, double endY, double bulge, out double centerX, out double centerY, out double radius, out double startAngle, out double sweepAngle)
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
}
