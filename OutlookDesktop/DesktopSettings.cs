using System;
using System.Reflection;
using System.Windows.Forms;
using log4net;

namespace OutlookDesktop
{
    public partial class DesktopSettings : Form
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly InstanceManager _instanceManager;

        public DesktopSettings(InstanceManager instanceManager)
        {
            InitializeComponent();

            _instanceManager = instanceManager;
        }

        #region Event Handlers

        /// <summary>
        /// Setups up the UI and loads the instances settings. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DesktopSettings_Load(object sender, EventArgs e)
        {
            //Setup the UI

            // Get the base node.
            TreeNode baseNode = uxSettingsTree.Nodes[0];

            if (baseNode == null)
            {
                log.Error("Base Node missing in Settings Form");
                return;
            }

            // Get the application instances.
            log.Debug("Iterating through the instances.");
            foreach (MainForm instance in _instanceManager.Instances)
            {
                log.Debug(String.Format("Processing: {0}", instance.InstanceName));
                var instanceNode = new TreeNode();
                instanceNode.Text = instance.InstanceName;
                instanceNode.Tag = instance;


                log.Debug("Adding instance to tree");
                baseNode.Nodes.Add(instanceNode);
            }
        }

        #endregion

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {
        }

        private void uxSettingsTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            log.Debug("Contract: Check that the tag contains a object");
            if (e.Node.Tag == null) return;


            if (e.Node.Tag is MainForm)
            {
                var instanceToEdit = e.Node.Tag as MainForm;

                var i = new InstanceSettings(instanceToEdit);

                splitContainer1.Panel2.Controls.Add(i);
            }
        }
    }
}