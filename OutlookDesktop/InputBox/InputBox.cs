using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OutlookDesktop
{
    /// <summary>
    /// Delegate used to Validate an InputBox
    /// </summary>
    public delegate void InputBoxValidatingEventHandler(object sender, InputBoxValidatingEventArgs e);

    public partial class InputBox : Form
    {
        /// <summary>
        /// Delegate used to validate the object
        /// </summary>
        private InputBoxValidatingEventHandler _validator;

        private InputBox()
        {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, System.EventArgs e)
        {
            this.Validator = null;
            this.Close();
        }

        private void OKButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Displays an Input Box with validation.
        /// </summary>
        /// <param name="owner">The form's owner.</param>
        /// <param name="instructions">Instructions to present above the input box.</param>
        /// <param name="caption">The form's caption</param>
        /// <param name="defaultValue">The default value to place in the Inbox Box.</param>
        /// <param name="validator">A validator method that peforms validation on the user's input.</param>
        /// <returns></returns>
        public static InputBoxResult Show(Form owner, string instructions, string caption, string defaultValue, InputBoxValidatingEventHandler validator)
        {
            using (InputBox form = new InputBox())
            {
                form.Owner = owner;
                form.PromptLabel.Text = instructions;
                form.Text = caption;
                form.InputTextBox.Text = defaultValue;
                form.Validator = validator;

                DialogResult result = form.ShowDialog();

                InputBoxResult retval = new InputBoxResult();
                if (result == DialogResult.OK)
                {
                    retval.Text = form.InputTextBox.Text;
                    retval.Ok = true;
                }
                return retval;
            }
        }

        private void InputTextBox_TextChanged(object sender, System.EventArgs e)
        {
            _errorProviderText.SetError(InputTextBox, "");
        }

        private void InputTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Validator != null)
            {
                InputBoxValidatingEventArgs args = new InputBoxValidatingEventArgs();
                args.Text = InputTextBox.Text;
                Validator(this, args);
                if (args.Cancel)
                {
                    e.Cancel = true;
                    _errorProviderText.SetError(InputTextBox, args.Message);
                }
            }
        }

        protected InputBoxValidatingEventHandler Validator
        {
            get { return (_validator); }
            set { _validator = value; }
        }
    }
}
