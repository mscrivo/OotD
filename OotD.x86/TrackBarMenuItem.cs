// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace OotD.Utility
{
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.MenuStrip |
                                   ToolStripItemDesignerAvailability.ContextMenuStrip)]
    public class TrackBarMenuItem : ToolStripControlHost
    {
        private readonly TrackBar? trackBar;

        public TrackBarMenuItem() : base(new TrackBar())
        {
            trackBar = Control as TrackBar;
        }

        // Add properties, events etc. you want to expose...
    }
}
