/*
 * Created by SharpDevelop.
 * User: Hewenyuan
 * Date: 2019/5/14
 * Time: 11:35
 * 
 */
using System;
using DocViewerDemo.DrawEntity;

namespace DocViewerDemo.Snap
{
	/// <summary>
	/// 捕捉到的点数据
	/// </summary>
	public class SnappedPoint
	{
		//捕捉到的点位置
		public Vector point;
		
		//捕捉到的点类型
		public Snap.SnapEnum pointType;
	
		//构造函数
		public SnappedPoint()
		{
			
		}
		
		//构造函数
		public SnappedPoint(Vector point,Snap.SnapEnum pointType)
		{
			this.point = point;
			this.pointType = pointType;
		}		
	}
}
