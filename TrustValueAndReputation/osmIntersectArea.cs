using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrustValueAndReputation
{
    public class InterstArea
    {
        public InterstArea()
        { }

        public InterstArea(double intersectArea, int isArea)
        {
            _intersectArea = intersectArea;
            _isArea = isArea;
        }

        #region Model
        private double _intersectArea;
        private int _isArea;
        /// <summary>
        /// intersectarea
        /// </summary>
        public double IntersectArea
        {
            set { _intersectArea = value; }
            get { return _intersectArea; }
        }
        public int isArea
        {
            set { _isArea = value; }
            get { return _isArea; }
        }
        #endregion Model
    }
}


