using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocViewerDemo.DrawEntity
{
    public class DrawEntity_Circle:DrawEntity
    {
        Vector center = new Vector();
        double radius = 0;
        Color color = Color.White;
        int width = 1;

        public DrawEntity_Circle(double centerX,double centerY,double radius, Color color, int width = 1)
        {
            center.x = centerX;
            center.y = centerY;
            this.radius = radius;
            this.width = width;
            this.color = color;
        }

		public override object Clone()
		{
			return new DrawEntity_Circle(center.x,center.y,radius,color,width);
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
        	
            this.DrawCircle(center, radius, width, lineColor);

            double radiusInScreen = this.TransFromDocToScreen(radius);

        	//需要显示操作框
        	if(status_showHandle == true && radiusInScreen>1)
        	{

        		//绘制边缘操作框
        		this.DrawHandleRect(new Vector(center.x + radius,center.y,0),handelRectNormalColor,handelRectBorderColor,handleRectSize);
        		this.DrawHandleRect(new Vector(center.x - radius,center.y,0),handelRectNormalColor,handelRectBorderColor,handleRectSize);
        		this.DrawHandleRect(new Vector(center.x,center.y + radius,0),handelRectNormalColor,handelRectBorderColor,handleRectSize);
        		this.DrawHandleRect(new Vector(center.x,center.y - radius,0),handelRectNormalColor,handelRectBorderColor,handleRectSize);
        		
        		//绘制中心操作框
        		this.DrawHandleRect(center,handelRectNormalColor,handelRectBorderColor,handleRectSize);
        	}
        }
        
        
        //是否在矩形框内
        public override bool IsInRect(Vector leftTop,Vector rightBottom)
        {
        	//计算border
        	double borderLeft = center.x - radius;
        	double borderTop = center.y + radius;
        	double borderRight = center.x + radius;
        	double borderBottom = center.y - radius;
        	
        	
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
         	
         	bool intersectWithLeft = Hatch.PathHatch.ArcIntersectionWithLineSegment(center,radius,0,360,leftTop,leftBottom).Count>0?true:false;
			bool intersectWithTop =  Hatch.PathHatch.ArcIntersectionWithLineSegment(center,radius,0,360,leftTop,rightTop).Count>0?true:false;
			bool intersectWithRight = Hatch.PathHatch.ArcIntersectionWithLineSegment(center,radius,0,360,rightTop,rightBottom).Count>0?true:false;
			bool intersectWithBottom =  Hatch.PathHatch.ArcIntersectionWithLineSegment(center,radius,0,360,leftBottom,rightBottom).Count>0?true:false;
			
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
        	Vector pointLef  = new Vector();
        	Vector pointTop = new Vector();
        	Vector pointRight = new Vector();
        	Vector pointBottom = new Vector();
        	
        	pointLef.x = center.x - radius;
    		pointLef.y = center.y ;
    		
    		pointTop.x = center.x;
    		pointTop.y = center.y + radius;
    		
    		pointRight.x = center.x + radius;
    		pointRight.y = center.y;
    		
    		pointBottom.x = center.x;
    		pointBottom.y = center.y - radius;
        	
        	return new Vector[5]{pointLef,pointTop,pointRight,pointBottom,center};
        }
        
        //获得夹持点坐标
        public override  Vector GetGripPoint(int index)
        {
        	Vector pointLef  = new Vector();
        	Vector pointTop = new Vector();
        	Vector pointRight = new Vector();
        	Vector pointBottom = new Vector();
        	
        	pointLef.x = center.x - radius;
    		pointLef.y = center.y ;
    		
    		pointTop.x = center.x;
    		pointTop.y = center.y + radius;
    		
    		pointRight.x = center.x + radius;
    		pointRight.y = center.y;
    		
    		pointBottom.x = center.x;
    		pointBottom.y = center.y - radius;
    		
        	switch (index)
        	{
        		case 0:
        			return pointLef;
        			break;
        		case 1:
        			return pointTop;
					break;
				case 2:
					return pointRight;
					break;
				case 3:
					return pointBottom;
					break;
				case 4:
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
        	switch (index)
        	{
        		case 0:
        		case 1:
        		case 2:
        		case 3:
        			radius  = Math.Sqrt(Math.Pow(pos.x - center.x,2)+Math.Pow(pos.y - center.y,2));
        			break;
				case 4:
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
			Vector leftTop = new Vector();
			double width, height;
			leftTop.x = center.x - radius - snapWindowSize/2;
			leftTop.y = center.y + radius + snapWindowSize/2;
			width = radius*2 + snapWindowSize;
			height = radius*2 + snapWindowSize;
			
			if(mouseX >= leftTop.x && mouseX<=leftTop.x + width && mouseY<= leftTop.y && mouseY >= leftTop.y - height)
			{
				
			}
			else
			{
				return null;
			}
			
			//判断是否在圆心附近
			if(Math.Abs(mouseX - center.x)<snapWindowSize/2 && Math.Abs(mouseY - center.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(center, Snap.SnapEnum.Snap_Center);
			}
			
			
			
			//判断是否在象限控制点附近
			Vector leftControlPoint = new Vector(center.x - radius,center.y,0);
			Vector topControlPoint = new Vector(center.x,center.y + radius,0);
			Vector rightControlPoint = new Vector(center.x + radius,center.y,0);
			Vector bottomControlPoint = new Vector(center.x,center.y - radius,0);
			if(Math.Abs(mouseX - leftControlPoint.x)<snapWindowSize/2 && Math.Abs(mouseY - leftControlPoint.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(leftControlPoint, Snap.SnapEnum.Snap_ControlPoint);
			}
			if(Math.Abs(mouseX - topControlPoint.x)<snapWindowSize/2 && Math.Abs(mouseY - topControlPoint.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(topControlPoint, Snap.SnapEnum.Snap_ControlPoint);
			}
			if(Math.Abs(mouseX - rightControlPoint.x)<snapWindowSize/2 && Math.Abs(mouseY - rightControlPoint.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(rightControlPoint, Snap.SnapEnum.Snap_ControlPoint);
			}
			if(Math.Abs(mouseX - bottomControlPoint.x)<snapWindowSize/2 && Math.Abs(mouseY - bottomControlPoint.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(bottomControlPoint, Snap.SnapEnum.Snap_ControlPoint);
			}
			
			
			//计算离元素最近点
			
			
			
			return null;
		}
        
    }//class
}//namespace
