using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GISEditor.EditorTool.BasicClass
{
    public partial class ParameterMapUI : Component
    {
        public ParameterMapUI()
        {
            InitializeComponent();
        }

        public ParameterMapUI(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
