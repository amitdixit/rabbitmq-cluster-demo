
docker run -d --hostname rabbit-host --name rabbitmq-demo -p 15672:15672 -p 5672:5672 rabbitmq:3-management


# run a standalone instance
docker network create rabbits

docker run -d --rm --net rabbits --hostname rabbit-1 --name rabbit-1 rabbitmq:3-management

docker run -d --hostname rabbit-host --name rabbitmq-demo -p 15672:15672 -p 5672:5672 rabbitmq:3-management


# how to grab existing erlang cookie
docker exec -it rabbit-1 cat /var/lib/rabbitmq/.erlang.cookie

# clean up
docker rm -f rabbit-1


docker exec -it rabbitmq-demo cat /var/lib/rabbitmq/.erlang.cookie


# Creating a Cluster (eg. 3 node cluster)
docker network create rabbits

>docker run -d --rm --net rabbits --hostname rabbit-1 --name rabbit-1 -p 15672:15672 -p 5672:5672 rabbitmq:3-management
>
>docker run -d --rm --net rabbits --hostname rabbit-2 --name rabbit-2 -p 15673:15672 -p 5673:5672 rabbitmq:3-management
>
>docker run -d --rm --net rabbits --hostname rabbit-3 --name rabbit-3 -p 15674:15672 -p 5674:5672 rabbitmq:3-management

**Cluster status**

docker exec -it rabbit-1 rabbitmqctl cluster_status


# Manual Clustering#

**For Nodes to join the cluster each and every node should have same Earlang Cookie for communication**\
docker run -d --rm --net rabbits -p 8080:15672 -e RABBITMQ_ERLANG_COOKIE=DSHEVCXBBETJJVJWTOWT --hostname rabbit-manager --name rabbit-manager rabbitmq:3.8-management


docker run -d --rm --net rabbits -e RABBITMQ_ERLANG_COOKIE=DSHEVCXBBETJJVJWTOWT --hostname rabbit-1 --name rabbit-1 -p 15672:15672 -p 5672:5672 rabbitmq:3-management

docker run -d --rm --net rabbits -e RABBITMQ_ERLANG_COOKIE=DSHEVCXBBETJJVJWTOWT --hostname rabbit-2 --name rabbit-2 -p 15673:15672 -p 5673:5672 rabbitmq:3-management

docker run -d --rm --net rabbits -e RABBITMQ_ERLANG_COOKIE=DSHEVCXBBETJJVJWTOWT --hostname rabbit-3 --name rabbit-3 -p 15674:15672 -p 5674:5672 rabbitmq:3-management


# Join Node 2
docker exec -it rabbit-2 rabbitmqctl stop_app\
docker exec -it rabbit-2 rabbitmqctl reset\
docker exec -it rabbit-2 rabbitmqctl join_cluster rabbit@rabbit-1\
docker exec -it rabbit-2 rabbitmqctl start_app\
docker exec -it rabbit-2 rabbitmqctl cluster_status


# Join Node 3
docker exec -it rabbit-3 rabbitmqctl stop_app \
docker exec -it rabbit-3 rabbitmqctl reset  \
docker exec -it rabbit-3 rabbitmqctl join_cluster rabbit@rabbit-1 \
docker exec -it rabbit-3 rabbitmqctl start_app \
docker exec -it rabbit-3 rabbitmqctl cluster_status


# Automated Clustering
If any container gets destroyed it wont we able to join the cluster since no persistence has been set

**Create configuration for each node/instances**

loopback_users.guest = false \
listeners.tcp.default = 5672

cluster_formation.peer_discovery_backend = rabbit_peer_discovery_classic_config \
cluster_formation.classic_config.nodes.1 = rabbit@rabbit-1 \
cluster_formation.classic_config.nodes.2 = rabbit@rabbit-2 \
cluster_formation.classic_config.nodes.3 = rabbit@rabbit-3



docker run -d --net rabbits `
-v ${PWD}/config/rabbit-1/:/config/ `
-e RABBITMQ_CONFIG_FILE=/config/rabbitmq `
-e RABBITMQ_ERLANG_COOKIE=DSHEVCXBBETJJVJWTOWT `
--hostname rabbit-1 `
--name rabbit-1 `
-p 15672:15672 -p 5672:5672 `
rabbitmq:3-management

docker run -d --rm --net rabbits `
-v ${PWD}/config/rabbit-2/:/config/ `
-e RABBITMQ_CONFIG_FILE=/config/rabbitmq `
-e RABBITMQ_ERLANG_COOKIE=DSHEVCXBBETJJVJWTOWT `
--hostname rabbit-2 `
--name rabbit-2 `
-p 15673:15672 -p 5673:5672 `
rabbitmq:3-management

docker run -d --rm --net rabbits `
-v ${PWD}/config/rabbit-3/:/config/ `
-e RABBITMQ_CONFIG_FILE=/config/rabbitmq `
-e RABBITMQ_ERLANG_COOKIE=DSHEVCXBBETJJVJWTOWT `
--hostname rabbit-3 `
--name rabbit-3 `
-p 15674:15672 -p 5674:5672 `
rabbitmq:3-management


# enable federation plugin
docker exec -it rabbit-1 rabbitmq-plugins enable rabbitmq_federation \
docker exec -it rabbit-2 rabbitmq-plugins enable rabbitmq_federation \
docker exec -it rabbit-3 rabbitmq-plugins enable rabbitmq_federation

# Basic Queue Mirroring Set the HA Policy (On the Master node)
docker exec -it rabbit-1 bash


rabbitmqctl set_policy ha-fed \
    ".*" '{"federation-upstream-set":"all", "ha-mode":"nodes", "ha-params":["rabbit@rabbit-1","rabbit@rabbit-2","rabbit@rabbit-3"]}' \
    --priority 1 \
    --apply-to queues

# Automatic Synchronization

docker exec -it rabbit-1 bash

**We need to set "ha-sync-mode":"automatic"**


rabbitmqctl set_policy ha-fed \
    ".*" '{"federation-upstream-set":"all", "ha-sync-mode":"automatic", "ha-mode":"nodes", "ha-params":["rabbit@rabbit-1","rabbit@rabbit-2","rabbit@rabbit-3"]}' \
    --priority 1 \
    --apply-to queues