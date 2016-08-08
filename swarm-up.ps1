$ErrorActionPreference = "Stop"

# build tokengen image
if(docker images -q tokengen-img){
    "using existing tokengen image" 
}else{
    docker build -t tokengen-img .
}

# initialize swarm
docker swarm init

# create and start tokengen service
docker service create --publish 5000:5000 --name tokengen tokengen-img

# scale x3
docker service scale tokengen=3