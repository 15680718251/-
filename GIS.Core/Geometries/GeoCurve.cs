using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    /// <summary>
    /// Curve是一个1维的几何类，常常包含一组GeoPoint
    /// 如果没有相交的情况出现，那么这个Curve是简单的
    /// 如果首尾节点完全相同，那么这个Curve是闭合的（这个判断用在组成面的情况，组成面的线必须是简单的闭合的）
    /// 如果一个Curve简单且闭合，则它就是一个Ring
    /// </summary>
    public abstract class GeoCurve:Geometry
    {
       

        public abstract GeoPoint StartPoint { get;}
        public abstract GeoPoint EndPoint { get;}

        /// <summary>
        /// 判断线是否闭合 (StartPoint = EndPoint).
        /// </summary>
        public bool IsClosed
        {
            get { return (this.StartPoint.IsEqual(this.EndPoint));}
        }
        /// <summary>
        /// 判断线是否为环，要求闭合且不自相交
        /// </summary>
        public abstract bool IsRing { get; }
      
    }
}
