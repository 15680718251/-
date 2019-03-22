using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace 全球典型要素数据库增量更新系统.NewFolder1
{
    public partial class MapUI : Component
    {
        public MapUI()
        {
            InitializeComponent();
        }

        public MapUI(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
