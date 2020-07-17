using DocViewerDemo.DrawEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace DocViewerDemo.Barcode
{
    public class DataMatrix
    {
        public ZXing.Common.BitMatrix GetBitMatrix(string code)
        {
            //ZXing.QrCode.QRCodeWriter writer = new ZXing.QrCode.QRCodeWriter();
            //var bitMatrix = writer.encode(code, ZXing.BarcodeFormat.QR_CODE, 1, 1);
            ZXing.Datamatrix.DataMatrixWriter writer = new ZXing.Datamatrix.DataMatrixWriter();
            IDictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
            hints.Add(EncodeHintType.MIN_SIZE, new Dimension(20,20));           //指定码大小，10*10至144*144 的偶数
            hints.Add(EncodeHintType.DATA_MATRIX_SHAPE, ZXing.Datamatrix.Encoder.SymbolShapeHint.FORCE_SQUARE);     //指定码为正方形
            var bitMatrix = writer.encode(code, ZXing.BarcodeFormat.DATA_MATRIX, 1, 1, hints);
            return bitMatrix;
        }

        public List<PathData> GetDotOutLine(string code)
        {
            List<PathData> result = new List<PathData>();
            var bitMatrix = GetBitMatrix(code);
  

            //生成外框矩形到dxf文件
            for (int w = 0; w < bitMatrix.Width; w++)
            {
                for (int h = 0; h < bitMatrix.Height; h++)
                {
                    if (bitMatrix[w, h] == false)
                    {
                        continue;
                    }

                    PathData newPathData = new PathData();
                    PathItemLine lineLeft = new PathItemLine();
                    PathItemLine lineTop = new PathItemLine();
                    PathItemLine lineRight = new PathItemLine();
                    PathItemLine lineBottom = new PathItemLine();

                    int x = w;
                    int y = bitMatrix.Height - h;

                    lineLeft.StartPoint.x = x; lineLeft.StartPoint.y = y; lineLeft.EndPoint.x = x; lineLeft.EndPoint.y = y+1;
                    lineTop.StartPoint.x = x; lineTop.StartPoint.y = y+1; lineTop.EndPoint.x = x+1; lineTop.EndPoint.y = y + 1;
                    lineRight.StartPoint.x = x+1; lineRight.StartPoint.y = y+1; lineRight.EndPoint.x = x+1; lineRight.EndPoint.y = y;
                    lineBottom.StartPoint.x = x+1; lineBottom.StartPoint.y = y; lineBottom.EndPoint.x = x; lineBottom.EndPoint.y = y;

                    newPathData.listDatas.Add(lineLeft);
                    newPathData.listDatas.Add(lineTop);
                    newPathData.listDatas.Add(lineRight);
                    newPathData.listDatas.Add(lineBottom);
                    result.Add(newPathData);
                }
            }

            return result;
        }

        public List<PathData> GetCircleOutLine(string code)
        {
            List<PathData> result = new List<PathData>();
            var bitMatrix = GetBitMatrix(code);


            //生成外框矩形到dxf文件
            for (int w = 0; w < bitMatrix.Width; w++)
            {
                for (int h = 0; h < bitMatrix.Height; h++)
                {
                    if (bitMatrix[w, h] == false)
                    {
                        continue;
                    }

                    PathData newPathData = new PathData();
                    PathItemArc arc1 = new PathItemArc();
                    PathItemArc arc2 = new PathItemArc();

                    int x = w;
                    int y = bitMatrix.Height - h;
                    arc1.StartPoint.x = x;
                    arc1.StartPoint.y = y;
                    arc1.SweepAngle = 180;

                    arc1.EndPoint.x = x;
                    arc1.EndPoint.y = y + 0.5;

                    arc2.EndPoint.x = x;
                    arc2.EndPoint.y = y;
                    arc2.SweepAngle = 180;

                    arc2.StartPoint.x = x;
                    arc2.StartPoint.y = y + 0.5;


                    newPathData.listDatas.Add(arc1);
                    newPathData.listDatas.Add(arc2);
                    result.Add(newPathData);
                }
            }

            return result;
        }


        public List<PathData> GetOutlines(string code,double height)
        {
            List<PathData> result = new List<PathData>();


            var bitMatrix = GetBitMatrix(code);

            OpenCvSharp.Mat mat = new OpenCvSharp.Mat(new OpenCvSharp.Size(bitMatrix.Width*10, bitMatrix.Height*10), OpenCvSharp.MatType.CV_8U);
            for (int x = 0; x < bitMatrix.Width*10; x++)
            {
                for (int y = 0; y < bitMatrix.Height*10; y++)
                {
                    mat.Set(y, x, 0);
                }
            }
            for (int x = 0; x < bitMatrix.Width; x++)
            {
                for (int y = 0; y < bitMatrix.Height; y++)
                {
                    for (int n = 0; n < 10; n++)
                    {
                        for (int m = 0; m < 10; m++)
                        {
                            int row = (bitMatrix.Height - 1 - y) * 10;
                            int col = x * 10;
                            mat.Set(row + m, col + n, bitMatrix[x, y] ? (byte)255 : (byte)0);
                        }
                    }
                   
                }
            }

            mat.SaveImage("test.bmp");

            OpenCvSharp.HierarchyIndex[] dierarchyIndex;
            OpenCvSharp.Point[][] contours;
            OpenCvSharp.Cv2.FindContours(mat,out contours, out dierarchyIndex, OpenCvSharp.RetrievalModes.List, OpenCvSharp.ContourApproximationModes.ApproxSimple);

            //转换Opencv获得的轮廓
            for (int i = 0; i < contours.Length; i++)
            {
                PathData newPathData = new PathData();
                newPathData.isClosed = true;
                for (int j = 1; j < contours[i].Length; j++)
                {
                    PathItemLine newLine = new PathItemLine();
                    newLine.StartPoint.x = contours[i][j - 1].X;
                    newLine.StartPoint.y = contours[i][j - 1].Y;
                    newLine.EndPoint.x = contours[i][j].X;
                    newLine.EndPoint.y = contours[i][j].Y;
                    newPathData.listDatas.Add(newLine);
                }

                result.Add(newPathData);
            }

            //处理轮廓，将轮廓中点数据取整（例如:9.5==>10  ;10.5==>10）
            foreach (var item in result)
            {
                foreach (var itemLine in item.listDatas)
                {
                    itemLine.StartPoint.x = Math.Round(itemLine.StartPoint.x/10)*10;
                    itemLine.StartPoint.y = Math.Round(itemLine.StartPoint.y/10)*10;
                    itemLine.EndPoint.x = Math.Round(itemLine.EndPoint.x/10)*10;
                    itemLine.EndPoint.y = Math.Round(itemLine.EndPoint.y/10)*10;
                }
            }

            //处理轮廓，去除长度==0 的线段
            foreach (var item in result)
            {
                for (int i = item.listDatas.Count -1; i >=0 ; i--)
                {
                    var startPoint = item.listDatas[i].StartPoint;
                    var endPoint = item.listDatas[i].EndPoint;
                    if (startPoint.x == endPoint.x && startPoint.y == endPoint.y)
                    {
                        item.listDatas.RemoveAt(i);
                    }
                }
            }

            //恢复轮廓尺寸
            foreach (var item in result)
            {
                foreach (var itemLine in item.listDatas)
                {
                    itemLine.StartPoint.x = itemLine.StartPoint.x / 10  / bitMatrix.Width * height;
                    itemLine.StartPoint.y = itemLine.StartPoint.y / 10 / bitMatrix.Width * height;
                    itemLine.EndPoint.x = itemLine.EndPoint.x / 10 / bitMatrix.Height * height;
                    itemLine.EndPoint.y = itemLine.EndPoint.y / 10 / bitMatrix.Height * height;
                }
            }

            return result;
        }


        public List<PathData> GetOutlines_New(string code)
        {
            List<PathData> result = new List<PathData>();


            var bitMatrix = GetBitMatrix(code);

            OpenCvSharp.Mat mat = new OpenCvSharp.Mat(new OpenCvSharp.Size(bitMatrix.Width, bitMatrix.Height), OpenCvSharp.MatType.CV_8UC1);
            for (int x = 0; x < bitMatrix.Width; x++)
            {
                for (int y = 0; y < bitMatrix.Height; y++)
                {
                    mat.Set(y, x, 0);
                }
            }
            for (int x = 0; x < bitMatrix.Width; x++)
            {
                for (int y = 0; y < bitMatrix.Height; y++)
                {
                    if(bitMatrix[x, y])
                    {
                        mat.Set(y, x, (byte)200);
                    }   
                }
            }

            mat.SaveImage("test.bmp");

            //计算连通域
            OpenCvSharp.Mat matLabel = new OpenCvSharp.Mat(new OpenCvSharp.Size(bitMatrix.Width, bitMatrix.Height), OpenCvSharp.MatType.CV_8U);
            OpenCvSharp.OutputArray label = OpenCvSharp.OutputArray.Create(matLabel);
            OpenCvSharp.Cv2.ConnectedComponents(mat, label, OpenCvSharp.PixelConnectivity.Connectivity4);

            OpenCvSharp.Mat lableScale = label.GetMat().Mul(new OpenCvSharp.Mat(new OpenCvSharp.Size(bitMatrix.Width, bitMatrix.Height), OpenCvSharp.MatType.CV_32SC1, 30));
            lableScale.SaveImage("label.bmp");


           

            return result;
        }
    }//class
}//namespace
