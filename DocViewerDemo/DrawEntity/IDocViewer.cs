using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocViewerDemo.DrawEntity
{
    /// <summary>
    /// 窗口绘制函数需要实现的接口
    /// </summary>
    public interface IDocViewer
    {
        // 缩放比例
        double Scale { get; set; }
        // 平移X
        double TranslateX { get; set; }
        // 平移Y
        double TranslateY { get; set; }
        // 设置需要绘制的实体Manager
        void SetEntityManager(DrawEntityManager entityManager);
        
        /*
            图纸坐标系到窗口坐标系的转换
            默认状态：图纸坐标原点对应窗口中心
            图纸X轴对应窗口X轴，方向相同
            图纸Y轴对应窗口Y轴，方向相反
        */
        Vector TransFromDocToScreen(Vector pointInDoc);
        double TransFromDocToScreen(double  lengthInDoc);
        /*
            窗口坐标系到图纸坐标系的转换
            默认状态：图纸坐标原点对应窗口中心
        */
        Vector TransFromScreenToDoc(Vector pointInScreen);
        double TransFromScreenToDoc(double lengthInScreen);
        	
        	
        	
        // 绘制函数
        void Render(bool redrawAll = false);
        // 直线绘制函数
        void DrawLine(double startX, double startY, double endX, double endY, Color color, int width);
        // 圆绘制函数
        void DrawCircle(double centerX,double centerY,double radius, Color color, int width);
        // 绘制圆弧
        void DrawArc(double centerX, double centerY, double radius, double startAngle, double sweepAngle, Color color, int width);
        // 绘制文字
        void DrawText(double left,double top,string text,Font font,double height,Color color);
        //绘制Bezier
        void DrawBezier(PointF p1, PointF p2, PointF p3, PointF p4, Color color, int width);
        void DrawBeziers(PointF[] points, Color color, int width);
        //绘制矩形框
        void DrawRect(float left,float top,float right,float bottom,Color color,int width);
        //绘制填充矩形框
        void FillRect(float left,float top,float right,float bottom,Brush brush);
        //绘制操作矩形框
        void DrawHandleRect(Vector pointInDoc,Color fillColor,Color borderColor,int rectSizeInScreen);
    }
}
