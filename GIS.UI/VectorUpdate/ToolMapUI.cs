using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GIS.UI.VectorUpdate
{
    public partial class ToolMapUI : Component
    {
        public ToolMapUI()
        {
            InitializeComponent();
        }

        public ToolMapUI(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
