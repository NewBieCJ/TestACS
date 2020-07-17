using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using DocViewerDemo.DrawEntity;
using System.Drawing.Drawing2D;

namespace DocViewerDemo
{
    public delegate void DelegateAfterDraw(Graphics g);
    public partial class ControlDocViewer : UserControl, IDocViewer
    {
        //绘图Graphic Buffer
        public Graphics graphicBuffer = null;
        public Graphics graphicScreenBuffer = null;
        public Graphics graphicScreen = null;
        public Graphics graphicCurrent = null;
        public Bitmap bitmapCanveBuffer = null;
        public Bitmap bitmapScreenBuffer = null;
        private bool redrawAll = false;

        //标尺绘制数据
        public RulerInfo RulerInfo { get; set; }

        //边角半径
        public int BorderCornerRadius { get; set; }


        public double Scale { get; set; }
        public double TranslateX { get; set; }
        public double TranslateY { get; set; }


        //绘制数据
        private DrawEntityManager entityManager;
        //命令数据
        public Command.CommandManager commandManager;
        //对象捕捉数据
        public Snap.SnapManager snapManager;

        //鼠标MouseDown记录的位置
        private Point mouseDownPos;
        //鼠标当前位置
        private Point mouseCurrenPos;
        //鼠标是否处于按压状态
        private bool isMouseMiddleDown = false;
        //中键移动前记录的状态
        private double scale_PreMove = 1;
        private double translateX_PreMove = 0;
        private double translateY_PreMove = 0;
        //框选状态
        private bool rectSelect = false;

        //事件
        public event DelegateAfterDraw AfterDrawEvent;



        public ControlDocViewer()
        {
            InitializeComponent();

            this.MouseMove += ControlDocViewer_MouseMove;
            this.MouseDown += ControlDocViewer_MouseDown;
            this.MouseUp += ControlDocViewer_MouseUp;
            this.MouseWheel += ControlDocViewer_MouseWheel;
            this.KeyDown += ControlDocViewer_KeyDown;

            this.Paint += ControlDocViewer_Paint;
            this.SizeChanged += ControlDocViewer_SizeChanged;

            
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            RulerInfo = new RulerInfo();

            //初始化
            Initial();
        }

        

        public void Initial()
        {
            Scale = 1;
            TranslateX = 0;
            TranslateY = 0;

           

            entityManager = new DrawEntityManager();//新添空entityManager
            commandManager =  new DocViewerDemo.Command.CommandManager();
            snapManager = new DocViewerDemo.Snap.SnapManager(entityManager,this);
        }


        // 设置需要绘制的实体Manager
        public void SetEntityManager(DrawEntityManager entityManager)
        {
            this.entityManager = entityManager;
            snapManager.SetEntityManager(entityManager);
        }


        //重绘事件
        private void ControlDocViewer_Paint(object sender, PaintEventArgs e)
        {
        	RenderScreen(e.Graphics);
        }

        //窗口SizeChanged
        private void ControlDocViewer_SizeChanged(object sender, EventArgs e)
        {
            //设置Region
            if (BorderCornerRadius < 0) BorderCornerRadius = 0;
            if(BorderCornerRadius > 0)
            {
                this.Region = new Region(CreateRoundedRectangle(this.ClientRectangle, BorderCornerRadius));

            }

            //重绘
            Render(true);
        }

        //键盘按键事件
		void ControlDocViewer_KeyDown(object sender, KeyEventArgs e)
		{
			if(commandManager.CurrentCmd != null)
			{
				commandManager.OnKeyDown(e);
				Render(true);
			}
			else
			{
				if(e.KeyData == Keys.Escape)
				{
					entityManager.ClearAllSelection();
					Render(true);
				}
			}
			
			
		}
		
		
		
        //鼠标MouseUp事件
        private void ControlDocViewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isMouseMiddleDown = false;
            }
            
