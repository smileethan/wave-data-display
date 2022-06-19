using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;

namespace WaveDataDisplay
{
    public partial class MainForm : Form
    {
        private ArrayList m_arrData = new ArrayList(); // 数据列表
        private float m_fDataMax; // 最大值
        private float m_fDataMin; // 最小值
        Bitmap m_bmpWave; // 显示位图

        float m_fDataRangeX = 500.0f; // x轴 数据范围
        int m_nMarkCntX = 10; // x轴 刻度数量
        PointF m_ptDataOffset = new PointF(100.0f, 100.0f); // 数据偏移
        float m_fMargin = 30.0f; // 留白
        float m_fScaleDTV = 1.0f;
        float m_fMarkSpan = 1.0f;

        public MainForm()
        {
            InitializeComponent();
        }
        
        private void btnOpenfile_Click(object sender, EventArgs e)  // 加载文件按钮
        {
            m_fDataMax = float.MinValue;
            m_fDataMin = float.MaxValue;
            m_arrData = new ArrayList();

            // 读取文件
            string strFileData = "";
            try
            {
                OpenFileDialog dlgOpen = new OpenFileDialog();
                dlgOpen.ShowDialog();
                StreamReader srReader = new StreamReader(dlgOpen.FileName);
                strFileData = srReader.ReadToEnd();
                srReader.Close();
            }
            catch
            {
                MessageBox.Show("文件读取失败！", "错误");
                return;
            }

            // 数据解析
            string[] arrFileDataSplit = strFileData.Split();
            for(int i=0; i< arrFileDataSplit.Length; i++)
            {
                string strData = arrFileDataSplit[i].Trim();
                if (strData.Length == 0)
                    continue;
                float fData;
                try
                {
                    fData = float.Parse(strData);
                }
                catch
                {
                    MessageBox.Show(string.Format("解析第{0:D}个数据【{1:S}】错误，已忽略", m_arrData.Count + 1, strData));
                    continue;
                }
                m_arrData.Add(fData);
                if (fData > m_fDataMax) m_fDataMax = fData;
                if (fData < m_fDataMin) m_fDataMin = fData;
            }
            // 刷新界面
            Display();
            return;
        }

        private PointF CoordTrans_DataToView(int nViewWidth, int nViewHeigh, float fScaleDTV, // 坐标转换 从数据到视图
            PointF ptDataOffset, PointF ptViewOffset, PointF ptDataPt)
        {
            return new PointF(
                ptViewOffset.X + (nViewWidth) * 0.5f + (ptDataPt.X - ptDataOffset.X) * fScaleDTV,
                ptViewOffset.Y + (nViewHeigh) * 0.5f - (ptDataPt.Y - ptDataOffset.Y) * fScaleDTV
            );

        }
        private bool CoordTrans_DataToView(int nViewWidth, int nViewHeigh, float fScaleDTV, // 批量坐标转换 从数据到视图
            PointF ptDataOffset, PointF ptViewOffset, ref PointF[] arrDataPts)
        {
            for (int i = 0; i < arrDataPts.Length; i++)
                arrDataPts[i] = CoordTrans_DataToView(nViewWidth, nViewHeigh, fScaleDTV, 
                    ptDataOffset, ptViewOffset, arrDataPts[i]);
            return true;
        }

