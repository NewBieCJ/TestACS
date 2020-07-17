/*
 * Created by SharpDevelop.
 * User: Hewenyuan
 * Date: 2019/5/14
 * Time: 11:01
 * 
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DocViewerDemo.DrawEntity;

namespace DocViewerDemo.Snap
{
	/// <summary>
	/// 捕捉点管理类
	/// </summary>
	public class SnapManager
	{
		List<SnapEnum> snapOptions = new List<SnapEnum>();
		//输入的实体
		DrawEntityManager entityManager;
		//绘图接口
		IDocViewer viewer;
		//当前的捕捉点
		SnappedPoint snappedPoint;
		
		
		public SnapManager(DrawEntityManager entityManager,IDocViewer viewer)
		{
			this.entityManager = entityManager;
			this.viewer = viewer;
		}
		
		// 设置需要绘制的实体Manager
        public void SetEntityManager(DrawEntityManager entityManager)
        {
            this.entityManager = entityManager;
        }
		
		/// <summary>
        /// Mouse Move
        /// </summary>
        public void OnMouseMove(MouseEventArgs e)
        {
        	//转换鼠标位置
        	var pointInDoc = viewer.TransFromScreenToDoc(new Vector(e.X,e.Y,0));
        	//转换窗口大小
        	double snapWindowInDoc = viewer.TransFromScreenToDoc(20);
        	//计算当前的捕捉点
        	snappedPoint = Snap(pointInDoc,snapWindowInDoc);
        }
        
		
        //获取鼠标附件的捕捉点
		public DocViewerDemo.Snap.SnappedPoint Snap(Vector mousePointInDoc,double snapWindowInDoc)
		{
			double minDis = double.MaxValue;
			SnappedPoint snappedPoint = null;
			//遍历元素
			foreach (var element in entityManager.allEntities)
			{
				var elementSnappedPoint = element.GetSnapPoint(mousePointInDoc.x,mousePointInDoc.y,snapWindowInDoc);
				if(elementSnappedPoint != null)
				{
					double dis = Math.Sqrt(Math.Pow(elementSnappedPoint.point.x,2)+ Math.Pow(elementSnappedPoint.point.y,2));
					if(dis<minDis)
					{
						minDis = dis;
						snappedPoint = elementSnappedPoint;
					}
				}
			}//遍历元素
			
			return snappedPoint;
		}
		
		//绘制捕捉点
		public void Draw()
		{
			if(snappedPoint == null)
			{
				return;
			}
			
			//转换窗口大小
        	double snapWindowInDoc = viewer.TransFromScreenToDoc(10);
        	
			//端点 绘制框
			if(snappedPoint.pointType == SnapEnum.Snap_EndPoint)
			{
				viewer.DrawRect((float)(snappedPoint.point.x - snapWindowInDoc/2),(float)(snappedPoint.point.y + snapWindowInDoc/2),
				                (float)(snappedPoint.point.x + snapWindowInDoc/2),(float)(snappedPoint.point.y - snapWindowInDoc/2),
				                Color.Blue,2);
			}
			//中点 绘制三角
			else if(snappedPoint.pointType == SnapEnum.Snap_MiddlePoint)
			{
				viewer.DrawLine((float)(snappedPoint.point.x),(float)(snappedPoint.point.y + snapWindowInDoc/2),
				                (float)(snappedPoint.point.x - snapWindowInDoc/2),(float)(snappedPoint.point.y - snapWindowInDoc/2),
				                Color.Blue,2);
				viewer.DrawLine((float)(snappedPoint.point.x - snapWindowInDoc/2),(float)(snappedPoint.point.y - snapWindowInDoc/2),
				                (float)(snappedPoint.point.x + snapWindowInDoc/2),(float)(snappedPoint.point.y - snapWindowInDoc/2),
				                Color.Blue,2);
				viewer.DrawLine((float)(snappedPoint.point.x + snapWindowInDoc/2),(float)(snappedPoint.point.y - snapWindowInDoc/2),
				                (float)(snappedPoint.point.x),(float)(snappedPoint.point.y + snapWindowInDoc/2),
				                Color.Blue,2);
			}
			//圆心
			else if(snappedPoint.pointType == SnapEnum.Snap_Center)
			{
				viewer.DrawCircle((float)(snappedPoint.point.x),(float)(snappedPoint.point.y),snapWindowInDoc/2,Color.Blue,2);
			}
			//控制点
			if(snappedPoint.pointType == SnapEnum.Snap_ControlPoint)
			{
				viewer.DrawRect((float)(snappedPoint.point.x - snapWindowInDoc/2),(float)(snappedPoint.point.y + snapWindowInDoc/2),
				                (float)(snappedPoint.point.x + snapWindowInDoc/2),(float)(snappedPoint.point.y - snapWindowInDoc/2),
				                Color.Blue,2);
			}
		}
	}//class
	
	/// <summary>
	/// 捕捉类型
	/// </summary>
	public enum SnapEnum
	{
		//端点
		Snap_EndPoint,
		//中点
		Snap_MiddlePoint,
		//圆心
		Snap_Center,
		//节点
		Snap_ControlPoint,
		//交叉点
		Snap_CrossPoint,
		//元素上最近点
		Snap_PointOnEntity,
		//正交
		Snap_Orthogonality
	}
	
}
