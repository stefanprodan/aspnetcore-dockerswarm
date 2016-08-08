$ErrorActionPreference = "Stop"

# build image
if(docker images -q tokengen-img){
    "using existing tokengen image" 
}else{
    docker build -t tokengen-img -f TokenGen.dockerfile .
}

# run container
docker run --name tokengen -d -p 5000:5000 -t tokengen-img