using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace GIS.TreeIndex.VectorSmallPolygonsMerge
{
    public class SmallPolygonsMerge// : IDisposable
    {

        string[] strNames;
        double area;
        frmSmallPolygonsMergeProgress m_frmProgressBar;
        private static int sp = 0;

        public SmallPolygonsMerge(string []strs, double area, frmSmallPolygonsMergeProgress fm)
        {
            strNames = strs;
            this.area = area;
            m_frmProgressBar = fm;
        }

        [DllImport("IncrementUpdateDll_haiou.dll")]
        public static extern int UninTouchSmallPolygon(string filename, double filterArea);

        public void Work()
        {
            try
            {
                int count = strNames.Length;
                ThreadPool.SetMaxThreads(10, 5);  //最大开启3个线程
                ManualResetEvent[] _ManualEvents = new ManualResetEvent[count];
                for (int i = 0; i < count; i++)
                {
                    _ManualEvents[i] = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(WorkSmallPolygons), new object[] { strNames[i], _ManualEvents[i] });
                }

                WaitHandle.WaitAll(_ManualEvents);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.ToString());
            }
        }

        private void WorkSmallPolygons(object parameter)
        {
            object[] parameters = (object[])parameter;
            string filename = (string)parameters[0];
            ManualResetEvent manualResetEvent = (ManualResetEvent)parameters[1];

            HandleUnionSmallPolygons husp = new HandleUnionSmallPolygons(filename, area);
            //husp.RemoveHoles();
            husp.Work();
            husp.Dispose();

            manualResetEvent.Set();
            sp++;

            int step = Convert.ToInt32(sp * 100 / strNames.Length);
            SetProgressBar(step);
        }

        private void SetProgressBar(int value)
        {
            m_frmProgressBar.progressBar1.Invoke(
           (MethodInvoker)delegate()
           {
               m_frmProgressBar.progressBar1.Value = value;
           }
           );
        }

    }
}
