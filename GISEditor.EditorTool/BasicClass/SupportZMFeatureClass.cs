using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace GISEditor.EditorTool.BasicClass
{
    class SupportZMFeatureClass
    {
        //修改FeatureClass的Z、M值
        public static IGeometry ModifyGeometryZMValue(IObjectClass featureClass, IGeometry modifiedGeo)
        {
            IFeatureClass TrgFtCls = featureClass as IFeatureClass;
            if (TrgFtCls == null) return null;
            string ShapeFileName = TrgFtCls.ShapeFieldName;
            IFields fields = TrgFtCls.Fields;
            int geometryIndex = fields.FindField(ShapeFileName);
            IField field = fields.get_Field(geometryIndex);
            IGeometryDef pGeometryDef = field.GeometryDef;
            IPointCollection pPointCollection = modifiedGeo as IPointCollection;
            if (pGeometryDef.HasZ)
            {
                IZAware pZAware = modifiedGeo as IZAware;
                pZAware.ZAware = true;
                IZ iz1 = modifiedGeo as IZ;
                //将Z值设为0
                iz1.SetConstantZ(0);
            }
            else
            {
                IZAware pZAware = modifiedGeo as IZAware;
                pZAware.ZAware = false;
            }
            if (pGeometryDef.HasM)
            {
                IMAware pMAware = modifiedGeo as IMAware;
                pMAware.MAware = true;
            }
            else
            {
                IMAware pMAware = modifiedGeo as IMAware;
                pMAware.MAware = false;
            }
            return modifiedGeo;
        }
    }
}
