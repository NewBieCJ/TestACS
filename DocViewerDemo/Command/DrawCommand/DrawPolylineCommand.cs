/*
 * Created by SharpDevelop.
 * User: Hewenyuan
 * Date: 2019/5/13
 * Time: 10:11
 * 
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DocViewerDemo.DrawEntity;

namespace DocViewerDemo.Command.DrawCommand
{
	/// <summary>
	/// 绘制多段线命令
	/// </summary>
	public class DrawPolylineCommand:Command
	{
		//绘图接口
		IDocViewer viewer;
		
		//元素拷贝对象
		private DrawEntity.DrawEntity entityCopy;
		
		//鼠标当前位置(屏幕坐标)
		private Point mousePointCurrent;
		
		//第一绘制点
		private List<Vector> controlPoints = new List<Vector>();
	
		
		public DrawPolylineCommand(DrawEntity.IDocViewer viewer)
		{
			this.viewer = viewer;
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
				
				//添加关键点
				controlPoints.Add(pointInDoc);
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
			//绘制已添加的线段
			if(controlPoints.Count >= 2)
			{
				for (int i = 0; i < controlPoints.Count-1; i++) 
				{
					var point0 = controlPoints[i];
					var point1 = controlPoints[i+1];
					//绘制
					viewer.DrawLine(point0.x,point0.y,point1.x,point1.y,Color.Black,1);
				}
			}
			
			
			
			//绘制最后已点到鼠标点的连线
			if(controlPoints.Count >= 1)
			{
				var lastPoint = controlPoints[controlPoints.Count-1];
				
				//转换鼠标坐标
				var mousePointInDoc = viewer.TransFromScreenToDoc(new Vector(mousePointCurrent.X,mousePointCurrent.Y,0));
				
				
				//绘制
				viewer.DrawLine(lastPoint.x,lastPoint.y,mousePointInDoc.x,mousePointInDoc.y,Color.Black,1);
			}

		}
	}
}
