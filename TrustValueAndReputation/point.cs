using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrustValueAndReputation
{
    public class point
    {
        public point()
        { }

        /// <summary>
        /// 构造函数 point
        /// </summary>
        /// <param name="objectid">objectid</param>
        /// <param name="id">id</param>
        /// <param name="version">version</param>
        /// <param name="userid">userid</param>
        /// <param name="username">username</param>
        /// <param name="lon">lon</param>
        /// <param name="lat">lat</param>
        /// <param name="changeset">changeset</param>
        /// <param name="visible">visible</param>
        /// <param name="tags">tags</param>
        /// <param name="timestamp">timestamp</param>
        /// <param name="updateTime">updateTime</param>
        public point(long objectid, long id, int version, long userid, string username, double lon, double lat, long changeset, string visible, string tags, DateTime timestamp)
        {
            _objectid = objectid;
            _id = id;
            _version = version;
            _userid = userid;
            _username = username;
            _lon = lon;
            _lat = lat;
            _changeset = changeset;
            _visible = visible;
            _tags = tags;
            _timestamp = timestamp;
        }

        #region Model
        private long _objectid;
        private long _id;
        private int _version;
        private long _userid;
        private string _username;
        private double _lon;
        private double _lat;
        private long _changeset;
        private string _visible;
        private string _tags;
        private DateTime _timestamp;

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
        /// lon
        /// </summary>
        public double lon
        {
            set { _lon = value; }
            get { return _lon; }
        }
        /// <summary>
        /// lat
        /// </summary>
        public double lat
        {
            set { _lat = value; }
            get { return _lat; }
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
        /// visible
        /// </summary>
        public string visible
        {
            set { _visible = value; }
            get { return _visible; }
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
        /// timestamp
        /// </summary>
        public DateTime timestamp
        {
            set { _timestamp = value; }
            get { return _timestamp; }
        }

        #endregion Model
    }
}

