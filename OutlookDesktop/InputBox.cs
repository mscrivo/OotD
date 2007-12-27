using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace OutlookDesktop
{
    public class InputBox : System.Windows.Forms.Form
    {
        protected System.Windows.Forms.Button ButtonOk
        {
            get { return _buttonOk; }
            set { _buttonOk = value; }
        }
        private System.Windows.Forms.Button _buttonOk;

        protected System.Windows.Forms.Button ButtonCancel
        {
            get { return _buttonCancel; }
            set { _buttonCancel = value; }
        }
        private System.Windows.Forms.Button _buttonCancel;

        protected System.Windows.Forms.Label LabelPrompt
        {
            get { return _labelPrompt; }
            set { _labelPrompt = value; }
        }
        private System.Windows.Forms.Label _labelPrompt;

        protected System.Windows.Forms.TextBox TextBoxText
        {
            get { return _textBoxText; }
            set { _textBoxText = value; }
        }
        private System.Windows.Forms.TextBox _textBoxText;

        protected System.Windows.Forms.ErrorProvider ErrorProviderText
        {
            get { return _errorProviderText; }
            set { _errorProviderText = value; }
        }
        private System.Windows.Forms.ErrorProvider _errorProviderText;

        private IContainer components;

        /// <summary>
        /// Delegate used to validate the object
        /// </summary>
        private InputBoxValidatingEventHandler _validator;

        private InputBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._textBoxText = new System.Windows.Forms.TextBox();
            this._labelPrompt = new System.Windows.Forms.Label();
            this._errorProviderText = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this._errorProviderText)).BeginInit();
            this.SuspendLayout();
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.Location = new System.Drawing.Point(127, 69);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 2;
            this._buttonOk.Text = "OK";
            this._buttonOk.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.CausesValidation = false;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(215, 69);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 3;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // _textBoxText
            // 
            this._textBoxText.Location = new System.Drawing.Point(16, 32);
            this._textBoxText.Name = "_textBoxText";
            this._textBoxText.Size = new System.Drawing.Size(274, 20);
            this._textBoxText.TabIndex = 1;
            this._textBoxText.TextChanged += new System.EventHandler(this.textBoxText_TextChanged);
            this._textBoxText.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxText_Validating);
            // 
            // _labelPrompt
            // 
            this._labelPrompt.AutoSize = true;
            this._labelPrompt.Location = new System.Drawing.Point(15, 15);
            this._labelPrompt.Name = "_labelPrompt";
            this._labelPrompt.Size = new System.Drawing.Size(39, 13);
            this._labelPrompt.TabIndex = 0;
            this._labelPrompt.Text = "prompt";
            // 
            // _errorProviderText
            // 
            this._errorProviderText.ContainerControl = this;
            this._errorProviderText.DataMember = "";
            // 
            // InputBox
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(312, 104);
            this.Controls.Add(this._labelPrompt);
            this.Controls.Add(this._textBoxText);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Title";
            ((System.ComponentModel.ISupportInitialize)(this._errorProviderText)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            this.Validator = null;
            this.Close();
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Displays a prompt in a dialog box, waits for the user to input text or click a button.
        /// </summary>
        /// <param name="prompt">String expression displayed as the message in the dialog box</param>
        /// <param name="title">String expression displayed in the title bar of the dialog box</param>
        /// <param name="defaultResponse">String expression displayed in the text box as the default response</param>
        /// <param name="validator">Delegate used to validate the text</param>
        /// <returns>An InputBoxResult object with the Text and the OK property set to true when OK was clicked.</returns>
        public static InputBoxResult Show(Form owner, string prompt, string title, string defaultResponse, InputBoxValidatingEventHandler validator)
        {
            using (InputBox form = new InputBox())
            {
                form.Owner = owner;
                form._labelPrompt.Text = prompt;
                form.Text = title;
                form._textBoxText.Text = defaultResponse;
                form.Validator = validator;

                DialogResult result = form.ShowDialog();

                InputBoxResult retval = new InputBoxResult();
                if (result == DialogResult.OK)
                {
                    retval.Text = form._textBoxText.Text;
                    retval.Ok = true;
                }
                return retval;
            }
        }

        /// <summary>
        /// Reset the ErrorProvider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxText_TextChanged(object sender, System.EventArgs e)
        {
            _errorProviderText.SetError(_textBoxText, "");
        }

        /// <summary>
        /// Validate the Text using the Validator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxText_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Validator != null)
            {
                InputBoxValidatingEventArgs args = new InputBoxValidatingEventArgs();
                args.Text = _textBoxText.Text;
                Validator(this, args);
                if (args.Cancel)
                {
                    e.Cancel = true;
                    _errorProviderText.SetError(_textBoxText, args.Message);
                }
            }
        }

        protected InputBoxValidatingEventHandler Validator
        {
            get { return (_validator); }
            set { _validator = value; }
        }
    }

    /// <summary>
    /// Class used to store the result of an InputBox.Show message.
    /// </summary>
    public class InputBoxResult
    {
        public bool Ok
        {
            get { return _Ok; }
            set { _Ok = value; }
        }
        private bool _Ok;

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        private string _text;
    }

    /// <summary>
    /// EventArgs used to Validate an InputBox
    /// </summary>
    public class InputBoxValidatingEventArgs : EventArgs
    {
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        private string _text;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        private string _message;

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
        private bool _cancel;
    }

    /// <summary>
    /// Delegate used to Validate an InputBox
    /// </summary>
    public delegate void InputBoxValidatingEventHandler(object sender, InputBoxValidatingEventArgs e);
}