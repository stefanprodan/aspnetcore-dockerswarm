$ErrorActionPreference = "Stop"

    Invoke-RestMethod -Method Get -Headers @{'accept'='application/json'} -Uri http://10.0.75.2:5000/api/token
}
