param (
    [string]$databasePassword
)

Add-WindowsCapability -Name Rsat.ActiveDirectory.DS-LDS.Tools~~~~0.0.1.0 -Online

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Data

# Database connection details
$databaseServer = "EPM-DB-PV202"
$databaseName = "EPMDB"
$databaseUsername = "epmconnection"

# Create a form
$form = New-Object System.Windows.Forms.Form
$form.Text = "Owner's Employee Number"
$form.Size = New-Object System.Drawing.Size(300,150)
$form.StartPosition = "CenterScreen"

# Create a label
$label = New-Object System.Windows.Forms.Label
$label.Location = New-Object System.Drawing.Point(10,20)
$label.Size = New-Object System.Drawing.Size(280,20)
$label.Text = "Enter the employee number for the assigned user:"
$form.Controls.Add($label)

# Create a textbox
$textbox = New-Object System.Windows.Forms.TextBox
$textbox.Location = New-Object System.Drawing.Point(10,40)
$textbox.Size = New-Object System.Drawing.Size(260,20)
$textbox.Add_KeyDown({
    if ($_.KeyCode -eq "Enter") {
        $form.DialogResult = [System.Windows.Forms.DialogResult]::OK
        $form.Close()
    }
})
$form.Controls.Add($textbox)

# Create an OK button
$button = New-Object System.Windows.Forms.Button
$button.Location = New-Object System.Drawing.Point(100,80)
$button.Size = New-Object System.Drawing.Size(75,23)
$button.Text = "OK"
$button.DialogResult = [System.Windows.Forms.DialogResult]::OK
$form.Controls.Add($button)

# Handle form's closing event
$form.Add_FormClosing({
    if ($textbox.Text -eq "") {
        $_.Cancel = $true
        Write-Host "Please enter an employee number before closing the window."
    }
})

try {
    Import-Module -Name ActiveDirectory
    
    $creds = Get-Credential

    # Show the form
    $result = $form.ShowDialog()

    # Check the result
    if ($result -eq [System.Windows.Forms.DialogResult]::OK) {
        $employeeNumber = $textbox.Text
        $employee = Get-ADUser -Filter { Description -eq $employeeNumber } -Credential $creds | Select-Object -ExpandProperty DistinguishedName
        
        if ($employee) {
            # Construct the SQL query to update the asset owner field
            $query = "UPDATE Computer SET PrimaryOwner = '$employee' WHERE DeviceName = '$env:COMPUTERNAME';"

            # Create a SqlConnection object
            $connectionString = "Server=$databaseServer;Database=$databaseName;User Id=$databaseUsername;Password=$databasePassword;"
            $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
            $sqlConnection.ConnectionString = $connectionString
            $sqlConnection.Open()

            # Create a SqlCommand object
            $sqlCommand = $sqlConnection.CreateCommand()
            $sqlCommand.CommandText = $query

            # Execute the query
            $sqlCommand.ExecuteNonQuery()

            Write-Host "Asset owner field updated successfully."

            # Close the connection
            $sqlConnection.Close()
        } else {
            Write-Host "Invalid employee number. Please enter a valid employee number from Active Directory."
        }
    } else {
        Write-Host "Employee number cannot be empty. Please try again."
    }
} catch {
    Write-Host "An error occurred: $_"
} finally {
    $form.Dispose()
}
