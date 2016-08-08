$ErrorActionPreference = "Stop"

for($i=1; $i -le 5; $i++){
    Invoke-RestMethod http://10.0.75.2:5000/api/token
}
