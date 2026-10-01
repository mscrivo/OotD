using System;
using System.Drawing;
using System.Windows.Forms;
using OotD.Enums;
using OotD.Events;
using OotD.Properties;

namespace OotD.Forms;

/// <summary>
///     Asks for the name of a new instance and which Outlook folder it should show.
/// </summary>
public sealed class NewInstanceDialog : Form
{
    private const int FieldWidth = 320;

    // Room on the right of each field for the error provider's icon.
    private const int ErrorIconSpace = 24;

    private readonly TextBox _nameTextBox;
    private readonly ComboBox _folderComboBox;
    private readonly ErrorProvider _errorProvider;
    private readonly InputBoxValidatingEventHandler _validator;

    public NewInstanceDialog(InputBoxValidatingEventHandler validator)
    {
        _validator = validator;
        _errorProvider = new ErrorProvider { ContainerControl = this };

        AutoScaleMode = AutoScaleMode.Font;
        AutoScaleDimensions = new SizeF(7F, 15F);

        Text = Resources.AddInstance;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;

        // Rows: [Name: textbox] [instructions] [Show: folder] [buttons]
        var mainLayout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(20, 20, 20, 16)
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var fieldMargin = new Padding(3, 6, ErrorIconSpace, 6);

        mainLayout.Controls.Add(CreateFieldLabel(Resources.NewInstanceNameLabel), 0, 0);
        _nameTextBox = new TextBox { Width = FieldWidth, Margin = fieldMargin };
        _nameTextBox.TextChanged += (_, _) => _errorProvider.SetError(_nameTextBox, "");
        mainLayout.Controls.Add(_nameTextBox, 1, 0);

        mainLayout.Controls.Add(new Label
        {
            Text = Resources.NewInstanceNameInstructions,
            AutoSize = true,
            MaximumSize = new Size(FieldWidth, 0),
            Margin = new Padding(3, 0, ErrorIconSpace, 16)
        }, 1, 1);

        mainLayout.Controls.Add(CreateFieldLabel(Resources.NewInstanceFolderLabel), 0, 2);
        _folderComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = FieldWidth,
            Margin = fieldMargin
        };
        _folderComboBox.Items.AddRange(
        [
            new FolderChoice(FolderViewType.Calendar, Resources.Calendar),
            new FolderChoice(FolderViewType.Contacts, Resources.Contacts),
            new FolderChoice(FolderViewType.Inbox, Resources.Inbox),
            new FolderChoice(FolderViewType.Notes, Resources.Notes),
            new FolderChoice(FolderViewType.Tasks, Resources.Tasks),
            new FolderChoice(FolderViewType.Todo, Resources.ResourceManager.GetString("TodoList")!),
            new FolderChoice(null, Resources.SelectFolder)
        ]);
        _folderComboBox.SelectedIndex = 0;
        mainLayout.Controls.Add(_folderComboBox, 1, 2);

        var buttonPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 20, ErrorIconSpace, 0)
        };

        var cancelButton = new Button
        {
            Text = "&Cancel",
            AutoSize = true,
            MinimumSize = new Size(96, 30),
            DialogResult = DialogResult.Cancel,
            CausesValidation = false,
            Margin = new Padding(0)
        };
        buttonPanel.Controls.Add(cancelButton);

        var okButton = new Button
        {
            Text = "&OK",
            AutoSize = true,
            MinimumSize = new Size(96, 30),
            Margin = new Padding(0, 0, 10, 0)
        };
        okButton.Click += OkButton_Click;
        buttonPanel.Controls.Add(okButton);

        mainLayout.Controls.Add(buttonPanel, 0, 3);
        mainLayout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(mainLayout);

        AcceptButton = okButton;
        CancelButton = cancelButton;
    }

    /// <summary>
    ///     The name entered for the new instance.
    /// </summary>
    public string InstanceName => _nameTextBox.Text;

    /// <summary>
    ///     The default Outlook folder the new instance should show, or null when the user chose to pick another
    ///     folder.
    /// </summary>
    public FolderViewType? SelectedFolder => ((FolderChoice)_folderComboBox.SelectedItem!).FolderViewType;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _errorProvider.Dispose();
        }

        base.Dispose(disposing);
    }

    private static Label CreateFieldLabel(string text)
    {
        return new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) };
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        var args = new InputBoxValidatingEventArgs { Text = _nameTextBox.Text };
        _validator(this, args);
        if (args.Cancel)
        {
            _errorProvider.SetError(_nameTextBox, args.Message);
            _nameTextBox.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
    }

    private sealed class FolderChoice(FolderViewType? folderViewType, string displayName)
    {
        public FolderViewType? FolderViewType { get; } = folderViewType;

        public override string ToString() => displayName;
    }
}
