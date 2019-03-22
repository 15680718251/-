using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrustValueAndReputation
{
    public class polygon
    {
        public polygon()
        { }

        /// <summary>
        /// 构造函数 polygon
        /// </summary>
        /// <param name="objectid">objectid</param>
        /// <param name="id">id</param>
        /// <param name="version">version</param>
        /// <param name="versionsub">versionsub</param>
        /// <param name="versionfinal">versionfinal</param>
        /// <param name="isArea">isArea</param>
        /// <param name="userid">userid</param>
        /// <param name="username">username</param>
        /// <param name="userReputation">userReputation</param>
        /// <param name="changeset">changeset</param>
        /// <param name="visible">visible</param>
        /// <param name="timestamp">timestamp</param>
        /// <param name="tags">tags</param>
        /// <param name="updateTime">updateTime</param>
        /// <param name="pointids">pointids</param>
        /// <param name="points">points</param>
        /// <param name="centroidX">centroidX</param>
        /// <param name="centroidY">centroidY</param>
        /// <param name="areaV">areaV</param>
        /// <param name="areaG">areaG</param>
        /// <param name="code">code</param>
        /// <param name="geomLine">geomLine</param>
        /// <param name="geom">geom</param>
        /// <param name="centrdiff">centrdiff</param>
        /// <param name="areadiffsim">areadiffsim</param>
        /// <param name="shapediffsim">shapediffsim</param>
        /// <param name="timestampDiff">timestampDiff</param>
        /// <param name="supportDegreeSimArea">supportDegreeSimArea</param>
        /// <param name="supportDegreeSimShape">supportDegreeSimShape</param>
        /// <param name="trustValue">信任值（可信度）</param>
        public polygon(long objectid, long id, int version, int versionsub, int versionfinal, int isArea, long userid, string username, double userReputation, long changeset, byte visible, DateTime timestamp, string tags, DateTime updateTime, string pointids, string points, decimal centroidX, decimal centroidY, decimal areaV, decimal areaG, string code, byte[] geomLine, byte[] geom, string centrdiff, string areadiffsim, string shapediffsim, string timestampDiff, string supportDegreeSimArea, string supportDegreeSimShape, double trustValue)
        {
            _objectid = objectid;
            _id = id;
            _version = version;
            _versionsub = versionsub;
            _versionfinal = versionfinal;
            _isArea = isArea;
            _userid = userid;
            _username = username;
            _userReputation = userReputation;
            _changeset = changeset;
            _visible = visible;
            _timestamp = timestamp;
            _tags = tags;
            _updateTime = updateTime;
            _pointids = pointids;
            _points = points;
            _centroidX = centroidX;
            _centroidY = centroidY;
            _areaV = areaV;
            _areaG = areaG;
            _code = code;
            _geomLine = geomLine;
            _geom = geom;
            _centrdiff = centrdiff;
            _areadiffsim = areadiffsim;
            _shapediffsim = shapediffsim;
            _timestampDiff = timestampDiff;
            _supportDegreeSimArea = supportDegreeSimArea;
            _supportDegreeSimShape = supportDegreeSimShape;
            _trustValue = trustValue;
        }

        #region Model
        private long _objectid;
        private long _id;
        private int _version;
        private int _versionsub;
        private int _versionfinal;
        private int _isArea;
        private long _userid;
        private string _username;
        private double _userReputation;
        private long _changeset;
        private byte _visible;
        private DateTime _timestamp;
        private string _tags;
        private DateTime _updateTime;
        private string _pointids;
        private string _points;
        private decimal _centroidX;
        private decimal _centroidY;
        private decimal _areaV;
        private decimal _areaG;
        private string _code;
        private byte[] _geomLine;
        private byte[] _geom;
        private string _centrdiff;
        private string _areadiffsim;
        private string _shapediffsim;
        private string _timestampDiff;
        private string _supportDegreeSimArea;
        private string _supportDegreeSimShape;
        private double _trustValue;
        /// <summary>
        /// objectid
        /// </summary>
        public long objectid
        {
            set { _objectid = value; }
            get { return _objectid; }
        }
        /// <summary>
        /// id
        /// </summary>
        public long id
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// version
        /// </summary>
        public int version
        {
            set { _version = value; }
            get { return _version; }
        }
        /// <summary>
        /// versionsub
        /// </summary>
        public int versionsub
        {
            set { _versionsub = value; }
            get { return _versionsub; }
        }
        /// <summary>
        /// versionfinal
        /// </summary>
        public int versionfinal
        {
            set { _versionfinal = value; }
            get { return _versionfinal; }
        }
        /// <summary>
        /// isArea
        /// </summary>
        public int isArea
        {
            set { _isArea = value; }
            get { return _isArea; }
        }
        /// <summary>
        /// userid
        /// </summary>
        public long userid
        {
            set { _userid = value; }
            get { return _userid; }
        }
        /// <summary>
        /// username
        /// </summary>
        public string username
        {
            set { _username = value; }
            get { return _username; }
        }
        /// <summary>
        /// userReputation
        /// </summary>
        public double userReputation
        {
            set { _userReputation = value; }
            get { return _userReputation; }
        }
        /// <summary>
        /// changeset
        /// </summary>
        public long   changeset
        {
            set { _changeset = value; }
            get { return _changeset; }
        }
        /// <summary>
        /// visible
        /// </summary>
        public byte visible
        {
            set { _visible = value; }
            get { return _visible; }
        }
        /// <summary>
        /// timestamp
        /// </summary>
        public DateTime timestamp
        {
            set { _timestamp = value; }
            get { return _timestamp; }
        }
        /// <summary>
        /// tags
        /// </summary>
        public string tags
        {
            set { _tags = value; }
            get { return _tags; }
        }
        /// <summary>
        /// updateTime
        /// </summary>
        public DateTime updateTime
        {
            set { _updateTime = value; }
            get { return _updateTime; }
        }
        /// <summary>
        /// pointids
        /// </summary>
        public string pointids
        {
            set { _pointids = value; }
            get { return _pointids; }
        }
        /// <summary>
        /// points
        /// </summary>
        public string points
        {
            set { _points = value; }
            get { return _points; }
        }
        /// <summary>
        /// centroidX
        /// </summary>
        public decimal centroidX
        {
            set { _centroidX = value; }
            get { return _centroidX; }
        }
        /// <summary>
        /// centroidY
        /// </summary>
        public decimal centroidY
        {
            set { _centroidY = value; }
            get { return _centroidY; }
        }
        /// <summary>
        /// areaV
        /// </summary>
        public decimal areaV
        {
            set { _areaV = value; }
            get { return _areaV; }
        }
        /// <summary>
        /// areaG
        /// </summary>
        public decimal areaG
        {
            set { _areaG = value; }
            get { return _areaG; }
        }
        /// <summary>
        /// code
        /// </summary>
        public string code
        {
            set { _code = value; }
            get { return _code; }
        }
        /// <summary>
        /// geomLine
        /// </summary>
        public byte[] geomLine
        {
            set { _geomLine = value; }
            get { return _geomLine; }
        }
        /// <summary>
        /// geom
        /// </summary>
        public byte[] geom
        {
            set { _geom = value; }
            get { return _geom; }
        }
        /// <summary>
        /// centrdiff
        /// </summary>
        public string centrdiff
        {
            set { _centrdiff = value; }
            get { return _centrdiff; }
        }
        /// <summary>
        /// areadiffsim
        /// </summary>
        public string areadiffsim
        {
            set { _areadiffsim = value; }
            get { return _areadiffsim; }
        }
        /// <summary>
        /// shapediffsim
        /// </summary>
        public string shapediffsim
        {
            set { _shapediffsim = value; }
            get { return _shapediffsim; }
        }
        /// <summary>
        /// timestampDiff
        /// </summary>
        public string timestampDiff
        {
            set { _timestampDiff = value; }
            get { return _timestampDiff; }
        }
        /// <summary>
        /// supportDegreeSimArea
        /// </summary>
        public string supportDegreeSimArea
        {
            set { _supportDegreeSimArea = value; }
            get { return _supportDegreeSimArea; }
        }
        /// <summary>
        /// supportDegreeSimShape
        /// </summary>
        public string supportDegreeSimShape
        {
            set { _supportDegreeSimShape = value; }
            get { return _supportDegreeSimShape; }
        }
        /// <summary>
        /// 信任值（可信度）
        /// </summary>
        public double trustValue
        {
            set { _trustValue = value; }
            get { return _trustValue; }
        }
        #endregion Model
    }
}

    


