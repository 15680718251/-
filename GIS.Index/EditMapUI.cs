using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using GIS.Map;
using GIS.Geometries;
using GIS.Layer;
using GIS.Toplogical;
using GIS.Utilities;
using GIS.TreeIndex.Forms;
using GIS.TreeIndex.OprtRollBack;
using System.Net.Sockets;
using System.Threading;

namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    {
        public List<GeoData.GeoDataRow> SltGeoSet
        {
            get { return m_Map.SltGeoSet; }
        }
        public List<GeoData.GeoDataRow> SltLabelSet
        {
            get { return m_Map.SltLableSet; }
        }

        public void ReverseSelect()
        {
            for (int i = 0; i < LayerCounts; i++)
            {
                GeoVectorLayer layer = GetLayerAt(i) as GeoVectorLayer;

                if (layer == null || layer.LayerTypeDetail == LAYERTYPE_DETAIL.SurveyLayer)
                    continue;
                for (int j = 0; j < layer.DataTable.Count; j++)
                {
                    GeoData.GeoDataRow row = layer.DataTable[j];
                    if ((int)row.EditState > 5)
                        continue;
                    if (row.SelectState == true)
                    {
                        RemoveSltObj(row);
                    }
                    else
                    {
                        AddSltObj(row);
                    } 
                }
            }
            Refresh();
 
            if (SltGeoSet.Count > 0 || SltLabelSet.Count > 0)
            OutPutTextInfo(string.Format(">>选中{0}个几何目标，{1}个文本注记\r\n", SltGeoSet.Count, SltLabelSet.Count));
        }
        public void SelectAll()
        {
            ClearAllSltWithoutRefresh();
            for (int i = 0; i < LayerCounts; i++)
            {
                GeoVectorLayer layer = GetLayerAt(i) as GeoVectorLayer;

                if (layer == null || layer.LayerTypeDetail==  LAYERTYPE_DETAIL.SurveyLayer)
                    continue;
                for (int j = 0; j < layer.DataTable.Count; j++)
                {
                    GeoData.GeoDataRow row = layer.DataTable[j]; 
                    row.SelectState = true;
                    if (row.Geometry is GeoLabel)
                    {
                        SltLabelSet.Add(row);
                    }
                    else
                     SltGeoSet.Add(row);
                }               
            }
            Refresh();
            if (SltGeoSet.Count > 0 || SltLabelSet.Count > 0)
            OutPutTextInfo(string.Format(">>选中{0}个几何目标，{1}个文本注记\r\n", SltGeoSet.Count,SltLabelSet.Count));

        }
        public bool PtOnGtr(Point pt, SelectType type, ref GIS.GeoData.GeoDataRow outrow)
        {
            GeoPoint ptMap = TransFromMapToWorld(pt);

            for (int j = 0; j < LayerCounts; j++)
            {
                GeoData.GeoDataTable table = null;
                GeoLayer layer = GetLayerAt(j);
                if (layer.LayerTypeDetail == LAYERTYPE_DETAIL.SurveyLayer)
                    continue;

                if (layer is GeoVectorLayer && layer.Enable)
                {
                    table = ((GeoVectorLayer)layer).DataTable;            //图层
                }
                else
                    continue;

                int nNumsGeo = table.Count;
                for (int k = 0; k < nNumsGeo; k++)
                {
                    GIS.GeoData.GeoDataRow row = table[k];   //找到的对象
                    if (row.Geometry == null ||
                        (int)row.EditState >= 6 ||
                        !row.Geometry.Bound.IsIntersectWith(m_ViewExtents)) //如果已经删除或者不在屏幕范围内则不进入判断
                        continue;
                    if (
                        ((type == SelectType.Point) && !(row.Geometry is GeoPoint || row.Geometry is GeoMultiPoint))
                        ||
                        ((type == SelectType.Line) && !(row.Geometry is GeoLineString || row.Geometry is GeoMultiLineString))
                        ||
                        ((type == SelectType.Polygon) && !(row.Geometry is GeoPolygon || row.Geometry is GeoMultiPolygon))
                        ||
                        ((type == SelectType.Geomtry) && row.Geometry is GeoLabel)
                        ||
                        (type == SelectType.Label) && !(row.Geometry is GeoLabel))
                    {
                        continue;
                    }
                    if (row.Geometry.IsSelectByPt(ptMap))    //判断对象是否被选中
                    {
                        outrow = row.Clone();
                        return true;
                    }
                }
            }
            return false;
        } 
        public GIS.GeoData.GeoDataRow SelectByPt(Point pt,SelectType type)
        {
            GeoPoint ptMap = TransFromMapToWorld(pt);

            GIS.GeoData.GeoDataRow rowSlt = null;

            for (int j = 0; j < LayerCounts; j++)
            {
                GeoData.GeoDataTable table = null;
                GeoLayer layer = GetLayerAt(j);
                if (layer.LayerTypeDetail == LAYERTYPE_DETAIL.SurveyLayer)
                    continue;
                if (layer is GeoVectorLayer && layer.Enable)
                {
                    table = ((GeoVectorLayer)layer).DataTable;            //图层
                }

                else
                    continue;

                int nNumsGeo = table.Count;
                for (int k = 0; k < nNumsGeo; k++)
                {
                    GIS.GeoData.GeoDataRow row = table[k];   //找到的对象
                    if (row.Geometry == null ||
                        (int)row.EditState >= 6 ||
                        !row.Geometry.Bound.IsIntersectWith(m_ViewExtents)) //如果已经删除或者不在屏幕范围内则不进入判断
                        continue;
                    if (
                        ((type == SelectType.Point)&& !(row.Geometry is GeoPoint || row.Geometry is GeoMultiPoint))
                        ||
                        ((type == SelectType.Line)&&!(row.Geometry is GeoLineString||row.Geometry is GeoMultiLineString))
                        ||
                        ((type == SelectType.Polygon)&&!(row.Geometry is GeoPolygon || row.Geometry is GeoMultiPolygon))
                        ||
                        ((type== SelectType.Geomtry)&& row.Geometry is GeoLabel)
                        ||
                        (type== SelectType.Label)&& !(row.Geometry is GeoLabel))
                    {
                        continue;
                    }
                    if (row.Geometry.IsSelectByPt(ptMap))    //判断对象是否被选中
                    {
                        if (row.SelectState == false)
                        {
                            m_Map.AddSltObj(row);         //添加对象
                        }
                        else
                        {
                            m_Map.RemoveSltObj(row);     //从选中集合中删除
                        }
                        rowSlt = row;
                        if (SltGeoSet.Count > 0 || SltLabelSet.Count > 0)
                            OutPutTextInfo(string.Format(">>选中{0}个几何目标，{1}个文本注记\r\n", SltGeoSet.Count, SltLabelSet.Count));

                        return rowSlt;
                    }
                }
            }
            if (SltGeoSet.Count > 0 || SltLabelSet.Count > 0)
                OutPutTextInfo(string.Format(">>选中{0}个几何目标，{1}个文本注记\r\n", SltGeoSet.Count, SltLabelSet.Count));

            return rowSlt;
        }
        /// <summary>
        /// 点选函数
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public GIS.GeoData.GeoDataRow SelectByPt(Point pt)
        {
            return SelectByPt(pt, m_SelectType);
        }
        public List<GIS.GeoData.GeoDataRow> SelectByRegion(GeoPolygon ring)
        {
            List<GIS.GeoData.GeoDataRow> rows = new List<GIS.GeoData.GeoDataRow>();

            for (int j = 0; j < LayerCounts; j++)
            {
                GeoData.GeoDataTable table = null;
                GeoLayer layer = GetLayerAt(j);
                if (layer.LayerTypeDetail == LAYERTYPE_DETAIL.SurveyLayer)
                    continue;
                if (layer is GeoVectorLayer && layer.Enable)
                {
                    table = ((GeoVectorLayer)layer).DataTable;            //图层
                }
                 
                else
                    continue;

                int nNumsGeo = table.Count;
                for (int k = 0; k < nNumsGeo; k++)
                {
                    GIS.GeoData.GeoDataRow row = table[k];   //找到的对象
                    if (row.Geometry == null || (int)row.EditState >= 6 ||
                        !row.Geometry.Bound.IsIntersectWith(m_ViewExtents)) //如果已经删除或者不在屏幕范围内则不进入判断
                        continue;

                    TpRelateConstants relate = TpRelatemain.IsTpWith(ring, row.Geometry, false);
                    if (relate != TpRelateConstants.tpDisjoint
                        && relate != TpRelateConstants.tpInside
                        && relate != TpRelateConstants.tpUnknow)    //判断对象是否被选中
                    {
                        if (row.SelectState == false)
                        {
                            m_Map.AddSltObj(row);         //添加对象
                        }
                        else
                        {
                            m_Map.RemoveSltObj(row);     //从选中集合中删除
                        }
                        rows.Add(row);

                    }
                }
            }
            if(SltGeoSet.Count>0 || SltLabelSet.Count>0)
                OutPutTextInfo(string.Format(">>选中{0}个几何目标，{1}个文本注记\r\n", SltGeoSet.Count, SltLabelSet.Count));
            return rows;
        }
        public void ClearAllSltWithoutRefresh()
        {
            for (int i = 0; i < m_Map.SltGeoSet.Count; i++)
            {
                GeoData.GeoDataRow row = m_Map.SltGeoSet[i];
                if (row.Geometry != null)
                {
                    row.SelectState = false;                    
                }
            }
            for (int i = 0; i < m_Map.SltLableSet.Count; i++)
            {
                GeoData.GeoDataRow row = m_Map.SltLableSet[i];
                if (row.Geometry != null)
                {
                    row.SelectState = false;
                
                }
            }
            m_Map.SltGeoSet.Clear();
            m_Map.SltLableSet.Clear();
        }
        public void ClearAllSlt()
        {
            bool bNeedRefresh = false;
            for (int i = 0; i < m_Map.SltGeoSet.Count; i++)
            {
                GeoData.GeoDataRow row = m_Map.SltGeoSet[i];
                if (row.Geometry != null)
                {
                    row.SelectState = false;
                    bNeedRefresh = true;
                }
            }

            for (int i = 0; i < m_Map.SltLableSet.Count; i++)
            {
                GeoData.GeoDataRow row = m_Map.SltLableSet[i];
                if (row.Geometry != null)
                {
                    row.SelectState = false;
                    bNeedRefresh = true;
                }
            }
            m_Map.SltGeoSet.Clear();
            m_Map.SltLableSet.Clear();
            if (bNeedRefresh)
                Refresh();
        }
        public void ClearDraft()
        {
            bool bNeedRefresh = false;
            LayerGroup lg = GetGroupByName("系统工作区");
            List<GeoLayer> layers = lg.Layers;
            for (int i = 0; i < layers.Count; i++)
            {
                GeoVectorLayer layer = layers[i] as GeoVectorLayer;
                GeoData.GeoDataTable gdt = layer.DataTable;
                if (gdt.Count > 0)
                {
                    bNeedRefresh = true;
                }
                gdt.Clear();
            }
            if (bNeedRefresh)
                Refresh();
        }
        public bool AddSltObj(GeoData.GeoDataRow row)
        {
             return m_Map.AddSltObj(row);
        }
        public bool RemoveSltObj(GeoData.GeoDataRow row)
        {
            return m_Map.RemoveSltObj(row);
        }
        /// <summary>
        /// 捕捉点
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public GIS.GeoData.GeoDataRow MouseCatch(GeoPoint pt)//, MouseCatchType type)
        {

            for (int j = 0; j < LayerCounts; j++)
            {
                GeoVectorLayer lyr = GetLayerAt(j) as GeoVectorLayer;            //图层
                if (lyr == null || !lyr.Enable)
                    continue;

                int nNumsGeo = lyr.DataTable.Count;
                for (int k = 0; k < nNumsGeo; k++)
                {
                    GIS.GeoData.GeoDataRow row = lyr.DataTable[k];   //找到的对象
                    if (row.Geometry == null || (int)row.EditState >= 6 ||
                        !row.Geometry.Bound.IsIntersectWith(m_ViewExtents)) //如果已经删除或者不在屏幕范围内则不进入判断
                        continue;

                    GeoPoint SnapPt = row.Geometry.MouseCatchPt(pt, m_CatchType);
                    if (SnapPt != null)    //判断对象是否被选中
                    {
                        m_SnapPoint = SnapPt;
                        return row;
                    }
                } 
            }

            m_SnapPoint = null;
            return null;
        }
        public GIS.GeoData.GeoDataRow MouseCatchInSltGeoSet(GeoPoint pt,MouseCatchType type)
        {
            for (int i = 0; i < SltGeoSet.Count; i++)
            {
                GeoPoint ptResult = SltGeoSet[i].Geometry.MouseCatchPt(pt, type);
                if (ptResult != null)
                {
                    m_SnapPoint = ptResult;
                    return SltGeoSet[i];
                }
            }
            m_SnapPoint = null;
            return null;
        }
        public bool DeleteSltObjSet(DeleteType type)
        {
            int numslts = 0;
            if (type == DeleteType.Geomtry)
                numslts = m_Map.SltGeoSet.Count;
            else if (type == DeleteType.Label)
                numslts = m_Map.SltLableSet.Count;
            if (numslts == 0) return false;

            List<GeoData.GeoDataRow> list = new List<GIS.GeoData.GeoDataRow>();

            #region 操做回退
      
            OperandList oprts = new OperandList();          
            m_OprtManager.AddOprt(oprts);
            #endregion

            for (int i = numslts - 1; i >= 0; i--)
            {
                GeoData.GeoDataRow row = null;

                if (type == DeleteType.Geomtry)
                    row = m_Map.SltGeoSet[i];
                else if (type == DeleteType.Label)
                    row = m_Map.SltLableSet[i];
                EditState state = row.EditState;
                if (row.EditState != EditState.Appear)
                {
                    row.EditState = EditState.Disappear;
                }
                else
                    row.EditState = EditState.Invalid;

                Operand oprt = new Operand(row, state, row.EditState);
                oprts.m_OldOperands.Add(oprt);

                m_Map.RemoveSltObj(row);
            }
            if (numslts > 0)
            {     //记录增量信息
                //Increment.IncDisappear incInfo = new GIS.Increment.IncDisappear(list);
                //m_IncManager.AddIncInfo(incInfo);
                OutPutTextInfo(string.Format("删除{0}个目标\r\n", numslts));
                return true;
            }
            return false;
        }
        public void OpenAttributeForm(string GroupName, string LayerName)
        {
            GIS.TreeIndex.Forms.AttributeForm form = new GIS.TreeIndex.Forms.AttributeForm(this,LayerName);
  
            form.ShowDialog();
        }
        public GeoData.GeoDataRow AddGeometryToActiveLayer(Geometry geom)
        {
            Layer.GeoVectorLayer actLyr = GetActiveVectorLayer() as Layer.GeoVectorLayer;
            if (actLyr != null)
            {
                GeoData.GeoDataRow row = actLyr.AddGeometry(geom);
                GetGroupByLayer(actLyr).LayerGroupBound = GetGroupByLayer(actLyr).GetBoundingBox();
                m_Map.MapBound = m_Map.GetBoundingBox(); 
                return row;
            }
            else
            {
                MessageBox.Show("当前活动图层不可用！请选中目标矢量图层，右击激活。");
                return null;
            }
        }
        public void InitialCopyGeoFeature(GeoData.GeoDataRow rowOld, GeoData.GeoDataRow rowNew)
        {
            for (int i =1; i < rowOld.ItemArray.Length; i++)
            {
                rowNew[i] = rowOld[i].ToString();
            }
            rowNew["FeatID"] = System.Guid.NewGuid().ToString();
            rowNew["BeginTime"] = MapUI.BeginTime;
        }
        public void InitialNewGeoFeature(GeoData.GeoDataRow row)
        {
            //search tag!!!
            //if (row["FeatID"] == DBNull.Value)
            //{
            //    row["FeatID"] = System.Guid.NewGuid().ToString();
            //}
            //if (row["BeginTime"] == DBNull.Value)
            //{
            //    row["BeginTime"] = MapUI.BeginTime;
            //}
            //if (row["ClasID"] == DBNull.Value)
            //{
            //    GeoData.GeoDataTable table = ( GeoData.GeoDataTable)row.Table;
            //    GeoLayer lyr = GetLayerByTable(table);
            //    //row["ClasID"] = lyr.ClasID;
            //    int cnt = table.Rows.Count;
            //    if (cnt == 1)
            //    {
            //        row["ClasID"] = lyr.ClasID;
            //    }
            //    else
            //    {
            //        int ttt = System.Convert.ToInt32(table.Rows[0]["ClasID"]);
            //        row["ClasID"] = ttt;
            //    }
            //}

            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                if (row.Table.Columns[i].ColumnName == "FeatID" && row["FeatID"] == DBNull.Value)
                {
                    row["FeatID"] = System.Guid.NewGuid().ToString();
                    continue;
                }
                if (row.Table.Columns[i].ColumnName == "BeginTime" && row["BeginTime"] == DBNull.Value)
                {
                    row["BeginTime"] = MapUI.BeginTime;
                    continue;
                }
                if (row.Table.Columns[i].ColumnName == "ClasID" && row["ClasID"] == DBNull.Value)
                {
                    GeoLayer lyr = GetLayerByTable((GeoData.GeoDataTable)row.Table);
                    int cnt = row.Table.Rows.Count;
                    //if (cnt == 1)
                    //{
                    //    row["ClasID"] = lyr.ClasID;
                    //}
                    //else
                    //{
                    //    int ttt = System.Convert.ToInt32(row.Table.Rows[0]["ClasID"]);
                    //    row["ClasID"] = ttt;
                    //}
                    row["ClasID"] = lyr.ClasID;
                    continue;
                }
                if (row.Table.Columns[i].ColumnName == "FID" || row.Table.Columns[i].ColumnName == "ClasID"
                    || row.Table.Columns[i].ColumnName == "FeatID" || row.Table.Columns[i].ColumnName == "BeginTime"
                    || row.Table.Columns[i].ColumnName == "PlgAttr")
                {
                    continue;
                }

                if (row.Table.Rows.Count > 1)
                {
                    row[i] = row.Table.Rows[0][i];
                }
            }
        }
        public void ZoomToLayerGroup(string lyrGroupName)
        {
            LayerGroup group = GetGroupByName(lyrGroupName);
            ZoomToBox(group.LayerGroupBound);
            Refresh();
        }
        public void ZoomToLayer(string lyrName)
        {
            GeoLayer lyr = GetLayerByName(lyrName);
            ZoomToBox(lyr.LayerBound);
            Refresh();
        }
        //通过几何修改地图范围
        public void BoundingBoxChangedBy(GeoData.GeoDataRow row)
        {
            m_Map.BoundingBoxChangedBy(row);
        }
        public void LabelLayerSetting()
        {
            LabelLayerSettringForm form = new LabelLayerSettringForm(this);
            form.ShowDialog();
        }
        //public void ManualInputSurveyPoint()
        //{
        //    InputXYZForm form = new InputXYZForm();
        //    if (form.ShowDialog() == DialogResult.OK)
        //    {
        //        m_SurveyPt = new GeoPoint3D(form.X, form.Y,form.Z);
        //        GeoPoint3D pt = new GeoPoint3D(form.X, form.Y, form.Z);
        //        GeoVectorLayer lyr = m_Map.GetLayerByName("测点层") as GeoVectorLayer;
        //        lyr.AddGeometry(pt);
        //        SetCenterGeoPoint(m_SurveyPt);
        //        Refresh();
        //    }
        //}
        public void RecSurveyPoint()
        {
            if (m_SurveyPt == null)
                return;
           GeoVectorLayer lyr = m_Map.GetLayerByName("测点层") as GeoVectorLayer;
           lyr.AddGeometry(m_SurveyPt);
           Refresh();

           Point pt = TransFromWorldToMap(m_SurveyPt);
           MouseEventArgs e = new MouseEventArgs(MouseButtons.Left,1,pt.X,pt.Y,0);
           MapTool.OnMouseDown(null, e);
        }
        public void LineVerticalCommand()
        {
            if (m_Tool is Tool.MapGenerateLineTool)
            {
                Tool.MapGenerateLineTool tool = m_Tool as Tool.MapGenerateLineTool;
                KeyEventArgs e = new KeyEventArgs(Keys.V);
                tool.CommandHandle(e);
            }
            else
                OutPutTextInfo(">>提示：在线绘制工具中才能使用该功能！请绘制线体,或追加线体。\r\n");
        }
        public void LineTCommand()
        {
            if (m_Tool is Tool.MapGenerateLineTool)
            {
                Tool.MapGenerateLineTool tool = m_Tool as Tool.MapGenerateLineTool;
                KeyEventArgs e = new KeyEventArgs(Keys.T);
                tool.CommandHandle(e);
            }
            else
                OutPutTextInfo(">>提示：在线绘制工具中才能使用该功能！请绘制线体,或追加线体。\r\n");
        }
        public void LineExentCommand()
        {
            if (m_Tool is Tool.MapGenerateLineTool)
            {
                Tool.MapGenerateLineTool tool = m_Tool as Tool.MapGenerateLineTool;
                KeyEventArgs e = new KeyEventArgs(Keys.E);
                tool.CommandHandle(e);
            }
            else
                OutPutTextInfo(">>提示：在线绘制工具中才能使用该功能！请绘制线体,或追加线体。\r\n");
        }
        public void LineCrossCommand()
        {
            if (m_Tool is Tool.MapGenerateLineTool)
            {
                Tool.MapGenerateLineTool tool = m_Tool as Tool.MapGenerateLineTool;
                KeyEventArgs e = new KeyEventArgs(Keys.X);
                tool.CommandHandle(e);
            }
            else
                OutPutTextInfo(">>提示：在线绘制工具中才能使用该功能！请绘制线体,或追加线体。\r\n");
        }
        public void SetSouthWestCordinate()
        {
            throw new NotImplementedException();
        }
        public GeoBound UnionBounds(List<GeoData.GeoDataRow> rows)
        {
            if (rows == null ||rows.Count == 0)
                return null;
            GeoBound bound = null;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Geometry != null)
                {
                    if (bound == null)
                        bound = rows[i].Geometry.Bound.Clone();
                    else
                        bound.UnionBound(rows[i].Geometry.Bound);
                }
            }
            return bound;
        }
        public delegate void SearchFeatureEventHandle(List<string> results);
        public event SearchFeatureEventHandle SearchFeatureResult;
        public void SearchFeatureByField(string text)
        {
            ClearAllSlt();
            
            List<string> results = new List<string>();
            for (int i = 0; i < LayerCounts; i++)
            {
                GeoVectorLayer lyr = GetLayerAt(i) as GeoVectorLayer;
                if (lyr == null || lyr.LayerName == "测点层") continue;

                for (int j = 0; j < lyr.DataTable.Count; j++)
                {
                    GeoData.GeoDataRow row = lyr.DataTable[j];
                    for (int k = 0; k < lyr.DataTable.Columns.Count; k++)
                    {
                        if (row[k].ToString().Contains(text))
                        {
                            AddSltObj(row);
                            results.Add(string.Format("{0},{1},{2}",lyr.LayerName,row[k].ToString(),j));
                            break;
                        }
                    }
                }
            }
            if (results.Count > 0)
            {
                GeoBound bound = UnionBounds(SltGeoSet);
                ZoomToBox(bound);
                if (SearchFeatureResult != null)
                {
                    SearchFeatureResult(results);
                }
            }
            Refresh();
        }
        public void ZoomToFeature(string lyrName, int id)
        {
           
           GeoVectorLayer lyr = GetLayerByName(lyrName) as GeoVectorLayer;
           if (lyr != null)
           {
               GeoData.GeoDataRow row = lyr.DataTable[id];
               if (row.Geometry != null)
               {
                   ClearAllSltWithoutRefresh();
                   AddSltObj(row);
                   ZoomToBox(row.Geometry.Bound);
                   Refresh();
               }
           }
        }
        public void StepBackword()
        {
            m_OprtManager.StepBackword();
            ClearAllSltWithoutRefresh();
            Refresh();
        }
        public void StepForword()
        {
            m_OprtManager.StepForword();
            ClearAllSltWithoutRefresh();
            Refresh();
        }

        //public void SendIncFile()
        //{
        //    NetTransForm form = new NetTransForm();
        //    form.ShowDialog();
        //}
        Thread threadReadFile;
        TcpListener tl;
        volatile bool m_bReading = false;
        public string m_FolderDefault="C:\\";
        //public void BeginReceiveFile()
        //{
        //    FolderBrowserDialog dlg = new FolderBrowserDialog();
        //    dlg.Description = "设置增量文件存储目录";
        //    dlg.SelectedPath = m_FolderDefault;
        //    if (dlg.ShowDialog() == DialogResult.OK)
        //    {
        //        m_FolderDefault = dlg.SelectedPath;
        //    }
        //    else
        //    {
        //        return;
        //    }

        //    if (m_bReading)
        //    {
        //        m_bReading = false;          
        //        tl.Stop();
        //        threadReadFile.Abort();
                
        //    }
        //    threadReadFile = new Thread(Listen);
        //    threadReadFile.IsBackground = true;
        //    m_bReading = true;
        //    threadReadFile.Start();
        //    MessageBox.Show("开始接受文件传输，正在监听！");
        //}
      
        public void Listen()
        {
            tl = new TcpListener(6767);
            tl.Start();
            while (m_bReading)
            {
                Socket sock = tl.AcceptSocket();
         
                byte[] filename = new byte[256];
                sock.Receive(filename, 256, SocketFlags.None);
                string strFileName = Encoding.Default.GetString(filename);
                strFileName = strFileName.Substring(0, strFileName.IndexOf('\0'));
                string path = m_FolderDefault + strFileName;
                FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
                BinaryWriter br = new BinaryWriter(fs);
                int count;
                byte[] b = new byte[1024];
                while ((count = sock.Receive(b, 1024, SocketFlags.None)) != 0)
                {
                    br.Write(b, 0, count);
                }
                br.Close();
                fs.Close();
                sock.Close();
                MessageBox.Show(string.Format("接受到了{0}文件,已存至{1}目录中，等待下一次接受", strFileName, m_FolderDefault));
            }
            
            tl.Stop();
        }

        //public void StopReceiveFile()
        //{
        //    if (m_bReading)
        //    { 
        //        m_bReading = false;
        //        tl.Stop();
        //        threadReadFile.Abort();
        //        MessageBox.Show("停止接收文件");
        //    }
        //}

        #region 20121013 可能需要修改的GeoTag
        public class GeoTag
        {
             public GeoTag(GeoPoint pt ,string pathName,int id)
             {
                 m_Pos = pt;
                 m_PathName = pathName;
                 m_ID = id;
             }
            public GeoPoint m_Pos;
            public string m_PathName;
            public int m_ID;
        }
        public List<GeoTag> m_GeoTagList = new List<GeoTag>();
        //public void ImportCamera()
        //{
        //    try
        //    {
        //        string cur_path = string.Format("{0}\\DCIM\\LuQi.Cam", Application.StartupPath);
        //        FileStream fs = new FileStream(cur_path, FileMode.Open, FileAccess.Read);
        //        StreamReader sr = new StreamReader(fs);
        //        string data = null;
        //        while ((data = sr.ReadLine()) != null)
        //        {
        //            string[] datas = data.Split(',');
        //            int id = int.Parse(datas[0]);
        //            double x = double.Parse(datas[1]);
        //            double y = double.Parse(datas[2]);
        //            string path = datas[3];
        //            GeoTag tag = new GeoTag(new GeoPoint(x, y), path, id);
        //            m_GeoTagList.Add(tag);
        //        }
        //        sr.Close();
        //        fs.Close();
        //        Refresh();
        //    }
        //    catch
        //    {
        //        MessageBox.Show("没有GEOTAG记录");
        //    }

        //}
        #endregion

        //public void WebCamera()
        //{
        //    if (m_SurveyPt == null)
        //    {
        //        MessageBox.Show("没有观测点坐标，请先确定位置！");
        //        return;
        //    }
        //     CameraForm form = new CameraForm();
        //     if (form.ShowDialog() == DialogResult.OK)
        //     {
        //         if (form.FileName != null )
        //         {
        //             string cur_path = string.Format("{0}\\DCIM\\LuQi.Cam", Application.StartupPath);
        //             int index = 0;
        //             try
        //             {                       
        //                 FileStream fs = new FileStream(cur_path, FileMode.Open, FileAccess.Read);
        //                 StreamReader sr = new StreamReader(fs);
        //                 string data = null;
        //                 while ((data = sr.ReadLine()) != null)
        //                 {
        //                     index++;
        //                 }
        //                 sr.Close();
        //                 fs.Close();
        //             }

        //             catch
        //             {

        //             }
        //             finally
        //             {

        //                 FileStream fs1 = new FileStream(cur_path, FileMode.Append, FileAccess.Write);
        //                 StreamWriter sw = new StreamWriter(fs1);
        //                 sw.WriteLine(string.Format("{0},{1},{2},{3}", index, m_SurveyPt.X, m_SurveyPt.Y, form.FileName));
        //                 GeoTag tag = new GeoTag(m_SurveyPt.Clone() as GeoPoint, form.FileName, index);
        //                 m_GeoTagList.Add(tag);
        //                 Refresh();
        //                 sw.Close();
        //                 fs1.Close();
        //             }
        //         }
        //     }
        //}

        //public void ClearCamera()
        //{
        //    m_GeoTagList.Clear();
        //    Refresh();
        //}
    }
}
 