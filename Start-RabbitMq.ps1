[CmdletBinding()]
param (
    # Start Browser
    [Parameter()]
    [switch]
    $StartBrowser
)

docker run -d --hostname $($ENV:COMPUTERNAME) --name rabbitmq -p 15672:15672 -p 5672:5672 rabbitmq:3-management
if ($StartBrowser) {
    Start-Process "http://localhost:15672/#/"
}