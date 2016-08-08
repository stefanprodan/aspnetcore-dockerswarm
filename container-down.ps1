$ErrorActionPreference = "Stop"

# stop container if running 
docker stop tokengen

# remove container
docker rm tokengen

# remove image
docker rmi -f tokengen-img