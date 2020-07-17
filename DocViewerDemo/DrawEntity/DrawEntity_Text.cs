using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DocViewerDemo.DrawEntity
{
	[StructLayout(LayoutKind.Sequential)]
	public struct FIXED
	{
		public short fract;
		public short value;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct POINTFX
	{
		[MarshalAs(UnmanagedType.Struct)]
		public FIXED x;
		[MarshalAs(UnmanagedType.Struct)]
		public FIXED y;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct TTPOLYGONHEADER
	{
		public int cb;
		public int dwType;
		[MarshalAs(UnmanagedType.Struct)]
		public POINTFX pfxStart;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct TTPOLYCURVE
	{
		public short wType;
		public short cpfx;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
		public POINTFX[] apfx;
		/// POINTFX[1]
	}

	/// <summary>
	/// 单行文字
	/// </summary>
	public class DrawEntity_Text:DrawEntity
	{
		//static Bitmap bitmapForFont = new Bitmap(1,1);
		//static Graphics gBufferForFont = Graphics.FromImage(bitmapForFont);
		//static IntPtr hdcForFont = gBufferForFont.GetHdc();

		Vector pointLeftTop = new Vector();
		char text = 'a';
		Font font = new Font(new FontFamily("Microsoft YaHei"), 10);
		double height = 10;
		//文字高度，单位mm
		int fontHeightForGetOutline = 5000;
		//提取轮廓时使用的文字高度
		Color color = Color.White;
		int width = 1;
		//是否已生成轮廓曲线
		bool isGeneratedOutline = false;

		//文字对应的路径数据
		List<PathData> pathDatas = new List<PathData>();


		public DrawEntity_Text(double left, double top, char text, Font font, int height, Color color)
		{
			pointLeftTop.x = left;
			pointLeftTop.y = top;
			this.text = text;
			this.font = new Font(font.FontFamily, fontHeightForGetOutline);
			this.height = height;
			this.color = color;
		}

		public override void Draw()
		{
			if (isGeneratedOutline == false)
			{
				GenerateTextOutline();

				isGeneratedOutline = true;
			}

			foreach (var item in pathDatas)
			{
				RenderPath(item);
			}
		}

		//绘制指定path
		private void RenderPath(PathData data)
		{
			bool isClosewise = data.IsCloseWise();
			Color renderColor = color;
			if (isClosewise)
			{
				renderColor = Color.Green;
			}
			else
			{
				renderColor = Color.Red;
			}

			//遍历曲线数据
			foreach (var item in data.listDatas)
			{
				//绘制直线
				if (item is PathItemLine)
				{
					var lineData = item as PathItemLine;
					DrawLine(new Vector(lineData.StartPoint.x + pointLeftTop.x, lineData.StartPoint.y + pointLeftTop.y, 0), new Vector(lineData.EndPoint.x + pointLeftTop.x, lineData.EndPoint.y + pointLeftTop.y, 0), 1, renderColor);
				}
                //绘制圆弧
                else if (item is PathItemArc)
				{
					var arcData = item as PathItemArc;
					//计算凸度
					double bulge = Math.Tan(arcData.SweepAngle / 180 * Math.PI / 4);
					//计算圆弧圆心
					double centerX, centerY, radius, startAngle, sweepAngle;
					TranslateArcFromBulge(arcData.StartPoint.x, arcData.StartPoint.y, arcData.EndPoint.x, arcData.EndPoint.y, bulge, out centerX, out centerY, out radius, out startAngle, out sweepAngle);

					DrawArc(new Vector(centerX + pointLeftTop.x, centerY + pointLeftTop.y, 0), radius, startAngle, sweepAngle, width, renderColor);
				}
                //绘制Bezier
                else if (item is PathItemBezier)
				{
					var bezierData = item as PathItemBezier;
					PointF p1, p2, p3, p4;
					p1 = new PointF((float)(bezierData.StartPoint.x + pointLeftTop.x), (float)(bezierData.StartPoint.y + pointLeftTop.y));
					p2 = new PointF((float)(bezierData.ControlPoint1.x + pointLeftTop.x), (float)(bezierData.ControlPoint1.y + pointLeftTop.y));
					p3 = new PointF((float)(bezierData.ControlPoint2.x + pointLeftTop.x), (float)(bezierData.ControlPoint2.y + pointLeftTop.y));
					p4 = new PointF((float)(bezierData.EndPoint.x + pointLeftTop.x), (float)(bezierData.EndPoint.y + pointLeftTop.y));

					DrawText(new Vector(p1.X, p1.Y + 1, 0), "1", new Font("宋体", 10), 1, Color.Red);
					DrawText(new Vector(p2.X, p2.Y + 1, 0), "2", new Font("宋体", 10), 1, Color.Red);
					DrawText(new Vector(p3.X, p3.Y + 1, 0), "3", new Font("宋体", 10), 1, Color.Red);
					//DrawText(new Vector(p4.X, p4.Y + 1, 0), "4", new Font("宋体", 10), 1, Color.Red);

					//{
					//    //转换Bezier 到 多段线
					//    var lines = Hatch.PathHatch.TransBezierToLines(bezierData, 10);
					//    //绘制多段线
					//    foreach (var itemLine in lines.listDatas)
					//    {
					//        DrawLine(new Vector(itemLine.StartPoint.x + pointLeftTop.x, itemLine.StartPoint.y + pointLeftTop.y, 0),
					//            new Vector(itemLine.EndPoint.x + pointLeftTop.x, itemLine.EndPoint.y + pointLeftTop.y, 0),1, Color.DarkMagenta);

					//        DrawCircle(new Vector(itemLine.EndPoint.x + pointLeftTop.x, itemLine.EndPoint.y + pointLeftTop.y, 0), 0.1, 1, Color.Green);
					//    }
					//}


					DrawCircle(new Vector(p1.X, p1.Y, 0), 0.1, 1, Color.Red);
					//DrawCircle(new Vector(p2.X, p2.Y, 0), 0.1, 1, Color.Red);
					//DrawCircle(new Vector(p3.X, p3.Y, 0), 0.1, 1, Color.Red);
					DrawCircle(new Vector(p4.X, p4.Y, 0), 0.1, 1, Color.Red);


					DrawBezier(p1, p2, p3, p4, renderColor, width);
				}
			}

			//是否闭合
			if (data.isClosed && data.listDatas.Count > 1)
			{
				int count = data.listDatas.Count;
				//添加闭合直线
				DrawLine(new Vector(data.listDatas[0].StartPoint.x + pointLeftTop.x, data.listDatas[0].StartPoint.y + pointLeftTop.y, 0), new Vector(data.listDatas[count - 1].EndPoint.x + pointLeftTop.x, data.listDatas[count - 1].EndPoint.y + pointLeftTop.y, 0), width, renderColor);
			}
		}

		//转换通过《起点、终点、凸度》定义的圆弧
		// b = 0.5*(1/bulge - bulge)
		// centerX = 0.5*((x1+x2)-b*(y2-y1))
		// centerY = 0.5*((y1+y2)+b*(x2-x1))
		private void TranslateArcFromBulge(double startX, double startY, double endX, double endY, double bulge, out double centerX, out double centerY, out double radius, out double startAngle, out double sweepAngle)
		{

			double b = 0.5 * (1 / bulge - bulge);
			//计算圆心
			centerX = 0.5 * ((startX + endX) - b * (endY - startY));
			centerY = 0.5 * ((startY + endY) + b * (endX - startX));
			//计算圆心
			radius = Math.Sqrt(Math.Pow(centerX - startX, 2) + Math.Pow(centerY - startY, 2));

			//计算起始角
			Vector vectorCentorToStart = new Vector(startX - centerX, startY - centerY, 0);
			startAngle = Math.Atan2(vectorCentorToStart.y / radius, vectorCentorToStart.x / radius) / Math.PI * 180;

			//计算终止角
			Vector vectorCentorToEnd = new Vector(endX - centerX, endY - centerY, 0);
			double endAngle = Math.Atan2(vectorCentorToEnd.y / radius, vectorCentorToEnd.x / radius) / Math.PI * 180;

			//计算扫描角度
			sweepAngle = Math.Atan(bulge) * 4 / Math.PI * 180;


			if (bulge > 0)
			{
				sweepAngle = Math.Abs(sweepAngle);
			}
			else
			{
				sweepAngle = Math.Abs(sweepAngle) * -1;
			}

		}

		//生成文字对应的轮廓数据
		private void GenerateTextOutline()
		{
			//清除原有数据
			pathDatas.Clear();

			try
			{
				Win32.GlyphMetrics metrics = new Win32.GlyphMetrics();
				Win32.Mat2 matrix = new Win32.Mat2();
				matrix.eM11.value = 1;
				matrix.eM12.value = 0;
				matrix.eM21.value = 0;
				matrix.eM22.value = 1;
				char c = text;
				using (Bitmap b = new Bitmap(1, 1))
				{
					using (Graphics g = Graphics.FromImage(b))
					{
						IntPtr hdc = g.GetHdc();

						IntPtr prev = Win32.GDI.SelectObject(hdc, font.ToHfont());
						//获取外轮廓
						uint bufferSize = Win32.Font.GetGlyphOutline(hdc, (uint)c, Win32.GGO.BEZIER, out metrics, 0, IntPtr.Zero, ref matrix);
						IntPtr buffer = Marshal.AllocCoTaskMem((int)bufferSize);
						uint result = Win32.Font.GetGlyphOutline(hdc, (uint)c, Win32.GGO.BEZIER, out metrics, bufferSize, buffer, ref matrix);

						if (result <= 0)
						{
							return;
						}
						TTPOLYGONHEADER Header;
						TTPOLYCURVE Curve;
						POINTFX pntfx = new POINTFX();

						IntPtr ipHeader = buffer;
						IntPtr endPtr = buffer + (int)bufferSize;
						IntPtr ipCurve;

						//遍历轮廓
						while (ipHeader.ToInt32() < endPtr.ToInt32())
						{
							PathData textOutline = new PathData();
							//记录封闭属性
							textOutline.isClosed = true;
							//添加到list
							pathDatas.Add(textOutline);

							Header = (TTPOLYGONHEADER)Marshal.PtrToStructure(ipHeader, typeof(TTPOLYGONHEADER));
							//获取起点
							double startX = Header.pfxStart.x.value;
							double startY = Header.pfxStart.y.value;

							List<Vector> curvePoints = new List<Vector>();
							curvePoints.Add(new Vector(startX, startY, 0));
							//TT_POLYGON_TYPE
							if (Header.dwType == 24)
							{
								ipCurve = ipHeader + Marshal.SizeOf(typeof(TTPOLYGONHEADER));

								while (ipCurve.ToInt32() < (ipHeader + Header.cb).ToInt32())
								{
									Curve = (TTPOLYCURVE)Marshal.PtrToStructure(ipCurve, typeof(TTPOLYCURVE));
									ipCurve = ipCurve + Marshal.SizeOf(typeof(TTPOLYCURVE)) - 8;

									POINTFX[] curvePointsTemp = new POINTFX[Curve.cpfx];
									for (int i = 0; i < Curve.cpfx; i++)
									{
										curvePointsTemp[i] = (POINTFX)Marshal.PtrToStructure(ipCurve, typeof(POINTFX));
										ipCurve = ipCurve + Marshal.SizeOf(typeof(POINTFX));
										//curvePoints.Add(new Vector(curvePointsTemp[i].x.value, curvePointsTemp[i].y.value, 0));
									}
									if (Curve.wType == 1)
									{
                                           
										for (int i = 0; i < Curve.cpfx; i++)
										{
											//记录尾点
											curvePoints.Add(new Vector(curvePointsTemp[i].x.value, curvePointsTemp[i].y.value, 0));

											//添加直线
											var newLine = new PathItemLine();
											newLine.StartPoint.x = curvePoints[curvePoints.Count - 2].x;
											newLine.StartPoint.y = curvePoints[curvePoints.Count - 2].y;

											newLine.EndPoint.x = curvePoints[curvePoints.Count - 1].x;
											newLine.EndPoint.y = curvePoints[curvePoints.Count - 1].y;


											textOutline.listDatas.Add(newLine);
										}
									}
									else if (Curve.wType == 2)
									{

									}
									else if (Curve.wType == 3)
									{
										PointF[] pointsBezier = new PointF[Curve.cpfx + 1];
										pointsBezier[0].X = (float)(curvePoints[curvePoints.Count - 1].x);
										pointsBezier[0].Y = (float)curvePoints[curvePoints.Count - 1].y;


										for (int i = 0; i < Curve.cpfx; i++)
										{
											pointsBezier[i + 1].X = (float)(curvePointsTemp[i].x.value);
											pointsBezier[i + 1].Y = curvePointsTemp[i].y.value;
										}

										for (int i = 0; i < pointsBezier.Length; i = i + 3)
										{
											if (i == pointsBezier.Length - 1)
											{
												break;
											}
                                                
											var newBezier = new PathItemBezier();
											newBezier.StartPoint.x = pointsBezier[i].X;
											newBezier.StartPoint.y = pointsBezier[i].Y;
											newBezier.ControlPoint1.x = pointsBezier[i + 1].X;
											newBezier.ControlPoint1.y = pointsBezier[i + 1].Y;
											newBezier.ControlPoint2.x = pointsBezier[i + 2].X;
											newBezier.ControlPoint2.y = pointsBezier[i + 2].Y;
											newBezier.EndPoint.x = pointsBezier[i + 3].X;
											newBezier.EndPoint.y = pointsBezier[i + 3].Y;
											textOutline.listDatas.Add(newBezier);
										}

										//var newBeziers = new PathItemBeziers();
										//for (int i = 0; i < pointsBezier.Length; i++)
										//{
										//    newBeziers.points.Add(new Vector(pointsBezier[i].X, pointsBezier[i].Y,0));
										//}
										//textOutline.listDatas.Add(newBeziers);



										//记录尾点
										curvePoints.Add(new Vector(curvePointsTemp[Curve.cpfx - 1].x.value, curvePointsTemp[Curve.cpfx - 1].y.value, 0));
									}
									else
									{
										int a = 0;
									}

								}//遍历封闭轮廓中的子曲线


							}
							ipHeader = ipHeader + Header.cb;
						}//遍历封闭轮廓
					}//using graphic
				}//using bitMap
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.Message);
			}


           


			//恢复成设计高度
			foreach (var itemOutline in pathDatas)
			{
				foreach (var item in itemOutline.listDatas)
				{
					if (item is PathItemLine)
					{
						var line = item as PathItemLine;
						line.StartPoint.x *= height / fontHeightForGetOutline;
						line.StartPoint.y *= height / fontHeightForGetOutline;
						line.EndPoint.x *= height / fontHeightForGetOutline;
						line.EndPoint.y *= height / fontHeightForGetOutline;
					}
					else if (item is PathItemBezier)
					{
						var bezier = item as PathItemBezier;
						bezier.StartPoint.x *= height / fontHeightForGetOutline;
						bezier.StartPoint.y *= height / fontHeightForGetOutline;
						bezier.ControlPoint1.x *= height / fontHeightForGetOutline;
						bezier.ControlPoint1.y *= height / fontHeightForGetOutline;
						bezier.ControlPoint2.x *= height / fontHeightForGetOutline;
						bezier.ControlPoint2.y *= height / fontHeightForGetOutline;
						bezier.EndPoint.x *= height / fontHeightForGetOutline;
						bezier.EndPoint.y *= height / fontHeightForGetOutline;
					}
				}
			}



		}


		//获得轮廓数据
		public List<PathData> GetTextOutline()
		{
			if (isGeneratedOutline == false)
			{
				GenerateTextOutline();

				isGeneratedOutline = true;
			}

			return pathDatas;
		}
	}
//class
}
//namespace
