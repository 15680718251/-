using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GIS.UI
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
