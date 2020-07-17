/*
 * Created by SharpDevelop.
 * User: Hewenyuan
 * Date: 2019/5/11
 * Time: 13:13
 * 
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using DocViewerDemo.DrawEntity;

namespace DocViewerDemo.Command.DrawCommand
{
	/// <summary>
	/// 直线绘制命令
	/// 直线只需两步，起点和终点
	/// 步0：绘制第一点
	/// 步1：绘制第二点
	/// </summary>
	public class DrawLineCommand:Command
	{
		//绘图接口
		IDocViewer viewer;
		
		//元素拷贝对象
		private DrawEntity.DrawEntity entityCopy;
		
		//鼠标当前位置(屏幕坐标)
		private Point mousePointCurrent;
		
		//第一绘制点
		private Vector firstPoint = new Vector();
		
		//第二绘制点
		private Vector secondPoint = new Vector();
		
		
		//当前已创建到哪一步
		private int curerentStep;
		
		public DrawLineCommand(DrawEntity.IDocViewer viewer)
		{
			this.viewer = viewer;
			entityCopy = new DocViewerDemo.DrawEntity.DrawEntity_Line(0,0,10,10,Color.Black,1);
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
			//绘制第一点到鼠标点的连线
			if(curerentStep == 1)
			{
				//转换鼠标坐标
				var mousePointInDoc = viewer.TransFromScreenToDoc(new Vector(mousePointCurrent.X,mousePointCurrent.Y,0));
				//绘制
				viewer.DrawLine(firstPoint.x,firstPoint.y,mousePointInDoc.x,mousePointInDoc.y,Color.Black,1);
			}

		}
	}
}
