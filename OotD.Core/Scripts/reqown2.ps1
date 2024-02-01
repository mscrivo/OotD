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

# Function to prompt for credentials interactively
function Get-InteractiveCredential {
    param (
        [string]$Message,
        [string]$Title
    )

    $credential = $null
    $form = New-Object System.Windows.Forms.Form
    $form.Text = $Title
    $form.Size = New-Object System.Drawing.Size(350, 225)
    $form.StartPosition = "CenterScreen"

    $label = New-Object System.Windows.Forms.Label
    $label.Location = New-Object System.Drawing.Point(10, 20)
    $label.Size = New-Object System.Drawing.Size(280, 20)
    $label.Text = $Message
    $form.Controls.Add($label)

    $usernameLabel = New-Object System.Windows.Forms.Label
    $usernameLabel.Location = New-Object System.Drawing.Point(10, 50)
    $usernameLabel.Size = New-Object System.Drawing.Size(80, 20)
    $usernameLabel.Text = "AD Username:"
    $form.Controls.Add($usernameLabel)

    $usernameTextBox = New-Object System.Windows.Forms.TextBox
    $usernameTextBox.Location = New-Object System.Drawing.Point(100, 50)
    $usernameTextBox.Size = New-Object System.Drawing.Size(180, 20)
    $form.Controls.Add($usernameTextBox)

    $passwordLabel = New-Object System.Windows.Forms.Label
    $passwordLabel.Location = New-Object System.Drawing.Point(10, 80)
    $passwordLabel.Size = New-Object System.Drawing.Size(80, 20)
    $passwordLabel.Text = "AD Password:"
    $form.Controls.Add($passwordLabel)

    $passwordTextBox = New-Object System.Windows.Forms.TextBox
    $passwordTextBox.Location = New-Object System.Drawing.Point(100, 80)
    $passwordTextBox.Size = New-Object System.Drawing.Size(180, 20)
    $passwordTextBox.UseSystemPasswordChar = $true
    $form.Controls.Add($passwordTextBox)

    $employeeNumberLabel = New-Object System.Windows.Forms.Label
    $employeeNumberLabel.Location = New-Object System.Drawing.Point(10, 110)
    $employeeNumberLabel.Size = New-Object System.Drawing.Size(150, 20)
    $employeeNumberLabel.Text = "Owner's Employee Number:"
    $form.Controls.Add($employeeNumberLabel)

    $employeeNumberTextBox = New-Object System.Windows.Forms.TextBox
    $employeeNumberTextBox.Location = New-Object System.Drawing.Point(160, 110)
    $employeeNumberTextBox.Size = New-Object System.Drawing.Size(140, 20)
    $form.Controls.Add($employeeNumberTextBox)

    $button = New-Object System.Windows.Forms.Button
    $button.Location = New-Object System.Drawing.Point(125, 150)
    $button.Size = New-Object System.Drawing.Size(75, 23)
    $button.Text = "OK"
    $button.DialogResult = [System.Windows.Forms.DialogResult]::OK
    $form.Controls.Add($button)

    $form.Add_FormClosing({
        if ($usernameTextBox.Text -eq "" -or $passwordTextBox.Text -eq "" -or $employeeNumberTextBox.Text -eq "") {
            $_.Cancel = $true
            Write-Host "Please enter all required information before closing the window."
        }
    })

    $result = $form.ShowDialog()

    if ($result -eq [System.Windows.Forms.DialogResult]::OK) {
        $securePassword = ConvertTo-SecureString -String $passwordTextBox.Text -AsPlainText -Force
        $credential = New-Object System.Management.Automation.PSCredential($usernameTextBox.Text, $securePassword)
        $employeeNumber = $employeeNumberTextBox.Text
    }

    $form.Dispose()
    return $credential, $employeeNumber
}

try {
    Import-Module -Name ActiveDirectory

    # Prompt for credentials and employee number interactively
    $creds, $employeeNumber = Get-InteractiveCredential -Message "Enter your credentials" -Title "Credential Prompt"

    # Check if credentials are provided
    if ($creds -eq $null -or $employeeNumber -eq "") {
        Write-Host "Credentials or employee number not provided. Exiting."
        exit
    }

    # Use AD credentials to run Get-ADUser
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
} catch {
    Write-Host "An error occurred: $_"
} finally {
    if ($sqlConnection.State -eq 'Open') {
        $sqlConnection.Close()
    }
}
