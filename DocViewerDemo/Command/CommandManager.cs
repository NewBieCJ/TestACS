/*
 * Created by SharpDevelop.
 * User: Hewenyuan
 * Date: 2019/5/10
 * Time: 14:41
 * 
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DocViewerDemo.Command
{
	/// <summary>
	/// 命令管理器
	/// </summary>
	public class CommandManager
	{
		/// <summary>
        /// 命令列表
        /// 用于撤销和重做
        /// </summary>
        private List<Command> _undoCmds = new List<Command>();
        private List<Command> _redoCmds = new List<Command>();
        
        /// <summary>
        /// 当前命令
        /// </summary>
        private Command currentCmd = null;
        public Command CurrentCmd
        {
            get { return currentCmd; }
        }
        
		public CommandManager()
		{
		}

		
		//当前命令的结束事件
		void currentCmd_OnCommandFinished()
		{
			currentCmd = null;
		}

		//当前命令的取消事件
		void currentCmd_OnCommandCancelled()
		{
			currentCmd = null;
		}
		
		
		
		/// <summary>
        /// 执行命令
        /// </summary>
        public void DoCommand(Command cmd)
        {
            if (currentCmd != null)
            {
                return;
            }

            currentCmd = cmd;
            
            //注册事件
            currentCmd.OnCommandFinished += currentCmd_OnCommandFinished;
            currentCmd.OnCommandCancelled += currentCmd_OnCommandCancelled;
        }
        
        /// <summary>
        /// 完成当前命令
        /// </summary>
        public void FinishCurrentCommand()
        {
            if (currentCmd != null)
            {
                currentCmd.Finish();
                currentCmd = null;
            }
        }
        
        
        /// <summary>
        /// 取消当前命令
        /// </summary>
        public void CancelCurrentCommand()
        {
            if (currentCmd != null)
            {
                currentCmd.Cancel();

                currentCmd = null;
            }
        }
        
        /// <summary>
        /// Mouse Down
        /// </summary>
        public void OnMouseDown(MouseEventArgs e)
        {
            if (currentCmd != null)
            {
                currentCmd.OnMouseDown(e);
            }
        }

        /// <summary>
        /// Mouse Up
        /// </summary>
        public void OnMouseUp(MouseEventArgs e)
        {
            if (currentCmd != null)
            {
                currentCmd.OnMouseUp(e);
            }
        }

        /// <summary>
        /// Mouse Move
        /// </summary>
        public void OnMouseMove(MouseEventArgs e)
        {
            if (currentCmd != null)
            {
                currentCmd.OnMouseMove(e);
            }
        }

        /// <summary>
        /// Key Down
        /// </summary>
        public void OnKeyDown(KeyEventArgs e)
        {
            if (currentCmd != null)
            {
                EventResult eRet = currentCmd.OnKeyDown(e);
                if (eRet.status == EventResultStatus.Unhandled)
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        this.CancelCurrentCommand();
                    }
                }
            }
        }

        
        /// <summary>
        /// 绘制
        /// </summary>
        public void OnPaint(Graphics g)
        {
            if (currentCmd != null)
            {
                currentCmd.OnPaint(g);
            }
        }
	}
}
