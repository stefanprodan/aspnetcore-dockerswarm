# Build tokengen image and run container

docker build -t tokengen-img .
docker run --name tokengen -d -p 5000:5000 -t tokengen-img