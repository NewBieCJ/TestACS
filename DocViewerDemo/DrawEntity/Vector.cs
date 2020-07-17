using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocViewerDemo.DrawEntity
{
    public class Vector
    {
        public double x = 0;
        public double y = 0;
        public double z = 0;

        public Vector()
        {

        }

        public Vector(double x,double y,double z)
        {
            this.x  = x;
            this.y = y;
            this.z = z;
        }

        //计算向量相对于X轴的角度，逆时针为正
        public double AngleToX()
        {
            return Math.Atan2(y, x)/Math.PI*180;
        }

        // 归一
        public Vector Normalize()
        {
            Vector result = new Vector();
            double length = Math.Sqrt(x * x + y * y);
            result.x = x / length;
            result.y = y / length;
            return result;
        }


        public static Vector operator-(Vector v1,Vector v2)
        {
            Vector result = new Vector();
            result.x = v1.x - v2.x;
            result.y = v1.y - v2.y;
            result.z = v1.z = v2.z;
            return result;
        }

        public static Vector operator + (Vector v1, Vector v2)
        {
            Vector result = new Vector();
            result.x = v1.x + v2.x;
            result.y = v1.y + v2.y;
            result.z = v1.z + v2.z;
            return result;
        }

        //叉积
        public static Vector CrossProduct(Vector v1, Vector v2)
        {
            Vector result = new Vector();
            result.x = v1.y* v2.z - v1.z * v2.y;
            result.y = v1.z * v2.x - v1.x * v2.z;
            result.z = v1.x * v2.y - v1.y * v2.x;
            return result;
        }

        public static Vector operator *(double scale,Vector v1)
        {
            Vector result = new Vector();
            result.x = v1.x * scale;
            result.y = v1.y * scale;
            result.z = v1.z * scale;
            return result;
        }

    }
}
