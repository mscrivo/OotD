// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.Windows.Forms;
using OotD.Utility;

namespace OotD.Forms;

/// <summary>
///     Dialog for selecting a virtual desktop to assign an instance to
/// </summary>
public partial class VirtualDesktopSelectionDialog : Form
{
    private ListBox _desktopListBox = null!;
    private Button _okButton = null!;
    private Button _cancelButton = null!;
    private Label _instructionLabel = null!;

    /// <summary>
    ///     Gets the selected desktop GUID, or null if none selected
    /// </summary>
    public Guid? SelectedDesktopId { get; private set; }

    public VirtualDesktopSelectionDialog()
    {
        InitializeComponent();
        LoadVirtualDesktops();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        // Form properties
        Text = "Select Virtual Desktop";
        ClientSize = new Size(400, 300);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;

        // Instruction label
        _instructionLabel = new Label
        {
            Text = "Select the virtual desktop where you want this instance to appear:",
            Location = new Point(12, 12),
            Size = new Size(376, 40),
            AutoSize = false
        };
        Controls.Add(_instructionLabel);

        // Desktop list box
        _desktopListBox = new ListBox
        {
            Location = new Point(12, 60),
            Size = new Size(376, 180),
            SelectionMode = SelectionMode.One,
            DisplayMember = "Name"
        };
        _desktopListBox.DoubleClick += DesktopListBox_DoubleClick;
        Controls.Add(_desktopListBox);

        // OK button
        _okButton = new Button
        {
            Text = "OK",
            Location = new Point(232, 255),
            Size = new Size(75, 23),
            DialogResult = DialogResult.OK
        };
        _okButton.Click += OkButton_Click;
        Controls.Add(_okButton);

        // Cancel button
        _cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(313, 255),
            Size = new Size(75, 23),
            DialogResult = DialogResult.Cancel
        };
        Controls.Add(_cancelButton);

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        ResumeLayout(false);
    }

    private void LoadVirtualDesktops()
    {
        try
        {
            var desktops = VirtualDesktopManager.GetVirtualDesktops();
            var currentDesktopId = VirtualDesktopManager.GetCurrentDesktopId();

            if (desktops.Count == 0)
            {
                _instructionLabel.Text = "No virtual desktops found. You may need to create additional desktops in Windows.";
                _desktopListBox.Enabled = false;
                _okButton.Enabled = false;
                return;
            }

            foreach (var desktop in desktops)
            {
                var displayName = desktop.Name;

                // Mark the current desktop
                if (currentDesktopId.HasValue && desktop.Id == currentDesktopId.Value)
                {
                    displayName += " (Current)";
                }

                var item = new VirtualDesktopItem
                {
                    Id = desktop.Id,
                    Name = displayName
                };

                _desktopListBox.Items.Add(item);

                // Pre-select the current desktop
                if (currentDesktopId.HasValue && desktop.Id == currentDesktopId.Value)
                {
                    _desktopListBox.SelectedItem = item;
                }
            }

            // If nothing is selected, select the first item
            if (_desktopListBox.SelectedIndex == -1 && _desktopListBox.Items.Count > 0)
            {
                _desktopListBox.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            _instructionLabel.Text = $"Error loading virtual desktops: {ex.Message}";
            _desktopListBox.Enabled = false;
            _okButton.Enabled = false;
        }
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        if (_desktopListBox.SelectedItem is VirtualDesktopItem selectedItem)
        {
            SelectedDesktopId = selectedItem.Id;
        }
    }

    private void DesktopListBox_DoubleClick(object? sender, EventArgs e)
    {
        if (_desktopListBox.SelectedItem != null)
        {
            OkButton_Click(sender, e);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    /// <summary>
    ///     Item in the desktop list box
    /// </summary>
    private class VirtualDesktopItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;
    }
}
