# karma-kebab
Event Management &amp; Planning Tool: A streamlined app for managing festival logistics—people, trucks—with check-in, scheduling, task management, and accessible briefings for remote employees on location.

## work-flow 

https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow

## run with docker

`docker-compose up --build`

docker inspect karma-kebab-keycloak:latest

docker tag karma-kebab-keycloak:latest $(oc registry info)/image-registry.openshift-image-registry.svc:5000/karma-kebab-keycloak:latest