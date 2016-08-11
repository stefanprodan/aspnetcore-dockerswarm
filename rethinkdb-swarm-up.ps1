# create and start rethinkdb primary 
docker service create --name rdb-primary --network backend-net rethinkdb:latest rethinkdb --bind all --no-http-admin 

# create and start rethinkdb replica x3 
docker service create --name rdb-replica --replicas 3 --network backend-net rethinkdb:latest rethinkdb --bind all --no-http-admin --join rdb-primary

# create and start rethinkdb proxy 
docker service create --name rdb-proxy --network backend-net --publish 8080:8080 rethinkdb:latest rethinkdb proxy --bind all --join rdb-primary
