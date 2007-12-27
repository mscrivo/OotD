using System;
using System.Collections.Generic;
using System.Text;

namespace OutlookDesktop
{
    public class InstanceRemovedEventArgs : EventArgs
    {
        public String InstanceName
        {
            get { return _instanceName; }
            set { _instanceName = value; }
        }
        private String _instanceName;

        public InstanceRemovedEventArgs(String instanceName)
        {
            this._instanceName = instanceName;
        }
    }
}
