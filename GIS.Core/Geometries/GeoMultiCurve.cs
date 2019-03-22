using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    /// <summary>
    /// MultiCurve是一个存放若干个Curve的1维GeometryCollection。
    /// MultiCurve是一个虚类不能被实例化，只是为其派生类定义了一系列方法让派生类来实现
    /// </summary>
    public abstract class GeoMultiCurve : GeometryCollection
    {
         
    }
}