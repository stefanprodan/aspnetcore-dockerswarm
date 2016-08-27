$ErrorActionPreference = "Stop"

# build image
if(docker images -q logwatcher-img){
    "using existing logwatcher image" 
}else{
    docker build -t logwatcher-img -f LogWatcher.dockerfile .
}

# create and start logwatcher service
docker service create --publish 5005:5005 --mount type=bind,src=/c/users/docker/logwatcher,dst=/root/.aspnet/DataProtection-Keys --name logwatcher --network backend-net logwatcher-img
