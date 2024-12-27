# ReadOnly Filesystem demo

This is a simple demo to show how to use the read-only filesystem feature in the ASPNET Core 8 with docker/kubernetes.

## How to run
- Clone the repository
- Run the following command to build the docker image
```bash
docker build -t rofs -f RoFsApp/Dockerfile .
```
- Run the following command to run the docker container
```bash
docker run -d -p 15066:8080 \
    --name rofs \
    --read-only \
    --tmpfs /tmp \
    rofs
```
- Open the browser and navigate to `http://localhost:8080`