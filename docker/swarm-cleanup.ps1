# Remove all stopped containers
docker rm $(docker ps -a -q -f "status=exited")

Start-Sleep 5

# Remove untagged images
docker rmi $(docker images -q -f "dangling=true")

Start-Sleep 5

# Remove orphaned volumes
docker volume rm $(docker volume ls -q -f "dangling=true")