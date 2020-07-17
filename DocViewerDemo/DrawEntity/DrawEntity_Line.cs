using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocViewerDemo.DrawEntity
{
	public class DrawEntity_Line:DrawEntity
	{
		Vector pointStart = new Vector();
		Vector pointEnd = new Vector();
		Color color = Color.White;
		int width = 1;

		public DrawEntity_Line(double startX, double startY, double endX, double endY, Color color, int width = 1)
		{
			pointStart.x = startX;
			pointStart.y = startY;
			pointEnd.x = endX;
			pointEnd.y = endY;
			this.color = color;
			this.width = width;
		}
        
		public override object Clone()
		{
			return new DrawEntity_Line(pointStart.x, pointStart.y, pointEnd.x, pointEnd.y, color, width);
		}

		public override void Draw()
		{
			Color lineColor = color;
			if (status_selected == false)
			{
				lineColor = color;
			}
			else
			{
				lineColor = Color.Red;
			}
        	
			this.DrawLine(pointStart, pointEnd, width, lineColor);
        	
			//需要显示操作框
			if (status_showHandle == true)
			{
				//绘制端点操作框
				this.DrawHandleRect(pointStart, handelRectNormalColor, handelRectBorderColor, handleRectSize);
				this.DrawHandleRect(pointEnd, handelRectNormalColor, handelRectBorderColor, handleRectSize);
        		
				double centerX = (pointStart.x + pointEnd.x) / 2;
				double centerY = (pointStart.y + pointEnd.y) / 2;
        		
				//绘制中点操作框
				this.DrawHandleRect(new Vector(centerX, centerY, 0), handelRectNormalColor, handelRectBorderColor, handleRectSize);
			}
		}
        
		//是否在矩形框内
		public override bool IsInRect(Vector leftTop, Vector rightBottom)
		{
			if (pointStart.x > leftTop.x && pointStart.x < rightBottom.x
			         && pointStart.y < leftTop.y && pointStart.y > rightBottom.y
			         && pointEnd.x > leftTop.x && pointEnd.x < rightBottom.x
			         && pointEnd.y < leftTop.y && pointEnd.y > rightBottom.y)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
        
		//是否与矩形框交集
		public override bool IsIntersectionWithRect(Vector leftTop, Vector rightBottom)
		{
			Vector rightTop = new Vector(rightBottom.x, leftTop.y, 0);
			Vector leftBottom = new Vector(leftTop.x, rightBottom.y, 0);
         	
			Vector intersetPoint = new Vector();
			bool intersectWithLeft = Hatch.PathHatch.LineSegmentIntersection(pointStart, pointEnd - pointStart, leftTop, leftBottom - leftTop, ref intersetPoint);
			bool intersectWithTop = Hatch.PathHatch.LineSegmentIntersection(pointStart, pointEnd - pointStart, leftTop, rightTop - leftTop, ref intersetPoint);
			bool intersectWithRight = Hatch.PathHatch.LineSegmentIntersection(pointStart, pointEnd - pointStart, rightBottom, rightTop - rightBottom, ref intersetPoint);
			bool intersectWithBottom = Hatch.PathHatch.LineSegmentIntersection(pointStart, pointEnd - pointStart, rightBottom, leftBottom - rightBottom, ref intersetPoint);
			
			if (intersectWithLeft || intersectWithTop || intersectWithRight || intersectWithBottom)
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
			return new Vector[3]{ pointStart, pointEnd, new Vector((pointStart.x + pointEnd.x) / 2, (pointStart.y + pointEnd.y) / 2, 0) };
		}
        
		//获得夹持点坐标
		public override  Vector GetGripPoint(int index)
		{
			switch (index)
			{
				case 0:
					return pointStart;
					break;
				case 1:
					return pointEnd;
					break;
				case 2:
					return new Vector((pointStart.x + pointEnd.x) / 2, (pointStart.y + pointEnd.y) / 2, 0);
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
		public override  void SetGripPoint(int index, Vector pos)
		{
			switch (index)
			{
				case 0:
					pointStart = pos;
					break;
				case 1:
					pointEnd = pos;
					break;
				case 2:
					double centerX = (pointStart.x + pointEnd.x) / 2;
					double centerY = (pointStart.y + pointEnd.y) / 2;
					pointStart.x += (pos.x - centerX);
					pointStart.y += (pos.y - centerY);
					pointEnd.x += (pos.x - centerX);
					pointEnd.y += (pos.y - centerY);
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
			leftTop.x = (pointStart.x < pointEnd.x ? pointStart.x : pointEnd.x) - snapWindowSize/2;
			leftTop.y = (pointStart.y > pointEnd.y ? pointStart.y : pointEnd.y) + snapWindowSize/2;
			width = Math.Abs(pointEnd.x - pointStart.x) + snapWindowSize;
			height = Math.Abs(pointEnd.y - pointStart.y) + snapWindowSize;
			
			if(mouseX >= leftTop.x && mouseX<=leftTop.x + width && mouseY<= leftTop.y && mouseY >= leftTop.y - height)
			{
				
			}
			else
			{
				return null;
			}
			
			//判断是否在端点附近
			if(Math.Abs(mouseX - pointStart.x)<snapWindowSize/2 && Math.Abs(mouseY - pointStart.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(pointStart, Snap.SnapEnum.Snap_EndPoint);
			}
			
			if(Math.Abs(mouseX - pointEnd.x)<snapWindowSize/2 && Math.Abs(mouseY - pointEnd.y)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(pointEnd, Snap.SnapEnum.Snap_EndPoint);
			}
			
			
			//判断是否在中点附近
			double centerX = (pointStart.x + pointEnd.x)/2;
			double centerY = (pointStart.y + pointEnd.y)/2;
			if(Math.Abs(mouseX - centerX)<snapWindowSize/2 && Math.Abs(mouseY - centerY)<snapWindowSize/2)
			{
				return new DocViewerDemo.Snap.SnappedPoint(new Vector(centerX,centerY,0), Snap.SnapEnum.Snap_MiddlePoint);
			}
			
			
			//计算离元素最近点
			
			
			
			return null;
		}
	}
//class
}
//nampespace
