/*
 * Created by SharpDevelop.
 * User: Hewenyuan
 * Date: 2019/5/11
 * Time: 14:03
 * 
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using DocViewerDemo.DrawEntity;

namespace DocViewerDemo.Command.DrawCommand
{
	/// <summary>
	/// 圆绘制命令--圆上三点
	/// 需要量步
	/// 步0：指定圆上第一点
	/// 步1：指定圆上第二点
	/// 步2：指定圆上第三点
	/// </summary>
	public class DrawCircleThreePointCommand:Command
	{
		//绘图接口
		IDocViewer viewer;
		
		//鼠标当前位置(屏幕坐标)
		private Point mousePointCurrent;
		
		//第一绘制点
		private Vector firstPoint = new Vector();
		
		//第二绘制点
		private Vector secondPoint = new Vector();
		
		//第三绘制点
		private Vector thirdPoint = new Vector();
		
		
		//当前已创建到哪一步
		private int curerentStep;
		
		public DrawCircleThreePointCommand(DrawEntity.IDocViewer viewer)
		{
			this.viewer = viewer;
			curerentStep = 0;
		}
		
		/// <summary>
		/// 完成
		/// </summary>
		public override void Finish()
		{
			base.Finish();
		}

		/// <summary>
		/// 撤销
		/// </summary>
		public override void Cancel()
		{
			base.Cancel();
		}
		
		/// <summary>
		/// Mouse Down
		/// </summary>
		public override EventResult OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				//转换坐标
				var pointInDoc = viewer.TransFromScreenToDoc(new Vector(e.X,e.Y,0));
				
				if(curerentStep == 0)
				{
					firstPoint = pointInDoc;
					curerentStep = 1;
				}
				else if(curerentStep == 1)
				{
					secondPoint = pointInDoc;
					curerentStep = 2;
				}
				else if(curerentStep == 2)
				{
					thirdPoint =pointInDoc;
					curerentStep = 3;
					
					//创建完成
					this.Finish();
					
					//重绘
        			viewer.Render(true);
				}
				
			}

			return EventResult.Handled;
		}

		/// <summary>
		/// Mouse Up
		/// </summary>
		public override EventResult OnMouseUp(MouseEventArgs e)
		{
			return EventResult.Unhandled;
		}

		/// <summary>
		/// Mouse Move
		/// </summary>
		public override EventResult OnMouseMove(MouseEventArgs e)
		{
			//记录鼠标位置
			mousePointCurrent =  e.Location;
			//转换鼠标坐标
			var pointInDoc = viewer.TransFromScreenToDoc(new Vector(mousePointCurrent.X,mousePointCurrent.Y,0));
			
			
			return EventResult.Unhandled;
		}

		/// <summary>
		/// Mouse Wheel
		/// </summary>
		public override EventResult OnMouseWheel(MouseEventArgs e)
		{
			return EventResult.Unhandled;
		}

		/// <summary>
		/// Key Down
		/// </summary>
		public override EventResult OnKeyDown(KeyEventArgs e)
		{
			return EventResult.Unhandled;
		}

		/// <summary>
		/// Key Up
		/// </summary>
		public override EventResult OnKeyUp(KeyEventArgs e)
		{
			return EventResult.Unhandled;
		}
		
		/// <summary>
		/// Paint
		/// </summary>
		public override void OnPaint(Graphics g)
		{
			//绘制
			if(curerentStep == 1)
			{
				//转换鼠标坐标
				var mousePointInDoc = viewer.TransFromScreenToDoc(new Vector(mousePointCurrent.X,mousePointCurrent.Y,0));
				//绘制第一点到第二点连线
				viewer.DrawLine(firstPoint.x,firstPoint.y,mousePointInDoc.x,mousePointInDoc.y,Color.Black,1);
			}
			else if(curerentStep == 2)
			{
				//转换鼠标坐标
				var mousePointInDoc = viewer.TransFromScreenToDoc(new Vector(mousePointCurrent.X,mousePointCurrent.Y,0));
				//绘制第一点到第二点连线
				viewer.DrawLine(firstPoint.x,firstPoint.y,secondPoint.x,secondPoint.y,Color.Black,1);
				//绘制第二点到鼠标点连线
				viewer.DrawLine(firstPoint.x,firstPoint.y,mousePointInDoc.x,mousePointInDoc.y,Color.Black,1);
				
				//计算圆参数
				double centerX = 0,centerY= 0,radius= 0,startAngle= 0,sweepAngle= 0;
				DrawEntity_Arc.CreateArcByThreePoint(firstPoint,secondPoint,mousePointInDoc,ref centerX,ref centerY,ref radius,ref startAngle,ref sweepAngle);
				//绘制圆
				viewer.DrawCircle(centerX,centerY,radius,Color.Black,1);
				//绘制圆心
				double length = viewer.TransFromScreenToDoc(5);
				viewer.DrawLine(centerX - length,centerY,centerX + length,centerY,Color.Black,1);
				viewer.DrawLine(centerX,centerY-length,centerX,centerY+length,Color.Black,1);
				
			}

		}
	}
}
