param(
	[int]
	$Port = 5201
)

function Get-Listeners {
	$conns = @()
	try {
		$conns = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction Stop
	} catch {
		# fallback to netstat parsing if Get-NetTCPConnection not available
		$net = netstat -ano | Select-String ":$Port\s"
		if ($net) {
			$lines = $net -split "\r?\n" | Where-Object { $_ -ne "" }
			foreach ($l in $lines) {
				$parts = ($l -split '\s+') | Where-Object { $_ -ne '' }
				# columns vary; PID is last
				$pid = $parts[-1]
				$conns += [PSCustomObject]@{ OwningProcess = [int]$pid }
			}
		}
	}
	return $conns
}

Write-Host "Inspecting listeners on port $Port..." -ForegroundColor Cyan
$listeners = Get-Listeners
if (-not $listeners -or $listeners.Count -eq 0) {
	Write-Host "No process is listening on port $Port." -ForegroundColor Green
	exit 0
}

$pids = $listeners.OwningProcess | Sort-Object -Unique
Write-Host "Found listener PID(s): $($pids -join ', ')" -ForegroundColor Yellow

foreach ($pid in $pids) {
	try {
		$proc = Get-Process -Id $pid -ErrorAction Stop
		Write-Host "PID: $($proc.Id)  Name: $($proc.ProcessName)  Path: $($proc.Path)" -ForegroundColor White
	} catch {
		Write-Host "PID: $pid (process info unavailable)" -ForegroundColor DarkYellow
	}
}

$confirm = Read-Host "Kill these PID(s)? Type Y to confirm"
if ($confirm -notmatch '^[Yy]') {
	Write-Host "Aborted. No processes killed." -ForegroundColor Yellow
	exit 0
}

foreach ($pid in $pids) {
	try {
		Stop-Process -Id $pid -Force -ErrorAction Stop
		Write-Host "Killed PID $pid" -ForegroundColor Green
	} catch {
		Write-Host "Failed to kill PID $pid: $($_.Exception.Message)" -ForegroundColor Red
	}
}

Start-Sleep -Milliseconds 500
# verify
$listeners = Get-Listeners
if (-not $listeners -or $listeners.Count -eq 0) {
	Write-Host "Port $Port is now free." -ForegroundColor Green
	exit 0
} else {
	Write-Host "Port $Port is still in use. Remaining PID(s): $($listeners.OwningProcess | Sort-Object -Unique -Join ', ')" -ForegroundColor Red
	exit 2
}
