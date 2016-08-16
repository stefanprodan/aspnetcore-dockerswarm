# create and start rethinkdb primary 
docker service create --name rdb-primary --network backend-net rethinkdb:latest rethinkdb --bind all --no-http-admin

Start-Sleep -s 5

# create and start rethinkdb secondary
docker service create --name rdb-secondary --network backend-net --replicas 1 rethinkdb:latest rethinkdb --bind all --no-http-admin --join rdb-primary

Start-Sleep -s 5

# create and start rethinkdb proxy 
docker service create --name rdb-proxy --network backend-net --publish 8080:8080 --publish 28015:28015 rethinkdb:latest rethinkdb proxy --bind all --join rdb-primary

Start-Sleep -s 5

# up 3 nodes to enable automatic failover
docker service scale rdb-secondary=2

Start-Sleep -s 5

# remove primary
docker service rm rdb-primary

# recreate primary with --join flag
docker service create --name rdb-primary --network backend-net rethinkdb:latest rethinkdb --bind all --no-http-admin --join rdb-secondary