        private void Display() // 刷新界面
        {
            // 视图范围
            int nViewWidth = pbWave.Width;  
            int nViewHeight = pbWave.Height;
            // 比例系数 从数据到视图 
            m_fScaleDTV = (nViewWidth - m_fMargin * 2) / m_fDataRangeX;  

            // 数据坐标范围
            float fDataRangeXMin = -m_fDataRangeX / 2.0f + m_ptDataOffset.X;
            float fDataRangeXMax = m_fDataRangeX / 2.0f + m_ptDataOffset.X;
            float fDataRangeYMin = -(nViewHeight * 0.5f - m_fMargin) / m_fScaleDTV + m_ptDataOffset.Y;
            float fDataRangeYMax = (nViewHeight * 0.5f - m_fMargin) / m_fScaleDTV + m_ptDataOffset.Y;

            // 创建画布
            m_bmpWave = new Bitmap(nViewWidth, nViewHeight);
            Graphics gWave = Graphics.FromImage(m_bmpWave);
            Pen penBlkThin = new Pen(Color.Black, 1);
            Pen penRedMid = new Pen(Color.Red, 2);
            Font fontBlkThin = new Font("宋体", 12);
            SolidBrush brshBlk = new SolidBrush(Color.Black);
            // 绘制坐标轴
            // 绘制直线
            PointF[] arrPts = new PointF[2];
            arrPts[0] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(-m_fMargin, 0.0f), new PointF(fDataRangeXMin, 0.0f));
            arrPts[1] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(m_fMargin, 0.0f), new PointF(fDataRangeXMax, 0.0f));
            gWave.DrawLine(penBlkThin, arrPts[0].X, arrPts[0].Y, arrPts[1].X, arrPts[1].Y);
            arrPts[0] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(0.0f, m_fMargin), new PointF(0.0f, fDataRangeYMin));
            arrPts[1] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(0.0f, -m_fMargin), new PointF(0.0f, fDataRangeYMax));
            gWave.DrawLine(penBlkThin, arrPts[0].X, arrPts[0].Y, arrPts[1].X, arrPts[1].Y);
            // 绘制箭头
            arrPts = new PointF[3]; 
            arrPts[0] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(m_fMargin-10.0f, -5.0f), new PointF(fDataRangeXMax, 0.0f));
            arrPts[1] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(m_fMargin, 0.0f), new PointF(fDataRangeXMax, 0.0f));
            arrPts[2] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(m_fMargin-10.0f, +5.0f), new PointF(fDataRangeXMax, 0.0f));
            gWave.DrawLines(penBlkThin, arrPts);
            arrPts[0] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(-5.0f, 10.0f - m_fMargin), new PointF(0.0f, fDataRangeYMax));
            arrPts[1] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(0.0f, -m_fMargin), new PointF(0.0f, fDataRangeYMax));
            arrPts[2] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(5.0f, 10.0f - m_fMargin), new PointF(0.0f, fDataRangeYMax));
            gWave.DrawLines(penBlkThin, arrPts);
            // 绘制原点
            arrPts[0] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(-25.0f, 0.0f), new PointF(0.0f, 0.0f)); ;
            gWave.DrawString("O", fontBlkThin, brshBlk, arrPts[0]);
            // 绘制刻度
            arrPts = new PointF[2];
            m_fMarkSpan = (fDataRangeXMax - fDataRangeXMin) / m_nMarkCntX;
            for (float fMark = fDataRangeXMin; fMark <= fDataRangeXMax; fMark += m_fMarkSpan)
            {
                if (fMark == 0)
                    continue;
                arrPts[0] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(0.0f, -5.0f), new PointF(fMark, 0.0f));
                arrPts[1] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(0.0f, 5.0f), new PointF(fMark, 0.0f));
                gWave.DrawLine(penBlkThin, arrPts[0], arrPts[1]);
                arrPts[1].X -= 20.0f;
                gWave.DrawString(string.Format("{0:F1}", fMark), fontBlkThin, brshBlk, arrPts[1]);
            }
            for (float fMark = fDataRangeYMin; fMark <= fDataRangeYMax; fMark += m_fMarkSpan)
            {
                if (fMark == 0)
                    continue;
                arrPts[0] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(-5.0f, 0.0f), new PointF(0.0f, fMark));
                arrPts[1] = CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(5.0f, 0.0f), new PointF(0.0f, fMark));
                gWave.DrawLine(penBlkThin, arrPts[0], arrPts[1]);
                arrPts[1].X -= 50.0f;
                arrPts[1].Y -= 10.0f;
                gWave.DrawString(string.Format("{0:F1}",fMark), fontBlkThin, brshBlk, arrPts[1]);
            }

            // 绘制数据
            if(m_arrData.Count > 1)
            {
                PointF[] arrPtData = new PointF[m_arrData.Count];
                for (int i = 0; i < m_arrData.Count; i++)
                {
                    arrPtData[i] = new PointF(i, (float)m_arrData[i]);
                }
                CoordTrans_DataToView(nViewWidth, nViewHeight, m_fScaleDTV, m_ptDataOffset, new PointF(0.0f, 0.0f), ref arrPtData);
                //gWave.DrawLines(penRedMid, arrPtData);
                gWave.DrawCurve(penRedMid, arrPtData);

            }

            // 显示
            pbWave.Image = m_bmpWave;
            // 释放资源
            penRedMid.Dispose();
            penBlkThin.Dispose();
            fontBlkThin.Dispose();
            brshBlk.Dispose();
            gWave.Dispose();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            Display();
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            m_ptDataOffset.X -= m_fMarkSpan;
            Display();
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            m_ptDataOffset.X += m_fMarkSpan;
            Display();

        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            m_ptDataOffset.Y += m_fMarkSpan;
            Display();

        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            m_ptDataOffset.Y -= m_fMarkSpan;
            Display();

        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            m_fDataRangeX *= 0.5f;
            Display();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            m_fDataRangeX *= 2.0f;
            Display();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Display();
        }
    }
}
