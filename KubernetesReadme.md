# Create Namespace
> kubectl create ns rabbits

# Get Storageclass
> kubectl get storageclass

# Create Role Based Access Controller (RBAC)

> kubectl apply -n rabbits -f 1-rabbitmq-rbac.yaml


# Earlang Cookie.
RabbitMq communicates with its other nodes/peer via Earlang Cookie hence every nodes in the cluster needs to have same Earlang Cookie
Create a secret for Cookie so that it can be used in the RabbitMq Environment variable

> kubectl apply -n rabbits -f 2-rabbit-secret.yaml

# Configure RabbitMq with all the necessary plugins and configurations
**We need to use Discovery plugin of RabbitMq in order to discover the nodes in the kubernetes cluster**

```
kubectl apply -n rabbits -f 3-rabbit-configmap.yaml

kubectl apply -n rabbits -f 4-rabbit-statefulset.yaml

kubectl -n rabbits get pods

kubectl -n rabbits get pvc
```
# To Access the UI port forward to any Pods ex. Pod 1 with name rabbitmq-0
> kubectl -n rabbits port-forward rabbitmq-0 15672:15672

To send the message \
> kubectl port-forward -n rabbits rabbitmq-0 5672:5672

# Automatic Synchronization
First enter the bash of master node
```
kubectl -n rabbits exec -it rabbitmq-0 bash 

rabbitmqctl set_policy ha-fed \
    ".*" '{"federation-upstream-set":"all", "ha-sync-mode":"automatic", "ha-mode":"nodes", "ha-params":["rabbit@rabbitmq-0.rabbitmq.rabbits.svc.cluster.local","rabbit@rabbitmq-1.rabbitmq.rabbits.svc.cluster.local","rabbit@rabbitmq-2.rabbitmq.rabbits.svc.cluster.local"]}' \
    --priority 1 \
    --apply-to queues
```

**Delete a pod to check if everything works fine**

> kubectl -n rabbits delete pods rabbitmq-0