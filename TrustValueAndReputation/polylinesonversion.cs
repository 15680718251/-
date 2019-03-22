using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrustValueAndReputation
{
    public class polylinesonversion
    {
         public polylinesonversion()
        { }

        /// <summary>
        /// 构造函数 polygonsonversion
        /// </summary>
        /// <param name="objectid">objectid</param>
        /// <param name="id">id</param>
        /// <param name="version">version</param>
        /// <param name="versionsub">versionsub</param>
        /// <param name="versionfinal">versionfinal</param>
        /// <param name="userid">userid</param>
        /// <param name="username">username</param>
        /// <param name="userReputation">userReputation</param>
        /// <param name="changeset">changeset</param>
        /// <param name="timestamp">timestamp</param>
        /// <param name="tag">tag</param>
        /// <param name="pointids">pointids</param>
        /// <param name="points">points</param>
        /// <param name="trustValue">trustValue</param>
        /// <param name="trustValue">areaV</param>
        /// <param name="trustValue">areaG</param>
        /// <param name="trustValue">areadiffsim</param>
         public polylinesonversion(int isArea, long objectid, long id, int version, int versionsub, int versionfinal, long userid, string username, double userReputation, long changeset, DateTime timestamp, string tags, string pointids, string points, double trustValue, string geom, double areaV, double areaG, double centroidX, double centroidY, decimal areadiffsim, decimal shapediffsim, string geomline)
        {
            _isArea = isArea;
            _objectid = objectid;
            _id = id;
            _version = version;
            _versionsub = versionsub;
            _versionfinal = versionfinal;
            _userid = userid;
            _username = username;
            _userReputation = userReputation;
            _changeset = changeset;
            _timestamp = timestamp;
            _tags = tags;
            _pointids = pointids;
            _points = points;
            _trustValue = trustValue;
            _geom = geom;
            _areaV = areaV;
            _areaG = areaG;
            _centroidX = centroidX;
            _centroidY = centroidY;
            _areadiffsim = areadiffsim;
            _shapediffsim = shapediffsim;
            _geomline=geomline;
        }

        #region Model
        private int _isArea;
        private long _objectid;
        private long _id;
        private int _version;
        private int _versionsub;
        private int _versionfinal;
        private long _userid;
        private string _username;
        private double _userReputation;
        private long _changeset;
        private DateTime _timestamp;
        private string _tags;
        private string _pointids;
        private string _points;
        private double _trustValue;
        private string _geom;
        private double _areaV;
        private double _areaG;
        private double _centroidX;
        private double _centroidY;
        private decimal _areadiffsim;
        private decimal _shapediffsim;
        private string _geomline;

        /// <summary>
        /// isArea
        /// </summary>
        public int isArea
        {
            set { _isArea = value; }
            get { return _isArea; }
        }

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
        public long changeset
        {
            set { _changeset = value; }
            get { return _changeset; }
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
        /// tag
        /// </summary>
        public string tags
        {
            set { _tags = value; }
            get { return _tags; }
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
        /// trustValue
        /// </summary>
        public double trustValue
        {
            set { _trustValue = value; }
            get { return _trustValue; }
        }
        /// <summary>
        /// geom
        /// </summary>
        public string geom
        {
            set { _geom = value; }
            get { return _geom; }
        }
        /// <summary>
        /// centroidX
        /// </summary>
        public double centroidX
        {
            set { _centroidX = value; }
            get { return _centroidX; }
        }
        /// <summary>
        /// centroidY
        /// </summary>
        public double centroidY
        {
            set { _centroidY = value; }
            get { return _centroidY; }
        }
        /// <summary>
        /// areaV
        /// </summary>
        public double areaV
        {
            set { _areaV = value; }
            get { return _areaV; }
        }
        /// <summary>
        /// areaG
        /// </summary>
        public double areaG
        {
            set { _areaG = value; }
            get { return _areaG; }
        }
        /// <summary>
        /// areadiffsim
        /// </summary>
        public decimal areadiffsim
        {
            set { _areadiffsim = value; }
            get { return _areadiffsim; }
        }
        /// <summary>
        /// shapediffsim
        /// </summary>
        public decimal shapediffsim
        {
            set { _shapediffsim = value; }
            get { return _shapediffsim; }
        }
        /// <summary>
        /// geomline
        /// </summary>
        public string geomline
        {
            set { _geomline = value; }
            get { return _geomline; }
        }
    
        #endregion Model
    }
    
}
