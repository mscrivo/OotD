while ($true) {
	Start-Process -FilePath "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" -ArgumentList "--kiosk https://www.corsium.com/ccpasvc --edge-kiosk-type=fullscreen" -Wait
	}