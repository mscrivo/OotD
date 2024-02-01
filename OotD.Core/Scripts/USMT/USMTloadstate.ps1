<# Script to automate running of USMT loadstate in EPM for SFHS #>

<#  Run loadstate.exe using MigUserCustom.xml to determine what folders/files to get, exclude sfmc\dsmruntime, sfmc\epmagent, local user accounts, and users who haven't logged in 90 days, and load data from \\sfmc.net\informationsystems\Endpoints\USMT\Storage\%%Computer_Display_Name%%\ #>

cd "\\sfmc.net\informationsystems\Endpoints\USMT\User State Migration Tool\amd64\"
.\loadstate.exe \\sfmc.net\informationsystems\Endpoints\USMT\Storage\%%Computer_Display_Name%%\ /i:MigUserCustom.xml /config:ConfigCustom.xml /ue:sfmc\dsmruntime /ue:sfmc\epmagent /ue:%%Computer_Display_Name%%\* /uel:90