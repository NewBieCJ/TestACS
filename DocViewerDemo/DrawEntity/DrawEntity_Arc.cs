using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocViewerDemo.DrawEntity
{
    public class DrawEntity_Arc:DrawEntity
    {
        Vector center = new Vector();
        double radius = 0;
        double startAngle = 0;
        double sweepAngle = 90;
        Color color = Color.White;
        int width = 1;

        public DrawEntity_Arc(double centerX,double centerY,double radius,double startAngle,double sweepAngle, Color color, int width = 1)
        {
            center.x = centerX;
            center.y = centerY;
            this.radius = radius;
            this.startAngle = startAngle;
            this.sweepAngle = sweepAngle;
            this.width = width;
            this.color = color;
        }

		public override object Clone()
		{
			return new DrawEntity_Arc(center.x,center.y,radius,startAngle,sweepAngle,color,width);
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
        	
            this.DrawArc(center, radius,startAngle,sweepAngle, width, lineColor);
            
            //需要显示操作框
        	if(status_showHandle == true)
        	{
        		double startPointX = center.x + radius*Math.Cos(startAngle/180*Math.PI);
        		double startPointy = center.y + radius*Math.Sin(startAngle/180*Math.PI);
        		
        		double endPointX = center.x + radius*Math.Cos((startAngle + sweepAngle)/180*Math.PI);
        		double endPointy = center.y + radius*Math.Sin((startAngle + sweepAngle)/180*Math.PI);
        		
        		double middlePointX = center.x + radius*Math.Cos((startAngle + sweepAngle/2)/180*Math.PI);
        		double middlePointY = center.y + radius*Math.Sin((startAngle + sweepAngle/2)/180*Math.PI);
        		
        		//绘制端点
        		this.DrawHandleRect(new Vector(startPointX,startPointy,0),handelRectNormalColor,handelRectBorderColor,handleRectSize);
        		this.DrawHandleRect(new Vector(endPointX,endPointy,0),handelRectNormalColor,handelRectBorderColor,handleRectSize);
        		this.DrawHandleRect(new Vector(middlePointX,middlePointY,0),handelRectNormalColor,handelRectBorderColor,handleRectSize);
        		
        		//绘制中心操作框
        		this.DrawHandleRect(center,handelRectNormalColor,handelRectBorderColor,handleRectSize);
        	}
        }
        
        
        //是否在矩形框内
        public override bool IsInRect(Vector leftTop,Vector rightBottom)
        {
        	//计算border
        	double borderLeft = 0;
        	double borderTop = 0;
        	double borderRight = 0;
        	double borderBottom = 0;
        	
        	GetOutRect(ref borderLeft,ref borderTop,ref borderRight,ref borderBottom);
        	
        	
        	if(borderLeft> leftTop.x && borderLeft<rightBottom.x 
        	   && borderTop < leftTop.y && borderTop> rightBottom.y
        	   && borderRight> leftTop.x && borderRight<rightBottom.x 
        	   && borderBottom < leftTop.y && borderBottom> rightBottom.y)
        	{
        		return true;
        	}
        	else
        	{
        		return false;
        	}
        }
        
        //是否与矩形框交集
         public override bool IsIntersectionWithRect(Vector leftTop,Vector rightBottom)
         {
         	Vector rightTop =  new Vector(rightBottom.x,leftTop.y,0);
         	Vector leftBottom = new Vector(leftTop.x,rightBottom.y,0);
         	
         	bool intersectWithLeft = Hatch.PathHatch.ArcIntersectionWithLineSegment(center,radius,startAngle,sweepAngle,leftTop,leftBottom).Count>0?true:false;
			bool intersectWithTop =  Hatch.PathHatch.ArcIntersectionWithLineSegment(center,radius,startAngle,sweepAngle,leftTop,rightTop).Count>0?true:false;
			bool intersectWithRight = Hatch.PathHatch.ArcIntersectionWithLineSegment(center,radius,startAngle,sweepAngle,rightTop,rightBottom).Count>0?true:false;
			bool intersectWithBottom =  Hatch.PathHatch.ArcIntersectionWithLineSegment(center,radius,startAngle,sweepAngle,leftBottom,rightBottom).Count>0?true:false;
			
			if(intersectWithLeft || intersectWithTop || intersectWithRight || intersectWithBottom)
		    {
				return true;
		    }
			else
			{
				return false;
			}
         }
         
         
        //获得所有夹持点
        public override  Vector[] GetAllGripPoint()
        {
        	Vector pointStart  = new Vector();
        	Vector pointEnd = new Vector();
        	Vector pointMiddle = new Vector();
        	
        	pointStart.x = center.x + radius*Math.Cos(startAngle/180*Math.PI);
    		pointStart.y = center.y + radius*Math.Sin(startAngle/180*Math.PI);
    		
    		pointEnd.x = center.x + radius*Math.Cos((startAngle + sweepAngle)/180*Math.PI);
    		pointEnd.y = center.y + radius*Math.Sin((startAngle + sweepAngle)/180*Math.PI);
    		
    		pointMiddle.x = center.x + radius*Math.Cos((startAngle + sweepAngle/2)/180*Math.PI);
    		pointMiddle.y = center.y + radius*Math.Sin((startAngle + sweepAngle/2)/180*Math.PI);
        	
        	return new Vector[4]{pointStart,pointEnd,pointMiddle,center};
        }
        
        //获得夹持点坐标
        public override  Vector GetGripPoint(int index)
        {
        	Vector pointStart  = new Vector();
        	Vector pointEnd = new Vector();
        	Vector pointMiddle = new Vector();
        	
        	pointStart.x = center.x + radius*Math.Cos(startAngle/180*Math.PI);
    		pointStart.y = center.y + radius*Math.Sin(startAngle/180*Math.PI);
    		
    		pointEnd.x = center.x + radius*Math.Cos((startAngle + sweepAngle)/180*Math.PI);
    		pointEnd.y = center.y + radius*Math.Sin((startAngle + sweepAngle)/180*Math.PI);
    		
    		pointMiddle.x = center.x + radius*Math.Cos((startAngle + sweepAngle/2)/180*Math.PI);
    		pointMiddle.y = center.y + radius*Math.Sin((startAngle + sweepAngle/2)/180*Math.PI);
    		
        	switch (index)
        	{
        		case 0:
        			return pointStart;
        			break;
        		case 1:
        			return pointEnd;
        			break;
        		case 2:
        			return pointMiddle;
        			break;
        		case 3:
        			return center;
        			break;
        		default:
        			return null;
        	}
        }
        //设置当前夹持点
        public override  void SetCurrentGripPointIndex(int index)
        {
        	gripCurentIndex = index;
        }
        //设置指定夹持点坐标
        public override  void SetGripPoint(int index,Vector pos)
        {
        	Vector pointStart  = new Vector();
        	Vector pointEnd = new Vector();
        	Vector pointMiddle = new Vector();
        	
        	pointStart.x = center.x + radius*Math.Cos(startAngle/180*Math.PI);
    		pointStart.y = center.y + radius*Math.Sin(startAngle/180*Math.PI);
    		
    		pointEnd.x = center.x + radius*Math.Cos((startAngle + sweepAngle)/180*Math.PI);
    		pointEnd.y = center.y + radius*Math.Sin((startAngle + sweepAngle)/180*Math.PI);
    		
    		pointMiddle.x = center.x + radius*Math.Cos((startAngle + sweepAngle/2)/180*Math.PI);
    		pointMiddle.y = center.y + radius*Math.Sin((startAngle + sweepAngle/2)/180*Math.PI);
    		
    		double newCenterX = 0,newCenterY = 0,newRadius = 0,newStartAngle = 0,newSweepAngle = 0;
    		
        	switch (index)
        	{
        		case 0:
        			DrawEntity_Arc.CreateArcByThreePoint(pos,pointMiddle,pointEnd,ref newCenterX,ref newCenterY,ref newRadius,ref newStartAngle,ref newSweepAngle);
        			this.center.x = newCenterX;
        			this.center.y = newCenterY;
        			this.radius = newRadius;
        			this.startAngle = newStartAngle;
        			this.sweepAngle = newSweepAngle;
        			break;
        		case 1:
        			DrawEntity_Arc.CreateArcByThreePoint(pointStart,pointMiddle,pos,ref newCenterX,ref newCenterY,ref newRadius,ref newStartAngle,ref newSweepAngle);
        			this.center.x = newCenterX;
        			this.center.y = newCenterY;
        			this.radius = newRadius;
        			this.startAngle = newStartAngle;
        			this.sweepAngle = newSweepAngle;
        			break;
        		case 2:
        			DrawEntity_Arc.CreateArcByThreePoint(pointStart,pos,pointEnd,ref newCenterX,ref newCenterY,ref newRadius,ref newStartAngle,ref newSweepAngle);
        			this.center.x = newCenterX;
        			this.center.y = newCenterY;
        			this.radius = newRadius;
        			this.startAngle = newStartAngle;
        			this.sweepAngle = newSweepAngle;
        			break;
				case 3:
        			center.x = pos.x;
        			center.y = pos.y;
        			break;
        		default:
        			break;
        	}
        }
        
        //获取鼠标在元素上的捕捉点
		public override DocViewerDemo.Snap.SnappedPoint GetSnapPoint(double mouseX, double mouseY, double snapWindowSize)
		{
			//判断鼠标点是否在元素的最小外接矩形外扩snapWindowSize/2的范围内
			double left = 0,top = 0,right = 0,bottom = 0;
			this.GetOutRect(ref left,ref top,ref right,ref bottom);
			left -= snapWindowSize/2;
			top += snapWindowSize/2;
			right += snapWindowSize/2;
			bottom -= snapWindowSize/2;
			
			if(mouseX >= left && mouseX<=right && mouseY<= top && mouseY >= bottom)
			{
				
			}
			else
			{
				return null;
			}
			
			//判断是否在端点附近
			if(Math.Abs(mouseX - StartPoint.x)<snapWindowSize/2 && Math.Abs(mouseY - StartPoint.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(StartPoint, Snap.SnapEnum.Snap_EndPoint);
			}
			
			if(Math.Abs(mouseX - EndPoint.x)<snapWindowSize/2 && Math.Abs(mouseY - EndPoint.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(EndPoint, Snap.SnapEnum.Snap_EndPoint);
			}
			
			
			//判断是否在中点附近
			if(Math.Abs(mouseX - MiddlePoint.x)<snapWindowSize/2 && Math.Abs(mouseY - MiddlePoint.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(MiddlePoint, Snap.SnapEnum.Snap_MiddlePoint);
			}
			
			//判断是否在圆心附近
			if(Math.Abs(mouseX - center.x)<snapWindowSize/2 && Math.Abs(mouseY - center.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(center, Snap.SnapEnum.Snap_Center);
			}
			
			//计算离元素最近点
			
			
			
			return null;
		}
   
        
        //获得圆弧起始点
		public Vector StartPoint
		{
			get
			{
				double x = center.x + radius * Math.Cos(startAngle / 180 * Math.PI);
				double y = center.y + radius * Math.Sin(startAngle / 180 * Math.PI);
				return new Vector(x, y,0);
			}
		}

		//获得圆弧终点
		public Vector EndPoint
		{
			get
			{
				double x = center.x + radius * Math.Cos((startAngle + sweepAngle) / 180 * Math.PI);
				double y = center.y + radius * Math.Sin((startAngle + sweepAngle) / 180 * Math.PI);
				return new Vector(x, y,0);
			}
		}
		
		//获取圆弧中点
		public Vector MiddlePoint
		{
			get
			{
				double x = center.x + radius * Math.Cos((startAngle + sweepAngle/2) / 180 * Math.PI);
				double y = center.y + radius * Math.Sin((startAngle + sweepAngle/2) / 180 * Math.PI);
				return new Vector(x, y,0);
			}
		}
		
		/// <summary>
		/// 已知圆上三点，求圆
		/// http://www.cnblogs.com/xpvincent/p/8266734.html
		/// 三点坐标(x1,y1),(x2,y2),(x3,y3)
		/// 圆方程:Ax2+Ay2+Bx+Cy+D = 0;
		/// A = x1*(y2-y3) - y1*(x2-x3) + x2*y3 - x3*y2
		/// B = (x1*x1 + y1*y1)(y3-y2) + (x2*x2+y2*y2)(y1-y3)+(x3*x3+y3*y3)(y2-y1)
		/// C = (x1*x1 + y1*y1)(x2-x3) + (x2*x2+y2*y2)(x3-x1)+(x3*x3+y3*y3)(x1-x2)
		/// D = (x1*x1 + y1*y1)(x3*y2-x2*y3) + (x2*x2+y2*y2)(x1*y3-x3*y1)+(x3*x3+y3*y3)(x2*y1-x1*y2)
		/// centerX = -B/2/A
		/// centerY = -C/2/A
		/// r = sqrt((B*B + C*C - 4*A*D)/4/A/A)
		/// </summary>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <param name="point3"></param>
		/// <param name="isAntiClock"></param>
		/// <param name="centerX"></param>
		/// <param name="centerY"></param>
		/// <param name="radius"></param>
		/// <param name="startAngle"></param>
		/// <param name="sweepAngle"></param>
		public static void CreateArcByThreePoint(Vector point1, Vector point2, Vector point3,ref double centerX,ref double centerY,ref double radius,ref double startAngle,ref double sweepAngle)
		{
			double x1,y1,x2,y2,x3,y3;
			double A,B,C,D;
			x1 = point1.x;
			y1 = point1.y;
			x2 = point2.x;
			y2 = point2.y;
			x3 = point3.x;
			y3 = point3.y;
			A = x1*(y2-y3) - y1*(x2-x3) + x2*y3 - x3*y2;
			B = (x1*x1 + y1*y1)*(y3-y2) + (x2*x2+y2*y2)*(y1-y3)+(x3*x3+y3*y3)*(y2-y1);
			C = (x1*x1 + y1*y1)*(x2-x3) + (x2*x2+y2*y2)*(x3-x1)+(x3*x3+y3*y3)*(x1-x2);
			D = (x1*x1 + y1*y1)*(x3*y2-x2*y3) + (x2*x2+y2*y2)*(x1*y3-x3*y1)+(x3*x3+y3*y3)*(x2*y1-x1*y2);
				
			centerX = -B/2/A;
			centerY = -C/2/A;
			radius = Math.Sqrt((B*B + C*C - 4*A*D)/4/A/A);
			
			//计算起始角
			startAngle = Math.Atan2(point1.y - centerY,point1.x - centerX)/Math.PI*180;
			
			//计算终止角
			double endAngle = Math.Atan2(point3.y - centerY,point3.x - centerX)/Math.PI*180;
			
			//计算中间点角度
			double middleAngle = Math.Atan2(point2.y - centerY,point2.x - centerX)/Math.PI*180;
			
			
			
			//计算扫描角的绝对值
			double sweepAngleAbs = endAngle - startAngle;
			while(sweepAngleAbs>360)
			{
				sweepAngleAbs -= 360;
			}
			while(sweepAngleAbs<0)
			{
				sweepAngleAbs += 360;
			}
			
			
			
			//假设为逆时针
			double tempEndAngle = endAngle;
			if(tempEndAngle<startAngle)
			{
				tempEndAngle += 360;
			}
			double tempMiddleAngle = middleAngle;
			if(tempMiddleAngle<startAngle)
			{
				tempMiddleAngle += 360;
			}
			if(tempMiddleAngle<tempEndAngle)
			{
				//确认是逆时针
				sweepAngle = sweepAngleAbs;
			}
			else
			{
				//假设否定，是顺时针
				sweepAngle = -(360-sweepAngleAbs);
			}
			
		}
			
        
        /// <summary>
		/// 计算最小包围矩形
		/// </summary>
		/// <returns>返回矩形</returns>
		public void GetOutRect(ref double left,ref double top,ref double right,ref double bottom)
		{
			//圆弧可能的在X轴Y轴上的最大、最小点
			//起点、终点、0度、90度、180度、270度
			Vector[] points = new Vector[6];
			//6个关键点都初始化为起点
			Vector pStart = this.StartPoint;
			for (int i = 0; i < 6; i++)
			{
				points[i] = new Vector();
				points[i].x = pStart.x;
				points[i].y = pStart.y;
			}
			//起点赋值
			//终点赋值
			points[1].x = this.EndPoint.x;
			points[1].y = this.EndPoint.y;
			//0度赋值
			if (IsAngleInArc(0))
			{
				points[2].x = this.center.x + radius;
				points[2].y = this.center.y;
			}
			//90度赋值
			if (IsAngleInArc(90))
			{
				points[3].x = this.center.x;
				points[3].y = this.center.y + radius;
			}
			//180度赋值
			if (IsAngleInArc(180))
			{
				points[4].x = this.center.x - radius;
				points[4].y = this.center.y;
			}
			//270度赋值
			if (IsAngleInArc(270))
			{
				points[5].x = this.center.x;
				points[5].y = this.center.y - radius;
			}
			//找最大X，找最小X，找最大Y，找最小Y
			double minX = 0, maxX = 0, minY = 0, maxY = 0;
			minX = points[0].x;
			maxX = points[0].x;
			minY = points[0].y;
			maxY = points[0].y;
			foreach (var element in points)
			{
				if (element.x < minX)
					minX = element.x;
				if (element.x > maxX)
					maxX = element.x;
				if (element.y < minY)
					minY = element.y;
				if (element.y > maxY)
					maxY = element.y;
			}
			left = minX;
			top = maxY;
			right = maxX;
			bottom = minY;
		}
		
		/// <summary>
		/// 判断角度是否处在StartAngle和EndAngle之间
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public bool IsAngleInArc(double testAngle)
		{
			if (sweepAngle > 0)
			{
				//将testAngle统一到StartAngle和StartAngle+360之间
				while (testAngle - startAngle < 0)
				{
					testAngle += 360;
				}
				while (testAngle - startAngle > 360)
				{
					testAngle -= 360;
				}
				if (testAngle >= startAngle && testAngle <= startAngle + sweepAngle)
				{
					return true;
				}
			}
			else
			{
				//将testAngle统一到StartAngle-360和StartAngle之间
				while (testAngle - (startAngle - 360) < 0)
				{
					testAngle += 360;
				}
				while (testAngle - startAngle > 0)
				{
					testAngle -= 360;
				}
				if (testAngle >= startAngle + sweepAngle && testAngle <= startAngle)
				{
					return true;
				}
			}
			
			return false;
		}
			
    }
}