            if(rectSelect)
            {
            	
            	//触发选择
            	Vector firstPoint = TransFromScreenToDoc(new Vector(mouseDownPos.X,mouseDownPos.Y,0));
            	Vector secondPoint = TransFromScreenToDoc(new Vector(mouseCurrenPos.X,mouseCurrenPos.Y,0));
            	
            	
            	entityManager.SelectByRect(firstPoint,secondPoint);
            	
            	rectSelect = false;
            	//重绘
            	Render(true);

                Console.WriteLine("mouseUp");
            }
        }

        //鼠标MouseDown事件
        private void ControlDocViewer_MouseDown(object sender, MouseEventArgs e)
        {
        	var moustPointInDoc = TransFromScreenToDoc(new Vector(e.X, e.Y, 0));
        	double pickSize = TransFromScreenToDoc(8);
	            	
        	//记录操作前是否有command在执行，在函数执行过程中commandManager.CurrentCmd可能会变化
        	var currentCmd = commandManager.CurrentCmd;
	            	
        	if(e.Button == MouseButtons.Middle)
            {
            	//中键移动视图
                isMouseMiddleDown = true;
                scale_PreMove = Scale;
                translateX_PreMove = TranslateX;
                translateY_PreMove = TranslateY;

                Cursor.Current = Cursors.Hand;
            }
        	
        	
        	
        	if(currentCmd != null)
        	{
        		commandManager.OnMouseDown(e);
        	}
        	

            if(e.Button == MouseButtons.Left && currentCmd == null)
            {

            	//*************************************************************************
            	// 选中夹持点
            	//*************************************************************************
            	DrawEntity.DrawEntity entityGrip;
            	int index;
            	if(entityManager.SelectGrip(moustPointInDoc,pickSize,out entityGrip,out index))
            	{
            		Command.GripPointCommand.GripPointCommand cmd = new DocViewerDemo.Command.GripPointCommand.GripPointCommand(entityGrip,index,this);
            		commandManager.DoCommand(cmd);
            	}
            }
            
            
	           

			if(e.Button == MouseButtons.Left && commandManager.CurrentCmd == null)
            {
		    	//*************************************************************************
		    	// 选中元素
		    	//*************************************************************************
		    	//尝试单击选择元素
		    	if(entityManager.SelectByPicker(moustPointInDoc,pickSize))
			   	{
		    		Render(true);
			   	}
		    	//左键单击选择元素或开始框选
		    	else
		    	{
		    		rectSelect = true;
		    	}

			}
        	
        	
        	
            
            
            mouseDownPos = e.Location;
        }

        //鼠标移动事件
        private void ControlDocViewer_MouseMove(object sender, MouseEventArgs e)
        {
            //鼠标中键处于按压状态，触发移动
            if(isMouseMiddleDown == true)
            {
                this.TranslateX = mouseDownPos.X - e.X + translateX_PreMove;
                this.TranslateY = mouseDownPos.Y - e.Y + translateY_PreMove;

                //触发重绘
                this.Render(true);
            }
            else
            {
            	//框选中
	            if(rectSelect)
	            {
	            	//触发重绘
	            	//this.Render();
	            }
	            this.Render(false);
                Console.WriteLine("mouseMove");
            }
            
            //转发对象捕捉
            snapManager.OnMouseMove(e);
            
            //转发到Command
            commandManager.OnMouseMove(e);
            
            //记录鼠标位置
            mouseCurrenPos.X = e.X;
            mouseCurrenPos.Y = e.Y;
        }

        //鼠标滚轮事件
        private void ControlDocViewer_MouseWheel(object sender, MouseEventArgs e)
        {
            //记录缩放前比例
            double scalePre = this.Scale;
            //记录缩放前鼠标点在图纸坐标系下的位置
            var moustPointInDoc = TransFromScreenToDoc(new Vector(e.X, e.Y, 0));

            if(e.Delta>0)
            {
                this.Scale *= (1 / 0.9);
            }
            else
            {
                this.Scale *= 0.9;
            }

            //计算缩放后Translate
            TranslateX = this.Width / 2 - e.X + moustPointInDoc.x * Scale;
            TranslateY = this.Height / 2 - e.Y + moustPointInDoc.y * Scale * -1;

            //触发重绘
            this.Render(true);
        }

        
        public void RenderScreen(Graphics g)
        {
            Console.WriteLine("redrawAll = " + redrawAll);
        	try
            {
            	if(bitmapCanveBuffer ==  null)
            	{
            		//缓冲图像
               		bitmapCanveBuffer = new Bitmap(this.Width, this.Height, PixelFormat.Format24bppRgb);
               		bitmapScreenBuffer = new Bitmap(this.Width, this.Height, PixelFormat.Format24bppRgb);
               		graphicBuffer = Graphics.FromImage(bitmapCanveBuffer);
               		graphicScreenBuffer = Graphics.FromImage(bitmapScreenBuffer);
               		//graphicScreen = this.CreateGraphics();
               		
               		redrawAll = true;
            	}
            	else if(bitmapCanveBuffer.Width != this.Width || bitmapCanveBuffer.Height != this.Height)
            	{
            		//缓冲图像
               		bitmapCanveBuffer = new Bitmap(this.Width, this.Height, PixelFormat.Format24bppRgb);
               		bitmapScreenBuffer = new Bitmap(this.Width, this.Height, PixelFormat.Format24bppRgb);
               		graphicBuffer = Graphics.FromImage(bitmapCanveBuffer);
               		graphicScreenBuffer = Graphics.FromImage(bitmapScreenBuffer);
               		//graphicScreen = this.CreateGraphics();
               		
               		redrawAll = true;
            	}
                

            	if(graphicScreen == null)
            	{
            		//graphicScreen = this.CreateGraphics();
            	}
                

                
                if(redrawAll ==  true)
                {
                    //redrawAll变量复位
                    redrawAll = false;


                	//设置当前Graphic
                	graphicCurrent = graphicBuffer;
                	
	                //画背景黑色
	                graphicBuffer.Clear(Color.FromArgb(240, 248, 255));
	
	                //绘制坐标系
	                var penCoor = new Pen(Color.Red, 1);
	                penCoor.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
	                var pointOriginInScreen = TransFromDocToScreen(new Vector(0, 0, 0));
	                graphicBuffer.DrawLine(penCoor,0, (float)pointOriginInScreen.y, this.Width, (float)pointOriginInScreen.y);//横轴
	                graphicBuffer.DrawLine(penCoor, (float)pointOriginInScreen.x, 0, (float)pointOriginInScreen.x, this.Height);//纵轴
	
	                //绘制所有Entity
	                if (entityManager != null)
	                {
	                    entityManager.Draw(this);
	                }
	
	                
	                
	                //graphicScreenBuffer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		            graphicScreenBuffer.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
		            graphicScreenBuffer.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;


                    
                }
                
                //设置当前Graphic
                graphicCurrent = graphicScreenBuffer;
				//canve缓存图像绘制到屏幕缓存图像上
	            graphicScreenBuffer.DrawImage(bitmapCanveBuffer, 0, 0);
	            
	            //绘制Snap
	            if(commandManager.CurrentCmd != null)
	            {
	            	snapManager.Draw();
	            }
	            
	            //绘制Command
	            commandManager.OnPaint(graphicCurrent);
	            
	            
	            
                //绘制选择框
                if(rectSelect)
                {
                	DrawSelectRect(mouseDownPos,mouseCurrenPos,graphicScreenBuffer);
                }
	                
                
                //绘制标尺
	            DrawRuler(graphicCurrent);

                //调用AfterDraw事件
                if(AfterDrawEvent != null)
                {
                    AfterDrawEvent(graphicCurrent);
                }
				
                g.DrawImage(bitmapScreenBuffer, 0, 0);
               // graphicScreen.DrawImage(bitmapCanveBuffer, 0, 0);

                

            }
            catch (Exception ex)
            {
                Console.WriteLine("exception:"+ex.Message);
            }
        }

        //绘图函数
        public void Render(bool redrawAll= false)
        {
            //如果redrawAll== true,不修改值
            if (this.redrawAll == false)
            {
                this.redrawAll = redrawAll;
            }
        	
        	Invalidate();
        }

        
        //绘制选择框
        public void DrawSelectRect(Point firstMousePoint,Point secondMousePoint,Graphics g)
        {
        	//判断当前框选模式  从左往右选==>元素必须在框内;  从右往左选==>元素只需部分在款内
        	if(firstMousePoint.X < secondMousePoint.X)
        	{
        		float x = (float)firstMousePoint.X;
        		float y = (float)(firstMousePoint.Y<secondMousePoint.Y?firstMousePoint.Y:secondMousePoint.Y);
        		float width = (float)Math.Abs(secondMousePoint.X - firstMousePoint.X );
        		float height = (float)Math.Abs(secondMousePoint.Y - firstMousePoint.Y );
        		//绘制填充框
        		Brush brush = new SolidBrush(Color.FromArgb(25,0,255,255));
				g.FillRectangle(brush,x,y,width,height);
				//绘制Border
				g.DrawRectangle(new Pen(Color.Black),x,y,width,height);
				
        	}
        	else
        	{
        		float x = (float)secondMousePoint.X;
        		float y = (float)(firstMousePoint.Y<secondMousePoint.Y?firstMousePoint.Y:secondMousePoint.Y);
        		float width = (float)Math.Abs(secondMousePoint.X - firstMousePoint.X );
        		float height = (float)Math.Abs(secondMousePoint.Y - firstMousePoint.Y );
        		//绘制填充框
        		Brush brush = new SolidBrush(Color.FromArgb(25,0,255,0));
				g.FillRectangle(brush,x,y,width,height);
				//绘制Border
				Pen newPen = new Pen(Color.Black);
				newPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
				g.DrawRectangle(newPen,x,y,width,height);
        	}
        	
        	
        }
        
        //标尺绘制函数
        public void DrawRuler(Graphics g)
        {
            //允许标尺显示的间隔
            double[] showRulerGap = new double[] { 100000000,50000000,20000000,10000000,5000000,2000000,10000000,5000000,2000000,1000000,500000,200000,100000,50000,20000,10000,5000,2000,1000,500,200,100,50,20,10,5,2,1,0,0.5,0.2,0.1,0.05,0.02,0.01,0.005,0.002,0.001,0.0005,0.0002,0.0001,0.00005,0.00002,0.00001,0.000005,0.000002,0.000001,0.0000005,0.0000002,0.0000001};
            //标尺宽度(像素)
            int rulerWidth = RulerInfo.rulerWidth;
            //标尺背景颜色
            Color rulerBackcolor = RulerInfo.rulerBackcolor;
            //标尺文字颜色
            Color rulerTextColor = RulerInfo.rulerTextColor;
            //长刻度颜色
            Color longMarkColor = RulerInfo.longMarkColor;
            //短刻度颜色
            Color shortMarkColor = RulerInfo.shortMarkColor;
            //标尺文字字体
            Font rulerTextFont = RulerInfo.rulerTextFont;
            //标尺短刻度长度
            int shortMarkLength = RulerInfo.shortMarkLength;
            //标尺长刻度长度
            int longMarkLength = RulerInfo.longMarkLength;


            //获得屏幕宽度、高度
            int screenWidth = this.Width;
            int screenHeight = this.Height;
            //获得鼠标位置
            var mousePointInScreen = mouseCurrenPos;
            //获取屏幕左上角在图纸坐标中的位置
            var leftTopInDoc = TransFromScreenToDoc(new Vector(0, 0, 0));
            //获取屏幕右上角在图纸坐标中的位置
            var rightTopInDoc = TransFromScreenToDoc(new Vector(screenWidth, 0, 0));
            //获取名目左下角在图纸坐标中的位置
            var leftBottomInDoc = TransFromScreenToDoc(new Vector(0, screenHeight, 0));
            //估计屏幕显示的图纸的尺度
            double screenWidthInDoc = (rightTopInDoc - leftTopInDoc).x;
            //估计100个像素显示的尺度
            double pixel_100_InDoc = TransFromScreenToDoc(100);
            //从showRulerGap挑选出标尺上的间隔
            double minDis = double.MaxValue;
            double bestRulerGap = showRulerGap[0];
            for (int i = 0; i < showRulerGap.Length; i++)
            {
                double dis = Math.Abs(pixel_100_InDoc - showRulerGap[i]);
                if (dis < minDis)
                {
                    minDis = dis;
                    bestRulerGap = showRulerGap[i];
                }
            }
            //计算每个长刻度间距对应的像素
            int rulerGapInScreen = (int)Math.Round(TransFromDocToScreen(bestRulerGap));//最近取整


            

            //***********************************************************************
            //绘制X标尺
            //***********************************************************************
            //绘制背景
            g.FillRectangle(new SolidBrush(rulerBackcolor), new RectangleF(rulerWidth, 0, screenWidth, rulerWidth));
            //从左开始搜索长刻度对应的像素
            int index = rulerWidth;
            while(index<screenWidth)
            {
                if(Math.Abs(TransFromScreenToDoc(new Vector(index,0,0)).x % bestRulerGap)< TransFromScreenToDoc(1))
                {
                    //绘制长标志
                    g.DrawLine(new Pen(longMarkColor), index, rulerWidth - longMarkLength, index, rulerWidth-1);
                    //显示文字
                    string longMarkString = (TransFromScreenToDoc(new Vector(index, 0, 0)).x - TransFromScreenToDoc(new Vector(index, 0, 0)).x % bestRulerGap).ToString();
                    //计算文字宽度 
                    int textLength = (int)g.MeasureString(longMarkString, rulerTextFont).Width;
                    //int textLength = (int)TextRenderer.MeasureText(g, longMarkString, rulerTextFont, new Size(0, 0), TextFormatFlags.NoPadding).Width;
                    //绘制文字
                    g.DrawString(longMarkString, rulerTextFont, new SolidBrush(rulerTextColor), index - textLength / 2, -1);
                    //累加
                    index += (rulerGapInScreen - 3);
                }
                else
                {
                    index++;
                }
                
                if(index>=screenWidth)
                {
                    break;
                }
            }

            //从左开始搜索短刻度对应的像素
            index = rulerWidth;
            while (index < screenWidth)
            {
                if (Math.Abs(TransFromScreenToDoc(new Vector(index, 0, 0)).x % (bestRulerGap/10)) < TransFromScreenToDoc(1))
                {
                    //绘制长标志
                    g.DrawLine(new Pen(longMarkColor), index, rulerWidth - shortMarkLength, index, rulerWidth - 1);
                    //累加
                    index += (rulerGapInScreen/10 - 1);
                }
                else
                {
                    index++;
                }

                if (index >= screenWidth)
                {
                    break;
                }
            }

            //从左开始搜索中刻度对应的像素（两个长刻度中的第5个短刻度）
            index = rulerWidth;
            while (index < screenWidth)
            {
                if (Math.Abs(TransFromScreenToDoc(new Vector(index, 0, 0)).x % (bestRulerGap / 2)) < TransFromScreenToDoc(1))
                {
                    //绘制长标志
                    g.DrawLine(new Pen(longMarkColor), index, rulerWidth - shortMarkLength - 5, index, rulerWidth - 1);

                    //累加
                    index += (rulerGapInScreen / 2 - 1);
                }
                else
                {
                    index++;
                }

                if (index >= screenWidth)
                {
                    break;
                }
            }




            //绘制鼠标位置
            g.DrawLine(Pens.White, mouseCurrenPos.X, 0, mouseCurrenPos.X, rulerWidth);

            //***********************************************************************
            //绘制Y标尺
            //***********************************************************************
            g.FillRectangle(new SolidBrush(rulerBackcolor), new RectangleF(0, rulerWidth, rulerWidth, screenHeight));
            //从上开始搜索长刻度对应的像素
            index = rulerWidth;
            while (index < screenHeight)
            {
                if (Math.Abs(TransFromScreenToDoc(new Vector(0, index, 0)).y % bestRulerGap) < TransFromScreenToDoc(1))
                {
                    //绘制长标志
                    g.DrawLine(new Pen(longMarkColor), rulerWidth - longMarkLength,index , rulerWidth - 1, index);
                    //显示文字
                    string longMarkString = (TransFromScreenToDoc(new Vector(0, index, 0)).y - TransFromScreenToDoc(new Vector(0, index, 0)).y % bestRulerGap).ToString();
                    //计算文字宽度 
                    int textLength = (int)g.MeasureString(longMarkString, rulerTextFont).Width;
                    //int textLength = (int)TextRenderer.MeasureText(g, longMarkString, rulerTextFont, new Size(0, 0), TextFormatFlags.NoPadding).Width;
                    //绘制文字
                    //g.DrawString(longMarkString, rulerTextFont, new SolidBrush(rulerTextColor),  -1, index - textLength / 2);
                    float showX, showY;
                    showX = -1;
                    showY = index + textLength / 2 ;
                    g.TranslateTransform(showX, showY);
                    g.RotateTransform(-90);

                    g.DrawString(longMarkString, rulerTextFont, new SolidBrush(rulerTextColor), new PointF(0, 0));
                    g.ResetTransform();


                    //累加
                    index += (rulerGapInScreen - 3);
                }
                else
                {
                    index++;
                }

                if (index >= screenHeight)
                {
                    break;
                }
            }

            //从上开始搜索短刻度对应的像素
            index = rulerWidth;
            while (index < screenHeight)
            {
                if (Math.Abs(TransFromScreenToDoc(new Vector(0, index, 0)).y % (bestRulerGap / 10)) < TransFromScreenToDoc(1))
                {
                    //绘制长标志
                    g.DrawLine(new Pen(longMarkColor), rulerWidth - shortMarkLength, index, rulerWidth - 1,index);

                    //累加
                    index += (rulerGapInScreen / 10 - 1);
                }
                else
                {
                    index++;
                }

                if (index >= screenHeight)
                {
                    break;
                }
            }

            //从上开始搜索中刻度对应的像素（两个长刻度中的第5个短刻度）
            index = rulerWidth;
            while (index < screenHeight)
            {
                if (Math.Abs(TransFromScreenToDoc(new Vector(0, index, 0)).y % (bestRulerGap / 2)) < TransFromScreenToDoc(1))
                {
                    //绘制长标志
                    g.DrawLine(new Pen(longMarkColor), rulerWidth - shortMarkLength -5, index, rulerWidth - 1, index);

                    //累加
                    index += (rulerGapInScreen / 2 - 1);
                }
                else
                {
                    index++;
                }

                if (index >= screenHeight)
                {
                    break;
                }
            }

            //绘制鼠标位置
            g.DrawLine(Pens.White, 0, mouseCurrenPos.Y,  rulerWidth,mouseCurrenPos.Y);

            //***********************************************************************
            //绘制左上角遮盖区域
            //***********************************************************************
            g.FillRectangle(new SolidBrush(Color.White), new RectangleF(0, 0, rulerWidth, rulerWidth));
        }
        


        /*
            图纸坐标系到窗口坐标系的转换
            默认状态：图纸坐标原点对应窗口中心
            图纸X轴对应窗口X轴，方向相同
            图纸Y轴对应窗口Y轴，方向相反
        */
        public Vector TransFromDocToScreen(Vector pointInDoc)
        {
            double screenWidth = this.Width;
            double screenHeight = this.Height;

            double xInScreen = pointInDoc.x * Scale - TranslateX + screenWidth / 2;
            double yInScreen = pointInDoc.y * Scale * -1 - TranslateY + screenHeight/2;

            return new Vector(xInScreen, yInScreen, 0);
        }

        /*
            图纸长度到窗口长度的转换
            默认状态：图纸坐标原点对应窗口中心
            图纸X轴对应窗口X轴，方向相同
            图纸Y轴对应窗口Y轴，方向相反
        */
        public double TransFromDocToScreen(double  lengthInDoc)
        {
            return lengthInDoc * Scale;
        }


        /*
            窗口坐标系到图纸坐标系的转换
            默认状态：图纸坐标原点对应窗口中心
        */
        public Vector TransFromScreenToDoc(Vector pointInScreen)
        {
            double screenWidth = this.Width;
            double screenHeight = this.Height;

            double xInDoc = (pointInScreen.x - screenWidth / 2 + TranslateX) / Scale;
            double yInDoc = (pointInScreen.y - screenHeight / 2 + TranslateY) / Scale * -1;

            return new Vector(xInDoc,yInDoc,0);
        }

        /*
            窗口长度到图纸长度的转换
            默认状态：图纸坐标原点对应窗口中心
        */
        public double TransFromScreenToDoc(double lengthInScreen)
        {
            return lengthInScreen / Scale;
        }

        //绘制直线
        public void DrawLine(double startX, double startY, double endX, double endY, Color color, int width)
        {
            if (graphicCurrent == null) return;
            //转换坐标
            Vector pointStartInDoc = new Vector(startX, startY, 0);
            Vector pointEndInDoc = new Vector(endX, endY, 0);
            var pointStartInScreen = TransFromDocToScreen(pointStartInDoc);
            var pointEndInScreen = TransFromDocToScreen(pointEndInDoc);

            graphicCurrent.DrawLine(
                new Pen(color,width),
                (float)pointStartInScreen.x, (float)pointStartInScreen.y, 
                (float)pointEndInScreen.x, (float)pointEndInScreen.y);
        }

        //绘制圆
        public void DrawCircle(double centerX, double centerY, double radius, Color color, int width)
        {
            if (graphicCurrent == null) return;
            //转换坐标
            Vector pointCenterInDoc = new Vector(centerX, centerY, 0);
            var pointCenterInScreen = TransFromDocToScreen(pointCenterInDoc);
            double radiusInScreen = TransFromDocToScreen(radius);

            if (pointCenterInScreen.x + radiusInScreen < 0 || pointCenterInScreen.x - radiusInScreen > this.Width || pointCenterInScreen.y + radiusInScreen < 0 || pointCenterInScreen.y - radiusInScreen > this.Height)
            {
                return;
            }

            if (radiusInScreen <= 1)
            {
                graphicCurrent.DrawLine(new Pen(color, width), (int)(pointCenterInScreen.x-0.5), (int)pointCenterInScreen.y, (int)(pointCenterInScreen.x+0.5), (int)pointCenterInScreen.y);
            }
            else
            {
                graphicCurrent.DrawArc(
                    new Pen(color, width),
                    (float)(pointCenterInScreen.x - radiusInScreen), (float)(pointCenterInScreen.y - radiusInScreen),
                    (float)radiusInScreen * 2,
                    (float)radiusInScreen * 2, 0, 360);
            }
            
        }

        //绘制圆弧
        public void DrawArc(double centerX, double centerY, double radius, double startAngle, double sweepAngle, Color color, int width)
        {
            if (graphicCurrent == null) return;
            //转换坐标
            Vector pointCenterInDoc = new Vector(centerX, centerY, 0);
            var pointCenterInScreen = TransFromDocToScreen(pointCenterInDoc);
            double radiusInScreen = TransFromDocToScreen(radius);

            if (radiusInScreen <= 1)
            {
                graphicCurrent.DrawLine(new Pen(color, width), (int)(pointCenterInScreen.x - 0.5), (int)pointCenterInScreen.y, (int)(pointCenterInScreen.x + 0.5), (int)pointCenterInScreen.y);
            }
            else
            {
                graphicCurrent.DrawArc(
                new Pen(color, width),
                (float)(pointCenterInScreen.x - radiusInScreen), (float)(pointCenterInScreen.y - radiusInScreen),
                (float)radiusInScreen * 2,
                (float)radiusInScreen * 2, (float)startAngle * -1, (float)sweepAngle * -1);
            }
        }

        //绘制文字
        public void DrawText(double left, double top, string text, Font font, double height ,Color color)
        {
            if (graphicCurrent == null) return;

            //转换坐标
            Vector pointLeftTopInDoc = new Vector(left, top, 0);
            var pointLeftTopInScreen = TransFromDocToScreen(pointLeftTopInDoc);
            double heightInScreen = TransFromDocToScreen(height);

            Font fontClone = new Font(font.FontFamily, (float)heightInScreen, font.Style, GraphicsUnit.Pixel);
            graphicCurrent.DrawString(text, fontClone, new SolidBrush(color), (float)pointLeftTopInScreen.x, (float)pointLeftTopInScreen.y);
            
        }

        //绘制Bezier
        public void DrawBezier(PointF p1, PointF p2, PointF p3, PointF p4, Color color, int width)
        {
            if (graphicCurrent == null) return;

            PointF p1InScreen = new PointF(), p2InScreen = new PointF(), p3InScreen = new PointF(), p4InScreen = new PointF();
            Vector pointInScreen;
            Vector pointInDoc;
            //转换坐标
            pointInDoc = new Vector(p1.X, p1.Y, 0);
            pointInScreen = TransFromDocToScreen(pointInDoc);
            p1InScreen.X = (float)pointInScreen.x;
            p1InScreen.Y = (float)pointInScreen.y;

            pointInDoc = new Vector(p2.X, p2.Y, 0);
            pointInScreen = TransFromDocToScreen(pointInDoc);
            p2InScreen.X = (float)pointInScreen.x;
            p2InScreen.Y = (float)pointInScreen.y;

            pointInDoc = new Vector(p3.X, p3.Y, 0);
            pointInScreen = TransFromDocToScreen(pointInDoc);
            p3InScreen.X = (float)pointInScreen.x;
            p3InScreen.Y = (float)pointInScreen.y;

            pointInDoc = new Vector(p4.X, p4.Y, 0);
            pointInScreen = TransFromDocToScreen(pointInDoc);
            p4InScreen.X = (float)pointInScreen.x;
            p4InScreen.Y = (float)pointInScreen.y;

            graphicCurrent.DrawBezier(new Pen(color), p1InScreen, p2InScreen, p3InScreen, p4InScreen);
        }


        //绘制Bezier
        public void DrawBeziers(PointF[] points, Color color, int width)
        {
            if (graphicCurrent == null) return;

            PointF[] pointsInScreen = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Vector pointInDoc = new Vector(points[i].X,points[i].Y,0);
                Vector pointInScreen = TransFromDocToScreen(pointInDoc);
                pointsInScreen[i].X = (float)pointInScreen.x;
                pointsInScreen[i].Y = (float)pointInScreen.y;
            }
            graphicCurrent.DrawBeziers(new Pen(color), pointsInScreen);
        }
        
        
        //绘制矩形框
        public void DrawRect(float left,float top,float right,float bottom,Color color,int width)
        {
        	if (graphicCurrent == null) return;
        	
        	Vector leftTopInScreen;
            Vector leftTopInDoc;
            Vector rightBottomInScreen;
            Vector rightBottomInDoc;
            
            //转换坐标
            leftTopInDoc = new Vector(left, top, 0);
            leftTopInScreen = TransFromDocToScreen(leftTopInDoc);
			rightBottomInDoc = new Vector(right, bottom, 0);
            rightBottomInScreen = TransFromDocToScreen(rightBottomInDoc);
            if (leftTopInScreen.x > this.Width || leftTopInScreen.y>this.Height || rightBottomInScreen.x<0 || rightBottomInScreen.y <0) return;
            
            graphicCurrent.DrawRectangle(new Pen(color,width),(float)leftTopInScreen.x,(float)leftTopInScreen.y,(float)(rightBottomInScreen.x - leftTopInScreen.x),(float)(rightBottomInScreen.y - leftTopInScreen.y));
        }
        
        
        
        
        //绘制填充矩形框
        public void FillRect(float left,float top,float right,float bottom,Brush brush)
        {
        	if (graphicCurrent == null) return;
        	
        	Vector leftTopInScreen;
            Vector leftTopInDoc;
            Vector rightBottomInScreen;
            Vector rightBottomInDoc;
            
            //转换坐标
            leftTopInDoc = new Vector(left, top, 0);
            leftTopInScreen = TransFromDocToScreen(leftTopInDoc);
			rightBottomInDoc = new Vector(right, bottom, 0);
            rightBottomInScreen = TransFromDocToScreen(rightBottomInDoc);
            
            graphicCurrent.FillRectangle(brush,(float)leftTopInScreen.x,(float)leftTopInScreen.y,(float)(rightBottomInScreen.x - leftTopInScreen.x),(float)(rightBottomInScreen.y - leftTopInScreen.y));
        }
        
        
        //绘制操作矩形框
        public void DrawHandleRect(Vector pointInDoc,Color fillColor,Color borderColor,int rectSizeInScreen)
        {
        	if (graphicCurrent == null) return;
        	
        	Vector pointInScreen;
        	//转换坐标
        	pointInScreen = TransFromDocToScreen(pointInDoc);
        	
        	int half = rectSizeInScreen/2;

            //
            if (pointInScreen.x + half < 0 || pointInScreen.x - half > this.Width || pointInScreen.y + half <0 || pointInScreen.y -half >this.Height)
            {
                return;
            }



            //绘制填充框
            graphicCurrent.FillRectangle(new SolidBrush(fillColor),(float)(pointInScreen.x - half),(float)(pointInScreen.y - half),rectSizeInScreen,rectSizeInScreen);
			//绘制Border
			graphicCurrent.DrawRectangle(new Pen(borderColor),(float)(pointInScreen.x - half),(float)(pointInScreen.y - half),rectSizeInScreen,rectSizeInScreen);			
        }

        //创建圆角矩形Path
        GraphicsPath CreateRoundedRectangle(Rectangle rect, int cornerRadius)
        {
            GraphicsPath path = new GraphicsPath();

            //左上角圆角
            path.AddArc(rect.Left, rect.Top, cornerRadius * 2, cornerRadius * 2, 180, 90);
            //上侧直线
            path.AddLine(rect.Left + cornerRadius, rect.Top, rect.Right - cornerRadius, rect.Top);
            //右上角圆角
            path.AddArc(rect.Right - cornerRadius * 2, rect.Top, cornerRadius * 2, cornerRadius * 2, 270, 90);
            //右侧直线
            path.AddLine(rect.Right, rect.Top + cornerRadius, rect.Right, rect.Bottom - cornerRadius);
            //右下角圆角
            path.AddArc(rect.Right - cornerRadius * 2, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            //下侧直线
            path.AddLine(rect.Left + cornerRadius, rect.Bottom, rect.Right - cornerRadius, rect.Bottom);
            //左下角直线
            path.AddArc(rect.Left, rect.Bottom - 2 * cornerRadius, cornerRadius * 2, cornerRadius * 2, 90, 90);
            //左侧直线
            path.AddLine(rect.Left, rect.Top + cornerRadius, rect.Left, rect.Bottom - cornerRadius);
            return path;
        }


    }//class
    
    //框选模式
    public enum SelectMode
    {
    	// 闭合框选,物体完全在选择矩形框内则选中物体
        Window = 1,
        // 交叉框选,物体与选择矩形框有交集则选中物体
        Cross = 2,
    }


    //标尺数据
    public class RulerInfo
    {
        //标尺宽度(像素)
        public int rulerWidth = 22;
        //标尺背景颜色
        public Color rulerBackcolor = Color.FromArgb(220, 228, 255);
        //标尺文字颜色
        public Color rulerTextColor = Color.Black;
        //长刻度颜色
        public Color longMarkColor = Color.Black;
        //短刻度颜色
        public Color shortMarkColor = Color.Black;
        //标尺文字字体
        public Font rulerTextFont = new Font("Microsoft YaHei", 7);
        //标尺短刻度长度
        public int shortMarkLength = 5;
        //标尺长刻度长度
        public int longMarkLength = 10;
    }

}//namespace
