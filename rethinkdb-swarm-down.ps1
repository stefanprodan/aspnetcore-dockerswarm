# remove RethinkDB cluster
docker service rm rdb-proxy
docker service rm rdb-primary
docker service rm rdb-secondary

Start-Sleep -s 5

# remove all containers
docker rm -f $(docker ps -a -q)

# remove all stopped containers
# docker rm -v $(docker ps -a -q -f status=exited)

# remove all unused images
# docker rmi $(docker images -q -f dangling=true)