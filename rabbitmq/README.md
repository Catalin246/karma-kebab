# RabbitMQ Service

This directory contains the necessary configuration for running a RabbitMQ service in a Docker container. The provided `Dockerfile` sets up RabbitMQ with the management console enabled.

## Features
- **RabbitMQ Version**: 3.x with the management plugin.
- **Exposed Ports**:
  - `5672`: RabbitMQ messaging port.
  - `15672`: RabbitMQ management console.

## Depoyment

`oc apply -f rabbitmq-pvc.yaml; oc apply -f rabbitmq-configmap.yaml; oc apply -f rabbitmq-deploymentconfig.yaml; oc apply -f rabbitmq-service.yaml; oc apply -f rabbitmq-route.yaml`

