using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocViewerDemo.DrawEntity
{
    /// <summary>
    /// 绘制实体基类
    /// 默认坐标是图纸坐标
    /// </summary>
    public abstract class DrawEntity: ICloneable
    {
        private IDocViewer viewer = null;
        
        
        //状态（元素被选中，显示选中颜色）
        public bool status_selected = false;
        //状态（元素被选中，显示操作框）
        public bool status_showHandle= false;
        
        //操作框大小(单位像素)
        protected int handleRectSize = 10;
        //操作框填充颜色
        protected Color handelRectNormalColor = Color.DodgerBlue;
        //操作框被选中颜色
        protected Color handleRectSelectColor = Color.Red;
        //操作框border颜色
        protected Color handelRectBorderColor = Color.Gray;
        
        //当前夹持点
        protected int gripCurentIndex = -1;
        
        
        
        
        
        public void SetViewer(IDocViewer viewer)
        {
            this.viewer = viewer;
        }

        //绘制函数
        virtual public void Draw() { }
        //绘制控制框
        virtual public void DrawHandle() { }
        //框选函数
        virtual public bool RectSelect(double leftTopX, double leftTopY, double rightBottomX, double rightBottomY) { return false; }
        //鼠标选中测试
        virtual public bool HitTest(){return false;}
        //鼠标拖拽
        virtual public void Drag(int handleIndex,double mouseX,double mouseY){}
        /// <summary>
        /// Snap
        /// 参数坐标系都为图纸坐标
        /// </summary>
        /// <param name="mouseX">鼠标位置X</param>
        /// <param name="mouseY">鼠标位置Y</param>
        /// <param name="snapWindowSizeInDoc">鼠标Snap的窗口大小</param>
        virtual public DocViewerDemo.Snap.SnappedPoint GetSnapPoint(double mouseX,double mouseY,double snapWindowSize){return null;}
        //是否在矩形框内
        virtual public bool IsInRect(Vector leftTop,Vector rightBottom){return false;}
        //是否与矩形框交集
        virtual public bool IsIntersectionWithRect(Vector leftTop,Vector rightBottom){return false;}
        //获得所有夹持点
        virtual public Vector[] GetAllGripPoint(){return new Vector[0];}
        //获得夹持点坐标
        virtual public Vector GetGripPoint(int index){return new Vector();}
        //设置当前夹持点
        virtual public void SetCurrentGripPointIndex(int index){gripCurentIndex = index;}
        //设置指定夹持点坐标
        virtual public void SetGripPoint(int index,Vector pos){}
        
        
        
        /*
            图纸坐标系到窗口坐标系的转换
            默认状态：图纸坐标原点对应窗口中心
        */
        protected Vector TransFromDocToScreen(Vector pointInDoc)
        {
        	return viewer.TransFromDocToScreen(pointInDoc);
        }
        protected double TransFromDocToScreen(double  lengthInDoc)
        {
        	return viewer.TransFromDocToScreen(lengthInDoc);
        }
        /*
            窗口坐标系到图纸坐标系的转换
            默认状态：图纸坐标原点对应窗口中心
        */
        protected Vector TransFromScreenToDoc(Vector pointInScreen)
        {
        	return viewer.TransFromScreenToDoc(pointInScreen);
        }
        protected double TransFromScreenToDoc(double lengthInScreen)
        {
        	return viewer.TransFromScreenToDoc(lengthInScreen);
        }
        
        
        /// <summary>
        /// 绘制控制框
        /// </summary>
        /// <param name="pointInDoc"></param>
        protected void DrawHandleRect(Vector pointInDoc,Color fillColor,Color borderColor,int rectSizeInScreen)
        {
        	//计算点屏幕上
        	viewer.DrawHandleRect(pointInDoc,fillColor,borderColor,rectSizeInScreen);
        }
        
        /// <summary>
        /// 直线绘制函数
        /// 参数坐标系：图纸坐标
        /// </summary>
        /// <param name="pointStart">起点坐标</param>
        /// <param name="pointEnd">终点坐标</param>
        /// <param name="lineWidth">直线宽度</param>
        /// <param name="color">颜色</param>
        protected void DrawLine(Vector pointStart,Vector pointEnd,int lineWidth,Color color)
        {
            if (viewer == null) return;

            viewer.DrawLine(pointStart.x, pointStart.y, pointEnd.x, pointEnd.y, color, lineWidth);
        }

        /// <summary>
        /// 圆绘制函数
        /// 参数坐标系：图纸坐标
        /// </summary>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <param name="lineWidth">直线宽度</param>
        /// <param name="color">颜色</param>
        protected void DrawCircle(Vector center,double radius, int lineWidth, Color color)
        {
            if (viewer == null) return;

            viewer.DrawCircle(center.x,center.y,radius,color,lineWidth);
        }

        /// <summary>
        /// 圆弧绘制函数
        /// 参数坐标系：图纸坐标
        /// </summary>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">起始角度，单位度</param>
        /// <param name="sweepAngle">扫描角度，单位度</param>
        /// <param name="lineWidth">直线宽度</param>
        /// <param name="color">颜色</param>
        protected void DrawArc(Vector center, double radius, double startAngle,double sweepAngle,int lineWidth, Color color)
        {
            if (viewer == null) return;

            viewer.DrawArc(center.x, center.y, radius, startAngle,sweepAngle,color, lineWidth);
        }

        /// <summary>
        /// 文字沪指函数
        /// </summary>
        /// <param name="leftTop">左上角坐标</param>
        /// <param name="text">文字内容</param>
        /// <param name="font">字体</param>
        /// <param name="height">字体高度，单位mm</param>
        /// <param name="color">颜色</param>
        protected void DrawText(Vector leftTop,string text,Font font,double height,Color color)
        {
            if (viewer == null) return;

            viewer.DrawText(leftTop.x, leftTop.y, text, font, height,color);
        }

        // 绘制Bezier
        protected void DrawBezier(PointF p1, PointF p2, PointF p3, PointF p4, Color color, int width)
        {
            if (viewer == null) return;

            viewer.DrawBezier(p1,p2,p3,p4, color, width);
        }
        
        // 绘制Bezier
        protected void DrawBeziers(PointF[] points, Color color, int width)
        {
            if (viewer == null) return;

            viewer.DrawBeziers(points, color, width);
        }
        
        
        //绘制矩形框
        protected void DrawRect(float left,float top,float right,float bottom,Color color,int width)
        {
        	if (viewer == null) return;
        	
        	viewer.DrawRect(left,top,right,bottom,color,width);
        }
        
        //绘制填充矩形框
        protected void FillRect(float left,float top,float right,float bottom,Brush brush)
        {
        	if (viewer == null) return;
        	
        	viewer.FillRect(left,top,right,bottom,brush);
        }
        
		#region ICloneable implementation
		public virtual object Clone()
		{
			return this.MemberwiseClone();
		}
		#endregion
    }//class
}//namespace
