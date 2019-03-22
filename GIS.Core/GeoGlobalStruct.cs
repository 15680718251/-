using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS
{
    public enum MouseCatchType
    {
        Vertex = 0,//端点
        Center = 1,//终点
        Both   = 2, //同时
        None   = 3  //不捕捉

    }
    public enum DeleteType
    {
        Geomtry,
        Label
    }
    public enum SelectType
    {
        Point ,
        Line,
        Polygon,
        Geomtry,
        Label,
        All        
    }
     public enum GeomType : int
    {
        /// <summary>
        /// Null shape with no geometric data
        /// </summary>
        Null = 0,
        /// <summary>
        /// A point consists of a pair of double-precision coordinates.
        /// GIS interpretes this as <see cref="GIS.Geometries.Point"/>
        /// </summary>
        Point = 1,
        /// <summary>
        /// PolyLine is an ordered set of vertices that consists of one or more parts. A part is a
        /// connected sequence of two or more points. Parts may or may not be connected to one
        ///	another. Parts may or may not intersect one another.
        /// GIS interpretes this as either <see cref="GIS.Geometries.LineString"/> or <see cref="GIS.Geometries.MultiLineString"/>
        /// </summary>
        PolyLine = 3,
        /// <summary>
        /// A polygon consists of one or more rings. A ring is a connected sequence of four or more
        /// points that form a closed, non-self-intersecting loop. A polygon may contain multiple
        /// outer rings. The order of vertices or orientation for a ring indicates which side of the ring
        /// is the interior of the polygon. The neighborhood to the right of an observer walking along
        /// the ring in vertex order is the neighborhood inside the polygon. Vertices of rings defining
        /// holes in polygons are in a counterclockwise direction. Vertices for a single, ringed
        /// polygon are, therefore, always in clockwise order. The rings of a polygon are referred to
        /// as its parts.
        /// GIS interpretes this as either <see cref="GIS.Geometries.Polygon"/> or <see cref="GIS.Geometries.MultiPolygon"/>
        /// </summary>
        Polygon = 5,
        /// <summary>
        /// A MultiPoint represents a set of points.
        /// GIS interpretes this as <see cref="GIS.Geometries.MultiPoint"/>
        /// </summary>
        Multipoint = 8,
        /// <summary>
        /// A PointZ consists of a triplet of double-precision coordinates plus a measure.
        /// GIS interpretes this as <see cref="GIS.Geometries.Point"/>
        /// </summary>
        PointZ = 11,
        /// <summary>
        /// A PolyLineZ consists of one or more parts. A part is a connected sequence of two or
        /// more points. Parts may or may not be connected to one another. Parts may or may not
        /// intersect one another.
        /// GIS interpretes this as <see cref="GIS.Geometries.LineString"/> or <see cref="GIS.Geometries.MultiLineString"/>
        /// </summary>
        PolyLineZ = 13,
        /// <summary>
        /// A PolygonZ consists of a number of rings. A ring is a closed, non-self-intersecting loop.
        /// A PolygonZ may contain multiple outer rings. The rings of a PolygonZ are referred to as
        /// its parts.
        /// GIS interpretes this as either <see cref="GIS.Geometries.Polygon"/> or <see cref="GIS.Geometries.MultiPolygon"/>
        /// </summary>
        PolygonZ = 15,
        /// <summary>
        /// A MultiPointZ represents a set of <see cref="PointZ"/>s.
        /// GIS interpretes this as <see cref="GIS.Geometries.MultiPoint"/>
        /// </summary>
        MultiPointZ = 18,
        /// <summary>
        /// A PointM consists of a pair of double-precision coordinates in the order X, Y, plus a measure M.
        /// GIS interpretes this as <see cref="GIS.Geometries.Point"/>
        /// </summary>
        PointM = 21,
        /// <summary>
        /// A shapefile PolyLineM consists of one or more parts. A part is a connected sequence of
        /// two or more points. Parts may or may not be connected to one another. Parts may or may
        /// not intersect one another.
        /// GIS interpretes this as <see cref="GIS.Geometries.LineString"/> or <see cref="GIS.Geometries.MultiLineString"/>
        /// </summary>
        PolyLineM = 23,
        /// <summary>
        /// A PolygonM consists of a number of rings. A ring is a closed, non-self-intersecting loop.
        /// GIS interpretes this as either <see cref="GIS.Geometries.Polygon"/> or <see cref="GIS.Geometries.MultiPolygon"/>
        /// </summary>
        PolygonM = 25,
        /// <summary>
        /// A MultiPointM represents a set of <see cref="PointM"/>s.
        /// GIS interpretes this as <see cref="GIS.Geometries.MultiPoint"/>
        /// </summary>
        MultiPointM = 28,
        /// <summary>
        /// A MultiPatch consists of a number of surface patches. Each surface patch describes a
        /// surface. The surface patches of a MultiPatch are referred to as its parts, and the type of
        /// part controls how the order of vertices of an MultiPatch part is interpreted.
        /// GIS doesn't support this feature type.
        /// </summary>
        MultiPatch = 31
    } ;
     public enum ArcType
     {
         Arc =0,
         Circle =1
     }
     public enum ChangeType
     {
         CHANGE_APPEAR = 1,		//出现
         CHANGE_DISAPPEAR = 2,		//消失
         CHANGE_GEOMETRY = 3,     //几何变化
         CHANGE_ATTRIBUTE = 4,		//稳定
         CHANGE_UNION = 5,     //合并
         CHANGE_SPILT = 6    //分割
     }
     public enum EditState1 : byte
     {
         Original = 0,       //保持原始状态   
         Appear = 1,         //出现   
         GeometryAft = 2,    //几何变化后  
         AttributeAft = 3,   //属性变化后
         UnionAft = 4,       //合并后的 
         SpiltAft = 5,       //分割后
         GeometryBef = 6,    //几何变化前    
         AttributeBef = 7,   //属性变化前
         Disappear = 8,      //消失
         UnionBef = 9,       //合并前       
         SpiltBef = 10,       //分割前  
         Invalid = 11    // 无
     }
    public enum EditState 
    {
         Original = 0,       //保持原始状态   
         Appear = 1,         //出现   
         GeometryAft = 2,    //几何变化后  
         AttributeAft = 3,   //属性变化后
        
         GeometryBef = 6,    //几何变化前    
         AttributeBef = 7,   //属性变化前
         Disappear = 8,      //消失
         
         Invalid = 10        //无效对象
    }

}
