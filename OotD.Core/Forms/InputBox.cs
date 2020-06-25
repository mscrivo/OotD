// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using OotD.Events;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OotD.Forms
{
    /// <summary>
    /// Delegate used to Validate an InputBox
    /// </summary>
    public delegate void InputBoxValidatingEventHandler(object sender, InputBoxValidatingEventArgs e);

    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
        }

        private InputBoxValidatingEventHandler? Validator { get; set; }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Validator = null;
            Close();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Displays an Input Box with validation.
        /// </summary>
        /// <param name="owner">The form's owner.</param>
        /// <param name="instructions">Instructions to present above the input box.</param>
        /// <param name="caption">The form's caption</param>
        /// <param name="defaultValue">The default value to place in the Inbox Box.</param>
        /// <param name="validator">A validator method that performs validation on the user's input.</param>
        /// <returns></returns>
        public static InputBoxResult Show(Form owner, string instructions, string caption, string defaultValue, InputBoxValidatingEventHandler validator)
        {
            using var form = new InputBox
            {
                Owner = owner,
                PromptLabel = { Text = instructions },
                Text = caption,
                InputTextBox = { Text = defaultValue },
                Validator = validator
            };

            var result = form.ShowDialog();

            var inputBoxResult = new InputBoxResult();
            if (result == DialogResult.OK)
            {
                inputBoxResult.Text = form.InputTextBox.Text;
                inputBoxResult.Ok = true;
            }
            return inputBoxResult;
        }

        private void InputTextBox_TextChanged(object sender, EventArgs e)
        {
            _errorProviderText.SetError(InputTextBox, "");
        }

        private void InputTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (Validator != null)
            {
                var args = new InputBoxValidatingEventArgs { Text = InputTextBox.Text };
                Validator(this, args);
                if (args.Cancel)
                {
                    e.Cancel = true;
                    _errorProviderText.SetError(InputTextBox, args.Message);
                }
            }
        }
    }
}
