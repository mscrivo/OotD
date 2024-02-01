while ($true) {
	Start-Process -FilePath "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" -ArgumentList "--kiosk https://www.sfmc.net/fitness-plus/membership/join/ --edge-kiosk-type=fullscreen" -Wait
	}