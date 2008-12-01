using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace OutlookDesktop
{
    public partial class DesktopSettings : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private InstanceManager _instanceManager;

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
                log.Debug(String.Format("Processing: {0}",instance.InstanceName)); 
                TreeNode instanceNode = new TreeNode();
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
            if(e.Node.Tag == null) return;


            if (e.Node.Tag is MainForm)
            {
                MainForm instanceToEdit = e.Node.Tag as MainForm;

                InstanceSettings i = new InstanceSettings(instanceToEdit);

                splitContainer1.Panel2.Controls.Add(i);
            }
            

        }




    }
}
