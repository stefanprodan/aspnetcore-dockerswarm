workflow loadtest{
    Param($Iterations)

    $array = 1..$Iterations

    $startTime = get-date

    foreach -Parallel -ThrottleLimit 10 ($i in $array){
        Invoke-RestMethod http://localhost:5000/api/token
    }

    "elapsed time " + ((get-date) - $startTime).TotalSeconds + "sec"

    # display load 
    Invoke-RestMethod http://localhost:5000/api/issuer
}

# run load test
loadtest 500
