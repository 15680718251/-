using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.TreeIndex;
using GIS.Geometries;
using GIS.GeoData;

namespace GIS.TreeIndex.OprtRollBack
{
    public class Operand
    {
        public Operand(GeoDataRow row, EditState ostate,EditState nstate)
        {
            m_Row = row;
            m_OldState = ostate;
            m_NewState = nstate;

        }
        public GeoDataRow m_Row;
        public EditState m_OldState;
        public EditState m_NewState;
    }
    public class OperandList
    {
        public OperandList()
        {
            m_OldOperands = new List<Operand>();
            m_NewOperands = new List<Operand>();
        }
        public List<Operand> m_OldOperands;
        public List<Operand> m_NewOperands;
        public void Run()
        {
            for (int i = 0; i < m_NewOperands.Count; i++)
            {
                m_NewOperands[i].m_Row.EditState = m_NewOperands[i].m_NewState;
            }
            for (int i = 0; i < m_OldOperands.Count; i++)
            {
                m_OldOperands[i].m_Row.EditState = m_OldOperands[i].m_NewState;
            }
        }
        public void Undo()
        {
            for (int i = 0; i < m_NewOperands.Count; i++)
            {
                m_NewOperands[i].m_Row.EditState = EditState.Invalid;
            }
            for (int i = 0; i < m_OldOperands.Count; i++)
            {
                m_OldOperands[i].m_Row.EditState = m_OldOperands[i].m_OldState;
            }
        }
    }
    public class OprtManager
    {
        public OprtManager()
        {
            m_OprtList = new List<OperandList>();
            m_nPos = -1;
        }
        private List<OperandList> m_OprtList;
        private int m_nPos;

        public void ClearEndOprt()
        {
            if (m_nPos == m_OprtList.Count - 1)
                return;
            for (int i = m_OprtList.Count - 1; i > m_nPos; i--)
            {
                m_OprtList.RemoveAt(i);
            }
        }
        public void AddOprt(OperandList oprt)
        {
            ClearEndOprt();
            m_OprtList.Add(oprt);
            m_nPos = m_OprtList.Count - 1;
        }
        public void StepForword()
        {
            if (m_nPos < -1 || m_nPos >= m_OprtList.Count - 1) 
                return;
            m_OprtList[++m_nPos].Run();
        }
        public void StepBackword()
        {
            if (m_nPos <= -1 || m_nPos > m_OprtList.Count - 1)
                return;
            m_OprtList[m_nPos--].Undo();
        }
    }
}
