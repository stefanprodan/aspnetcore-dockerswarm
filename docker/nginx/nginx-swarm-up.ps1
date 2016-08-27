$ErrorActionPreference = "Stop"

# build image
if(docker images -q nginx-img){
    "using existing nginx image"
}else{
    docker build -t nginx-img -f nginx.dockerfile .
}

# create and start nginx service
docker service create --publish 80:80 --name nginx --network backend-net nginx-img
