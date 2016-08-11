workflow loadtest{
    Param($Iterations)

    $array = 1..$Iterations

    foreach -Parallel -ThrottleLimit 10 ($i in $array){
        Invoke-RestMethod http://10.0.75.2:5000/api/token
    }
}

# run load test
loadtest 1000

# display issuers load 
# Invoke-RestMethod http://10.0.75.2:5000/api/issuer