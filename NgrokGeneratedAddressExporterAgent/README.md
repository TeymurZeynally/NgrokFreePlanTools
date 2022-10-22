# Description
**NgrokGeneratedAddressExporterAgent** This is a service/daemon that starts ngrok at system startup and sends the generated address to the specified HTTP endpoint via curl.

# Configuration
* NgrokPath - path to ngrok command
* NgrokWorkingDirectory - directory where the ngrok command will be run
* NgrokArguments - ngrok arguments
* CurlPath - path to the curl command
* CurlWorkingDirectory - directory where the curl command will be run
* CurlArguments - curl arguments, **{{Address}}** will be filled with the generated address

```YAML
NgrokPath: ngrok
NgrokWorkingDirectory: .
NgrokArguments: tcp 1234
CurlPath: curl
CurlWorkingDirectory: .
CurlArguments: |
  -X PUT https://your-endpoint.com -H "Authorization:12345" -d "{ \"address\": \"{{Address}}\" }"

Serilog:
  Using:
    - Serilog.Sinks.Console
    - Serilog.Sinks.File
  MinimumLevel: Debug
  WriteTo:
    - Name: Console
    - Name: File
      Args:
        rollingInterval: Day
        rollOnFileSizeLimit: true
        fileSizeLimitBytes: 52428800
        retainedFileCountLimit: 100
        path: Logs/log-.log
```

# Run
## As application
```
.\NgrokGeneratedAddressExporterAgent.exe
```
## As Windows Service
```PowerShell
New-Service `
    -Name NgrokGeneratedAddressExporterAgent `
    -BinaryPathName "D:\Program Files\NgrokGeneratedAddressExporterAgent\NgrokGeneratedAddressExporterAgent.exe" `
    -DisplayName "Ngrok Generated Address Exporter Agent" `
    -Description "Auto startup ngrok and expot generated address service" `
    -Credential $(Get-Credential)
```
