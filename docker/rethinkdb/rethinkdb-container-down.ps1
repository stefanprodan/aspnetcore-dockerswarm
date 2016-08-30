$ErrorActionPreference = "Stop"

# stop container if running 
docker stop rethinkdev

# remove container
docker rm rethinkdev
