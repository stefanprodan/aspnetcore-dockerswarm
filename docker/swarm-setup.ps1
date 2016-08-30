$ErrorActionPreference = "Stop"

# initialize swarm
if(!(docker info).contains("Swarm: active")){
	docker swarm init
}

# create network
$network = "backend-net"
if(!(docker network ls --filter name=$network -q)){
	docker network create --driver overlay $network
}
