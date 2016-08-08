$ErrorActionPreference = "Stop"

# stop all services by scaling to 0
docker service scale tokengen=0

# remove serive
docker service rm tokengen

# remove image
docker rmi -f tokengen-img