using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocViewerDemo.DrawEntity
{
    public class DrawEntity_Polyline:DrawEntity
    {

        List<PolylineVertex> controlVectexex = new List<PolylineVertex>();
        bool closed = false;

        Color color = Color.White;
        int width = 1;

        public DrawEntity_Polyline(double[] x,double[] y,double[] bulge,bool closed,Color color,int width = 1)
        {
            for (int i = 0; i < x.Length; i++)
            {
                controlVectexex.Add(new PolylineVertex(x[i], y[i], bulge[i]));
            }
            this.closed = closed;
            this.color = color;
            this.width = width;
        }
        
		public override object Clone()
		{
			double[] x =new double[controlVectexex.Count];
			double[] y =new double[controlVectexex.Count];
			double[] bulge =new double[controlVectexex.Count];
			for (int i = 0; i < controlVectexex.Count; i++)
			{
				x[i] = controlVectexex[i].x;
				y[i] = controlVectexex[i].y;
				bulge[i] = controlVectexex[i].bulge;
			}
			return new DrawEntity_Polyline(x,y,bulge,closed,color,width);
		}

        public override void Draw()
        {
        	Color lineColor = color;
        	if(status_selected == false)
        	{
        		lineColor = color;
        	}
        	else
        	{
        		lineColor = Color.Red;
        	}
        	
            if (controlVectexex.Count < 2) return;

            for (int i = 1; i < controlVectexex.Count; i++)
            {
                if (Math.Abs(controlVectexex[i - 1].bulge) < 0.001)
                {
                	//绘制直线
                	DrawLine(new Vector(controlVectexex[i - 1].x, controlVectexex[i - 1].y,0), new Vector(controlVectexex[i].x, controlVectexex[i].y, 0),width, lineColor);
                }
                else
                {
                    //计算圆弧
                    double centerX, centerY, radius,startAngle, sweepAngle;
                    TranslateArcFromBulge(controlVectexex[i - 1].x, controlVectexex[i - 1].y, controlVectexex[i].x, controlVectexex[i].y, controlVectexex[i-1].bulge,out centerX, out centerY,out radius, out startAngle, out sweepAngle);
                	//绘制圆弧
                	DrawArc(new Vector(centerX, centerY, 0),radius ,startAngle, sweepAngle, width, lineColor);
                }
            }

            //是否闭合
            if(closed)
            {
                int count = controlVectexex.Count;
            	//添加闭合直线
            	DrawLine(new Vector(controlVectexex[0].x, controlVectexex[0].y, 0), new Vector(controlVectexex[count - 1].x, controlVectexex[count - 1].y, 0), width, lineColor);
            }
            
            //需要显示操作框
        	if(status_showHandle == true)
        	{
        		//绘制边缘操作框
        		foreach (var point in controlVectexex) 
        		{
        			this.DrawHandleRect(new Vector(point.x,point.y,0),handelRectNormalColor,handelRectBorderColor,handleRectSize);
        		}
        		
        	}
            
        }

        //转换通过《起点、终点、凸度》定义的圆弧
        // b = 0.5*(1/bulge - bulge)
        // centerX = 0.5*((x1+x2)-b*(y2-y1))
        // centerY = 0.5*((y1+y2)+b*(x2-x1))
        public void TranslateArcFromBulge(double startX,double startY,double endX,double endY,double bulge,out double centerX, out double centerY,out double radius,out double startAngle,out double sweepAngle)
        {
            double b = 0.5 * (1 / bulge - bulge);
            //计算圆心
            centerX = 0.5 * ((startX + endX) - b * (endY - startY));
            centerY = 0.5 * ((startY + endY) + b * (endX - startX));
            //计算圆心
            radius = Math.Sqrt(Math.Pow(centerX - startX, 2)+Math.Pow(centerY - startY,2));

            //计算起始角
            Vector vectorCentorToStart = new Vector(startX - centerX, startY - centerY, 0);
            startAngle = Math.Atan2(vectorCentorToStart.y/radius, vectorCentorToStart.x/radius) / Math.PI * 180;
  
            //计算终止角
            Vector vectorCentorToEnd = new Vector(endX - centerX, endY - centerY, 0);
            double endAngle = Math.Atan2(vectorCentorToEnd.y/radius, vectorCentorToEnd.x/radius) / Math.PI * 180;

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

        //多段线控制点
        public class PolylineVertex
        {
            public double x = 0;
            public double y = 0;
            public double bulge = 0;

            public PolylineVertex(double x,double y,double bulge)
            {
                this.x = x;
                this.y = y;
                this.bulge = bulge;
            }
        }
    }//class
}//namespace
