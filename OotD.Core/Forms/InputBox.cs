using System;
using System.ComponentModel;
using System.Windows.Forms;
using OotD.Events;

namespace OotD.Forms;

/// <summary>
///     Delegate used to Validate an InputBox
/// </summary>
public delegate void InputBoxValidatingEventHandler(object sender, InputBoxValidatingEventArgs e);

public partial class InputBox : Form
{
    private InputBox()
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
    ///     Displays an Input Box with validation.
    /// </summary>
    /// <param name="owner">The form's owner.</param>
    /// <param name="instructions">Instructions to present above the input box.</param>
    /// <param name="caption">The form's caption</param>
    /// <param name="defaultValue">The default value to place in the Inbox Box.</param>
    /// <param name="validator">A validator method that performs validation on the user's input.</param>
    /// <returns></returns>
    public static InputBoxResult Show(Form owner, string instructions, string caption, string defaultValue,
        InputBoxValidatingEventHandler validator)
    {
        return Show(owner, instructions, caption, defaultValue, validator, form => form.ShowDialog());
    }

    /// <summary>
    ///     Overload that lets tests replace the modal <see cref="Form.ShowDialog()" /> call.
    /// </summary>
    internal static InputBoxResult Show(Form owner, string instructions, string caption, string defaultValue,
        InputBoxValidatingEventHandler validator, Func<InputBox, DialogResult> showDialog)
    {
        using var form = new InputBox();
        form.Owner = owner;
        form.PromptLabel.Text = instructions;
        form.Text = caption;
        form.InputTextBox.Text = defaultValue;
        form.Validator = validator;

        var result = showDialog(form);

        var inputBoxResult = new InputBoxResult();
        if (result != DialogResult.OK)
        {
            return inputBoxResult;
        }

        inputBoxResult.Text = form.InputTextBox.Text;
        inputBoxResult.Ok = true;
        return inputBoxResult;
    }

    private void InputTextBox_TextChanged(object sender, EventArgs e)
    {
        _errorProviderText.SetError(InputTextBox, "");
    }

    private void InputTextBox_Validating(object sender, CancelEventArgs e)
    {
        if (Validator == null)
        {
            return;
        }

        var args = new InputBoxValidatingEventArgs { Text = InputTextBox.Text };
        Validator(this, args);
        if (!args.Cancel)
        {
            return;
        }

        e.Cancel = true;
        _errorProviderText.SetError(InputTextBox, args.Message);
    }
}
