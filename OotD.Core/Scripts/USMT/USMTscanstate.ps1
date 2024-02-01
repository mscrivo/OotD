<# Script to automate running of USMT scanstate in EPM for SFHS #>

<# Create directory based on %%Computer_Display_Name%% variable in USMT share #>
New-Item -ItemType Directory -Path \\sfmc.net\informationsystems\Endpoints\USMT\Storage\%%Computer_Display_Name%%\

<# Run scanstate.exe using MigUserCustom.xml to determine what folders/files to get, exclude sfmc\dsmruntime, sfmc\epmagent, local user accounts, and users who haven't logged in 90 days, and store data at \\sfmc.net\informationsystems\Endpoints\USMT\Storage\%%Computer_Display_Name%%\ #>

cd "\\sfmc.net\informationsystems\Endpoints\USMT\User State Migration Tool\amd64\"

.\scanstate.exe \\sfmc.net\informationsystems\Endpoints\USMT\Storage\%%Computer_Display_Name%%\ /i:MigUserCustom.xml /config:ConfigCustom.xml /ue:sfmc\dsmruntime /ue:sfmc\epmagent /ue:%%Computer_Display_Name%%\* /uel:90 /o
 