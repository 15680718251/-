/*******************************************************
 * 文档说明：坐标类. by zbl 2018.7.11
 ******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.OSMIncrement.Buffer
{
    /// <summary>
    /// GEOObject坐标类
    /// </summary>
    public class Coordinate
    {
        #region Private Members
        private double _x = 0.0;
        private double _y = 0.0;
        #endregion

        #region Public Construtors
        public Coordinate()
        {
        }
        public Coordinate(double x, double y)
        {
            this._x = x;
            this._y = y;
        }
        public Coordinate(string x, string y)
        {
            try
            {
                this._x = x.Trim() == "" ? 0.0 : Convert.ToDouble(x.Trim());
                this._y = y.Trim() == "" ? 0.0 : Convert.ToDouble(y.Trim());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public Coordinate(string coord)
        {
            if (coord.Trim().Length > 0)
            {
                string[] coords = coord.Split(new char[] { ',' });
                if (coords.Length == 2)
                {
                    this._x = coords[0].Trim().Length > 0 ? Convert.ToDouble(coords[0].Trim()) : 0.0;
                    this._y = coords[1].Trim().Length > 0 ? Convert.ToDouble(coords[1].Trim()) : 0.0;
                }
            }
        }
        #endregion

        #region Public Properities
        public double X
        {
            get
            {
                return this._x;
            }
            set
            {
                this._x = value;
            }
        }
        public double Y
        {
            get
            {
                return this._y;
            }
            set
            {
                this._y = value;
            }
        }
        #endregion

        #region Public Override Methods
        public override string ToString()
        {
            return "(" + this._x.ToString() + "," + this._y.ToString() + ")";
        }
        #endregion
    }
}
