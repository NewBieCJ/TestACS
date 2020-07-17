/*
 * Created by SharpDevelop.
 * User: Hewenyuan
 * Date: 2019/5/10
 * Time: 13:35
 * 
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using DocViewerDemo.DrawEntity;

namespace DocViewerDemo.Command.GripPointCommand
{
	/// <summary>
	/// 夹持点（元素控制点）鼠标操作命令
	/// </summary>
	public class GripPointCommand:Command
	{
		//绘图接口
		private DrawEntity.IDocViewer viewer;
		//夹持夹所属于的元素
		private DrawEntity.DrawEntity entity;
		//元素拷贝对象
		private DrawEntity.DrawEntity entityCopy;
		//夹持点在元素夹持点列表中的编号
		private int gripPointIndex = 0;
		//夹持点初始位置
		private Vector gripPointInitialPos = new Vector();
		//鼠标当前位置(屏幕坐标)
		private Point mousePointCurrent;
		
		
		public GripPointCommand(DrawEntity.DrawEntity entity,int gripPointIndex,DrawEntity.IDocViewer viewer)
		{
			this.entity = entity;
			entityCopy = (DrawEntity.DrawEntity)entity.Clone();
			this.gripPointIndex = gripPointIndex;
			this.viewer = viewer;
			
			entityCopy.SetViewer(viewer);
			//设置元素当前夹持点
			entity.SetCurrentGripPointIndex(gripPointIndex);
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
        		entity.SetGripPoint(gripPointIndex,pointInDoc);
        		
        		
        		//修改完成
				this.Finish();
				
				//重绘
    			viewer.Render(true);
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
        	entityCopy = (DrawEntity.DrawEntity)entity.Clone();
        	entityCopy.SetViewer(viewer);
        	entityCopy.SetCurrentGripPointIndex(gripPointIndex);
        	entityCopy.SetGripPoint(gripPointIndex,pointInDoc);
        	
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
        	entityCopy.Draw();
        	//获得当前夹持点的位置
        	var gripOriginPos = entity.GetGripPoint(gripPointIndex);
        	//转换坐标
        	var pointInScreen = viewer.TransFromDocToScreen(gripOriginPos);
        	//绘制夹持点到当前位置的连线
        	g.DrawLine(new Pen(Color.Gray),(float)pointInScreen.x,(float)pointInScreen.y,mousePointCurrent.X,mousePointCurrent.Y);
        }
	}
}
