using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocViewerDemo.DrawEntity
{
    /// <summary>
    /// 绘图管理类
    /// </summary>
    public class DrawEntityManager
    {
        //当前选中的元素集合
        public List<DrawEntity> selections = new List<DrawEntity>();

        //所有元素
        public List<DrawEntity> allEntities = new List<DrawEntity>();

        
        //绘图
        public void Draw(IDocViewer docViewer)
        {
            //遍历绘制所有元素
            foreach (var item in allEntities)
            {
                item.SetViewer(docViewer);
                item.Draw();
            }
        }
        
        /// <summary>
        /// 鼠标选中夹持点
        /// 返回鼠标附件的夹持点编号，以及夹持点所属于的元素
        /// </summary>
        /// <param name="mousePosInDoc">鼠标在Doc中的坐标</param>
        /// <param name="rectSize">选择框在Doc中的大小</param>
        /// <param name="entity">选中的元素</param>
        /// <param name="gripIndex">选中夹持点在元素夹持点列表中的编号</param>
        /// <returns>返回是否夹持点被选中</returns>
        public bool SelectGrip(Vector mousePosInDoc,double rectSize,out DrawEntity entity,out int gripIndex)
        {
        	//遍历绘制所有元素
            foreach (var item in allEntities)
            {
            	if(item.status_showHandle == false)
            	{
            		continue;
            	}
            	
            	var gripsInEntity = item.GetAllGripPoint();
            	for (int i = 0; i < gripsInEntity.Length; i++) 
            	{
            		double gripX = gripsInEntity[i].x;
            		double gripY = gripsInEntity[i].y;
            		if(gripX>mousePosInDoc.x - rectSize/2 && gripX<mousePosInDoc.x + rectSize/2
            		   && gripY>mousePosInDoc.y - rectSize/2 && gripY<mousePosInDoc.y + rectSize/2)
            		{
            			//找到夹持点
            			entity = item;
            			gripIndex = i;
            			return true;
            		}
            	}
            }
            entity = null;
            gripIndex = -1;
            return false;
        }
        
        
        //鼠标单选
        public bool SelectByPicker(Vector mousePosInDoc,double rectSize)
        {
        	Vector leftTop = new Vector(mousePosInDoc.x - rectSize/2,mousePosInDoc.y + rectSize/2,0);
    		Vector rightBottom = new Vector(mousePosInDoc.x + rectSize/2,mousePosInDoc.y - rectSize/2,0);
    			
        	//遍历绘制所有元素
            foreach (var item in allEntities)
            {
                //判断元素离鼠标最近点是否在鼠标周围
                if(item.IsIntersectionWithRect(leftTop,rightBottom) == true)
    			{
    				item.status_selected =  true;
    				item.status_showHandle = true;
    				
    				//只选择第一个元素
    				return true;
    			}
            }
            
            return false;
        }
        
        //鼠标框选元素
        public bool SelectByRect(Vector firstPoint,Vector secondPoint)
        {
        	//rect
        	float x = (float)(firstPoint.x<secondPoint.x?firstPoint.x:secondPoint.x);
    		float y = (float)(firstPoint.y>secondPoint.y?firstPoint.y:secondPoint.y);
    		float width = (float)Math.Abs(secondPoint.x - firstPoint.x );
    		float height = (float)Math.Abs(secondPoint.y - firstPoint.y );
        	
    		Vector leftTop = new Vector(x,y,0);
    		Vector rightBottom = new Vector(x+width,y-height,0);
        	
        	
        	//window
        	if(firstPoint.x<secondPoint.x)
        	{
        		foreach (var item in allEntities)
        		{
        			if(item.IsInRect(leftTop,rightBottom) == true)
        			{
        				item.status_selected =  true;
        				item.status_showHandle = true;
        			}
        		}
        	}
        	//cross
        	else
        	{
        		foreach (var item in allEntities)
        		{
        			if(item.IsInRect(leftTop,rightBottom) == true)
        			{
        				item.status_selected =  true;
        				item.status_showHandle = true;
        			}
        			else
        			{
        				if(item.IsIntersectionWithRect(leftTop,rightBottom) == true)
        				{
        					item.status_selected =  true;
        					item.status_showHandle = true;
        				}
        			}
        		}
        	}
        	
        	return false;
        }
        
        //清除所有选择
        public void ClearAllSelection()
        {
        	foreach (var item in allEntities)
    		{

    			item.status_selected =  false;
    			item.status_showHandle = false;
    		}
        }
        
        
    }//class
}//namespace
