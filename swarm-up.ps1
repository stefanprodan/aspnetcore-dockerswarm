#$ErrorActionPreference = "Stop"

# build tokengen image
if(docker images -q tokengen-img){
    "using existing tokengen image" 
}else{
    docker build -t tokengen-img -f TokenGen.dockerfile .
}

# initialize swarm
docker swarm init

# create network
docker network create --driver overlay backend-net

# create and start rethinkdb service 
docker service create --publish 8080:8080 --name rethinkdev --network backend-net rethinkdb:latest rethinkdb --bind all --server-name DEV

# create and start tokengen service
docker service create --publish 5000:5000 --name tokengen --network backend-net tokengen-img

# scale x3
docker service scale tokengen=3