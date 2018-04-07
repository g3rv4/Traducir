# Running SQL Server inside of a docker container

That's how I do it... and I want to have it always available.

## Requisites

1. Docker!

## Setting it up

On a new folder, create a file named `docker-compose.yml`. Inside of it, put:

```
version: '2'
services:
  mssql:
    image: 'microsoft/mssql-server-linux:2017-latest'
    restart: always
    hostname: 'mssql'
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=SuperP4ssw0rd!
      - MSSQL_PID=Express
    ports:
      - '1433:1433'
    volumes:
      - 'mssql-data:/var/opt/mssql'
volumes:
  mssql-data:
```

aaand... just doing `docker-compose up -d` will create the container for you and ensure that it's restarted the next time you restart your machine. If you have an IDE, you can connect it to `127.0.0.1` with the username `sa` and the password `SuperP4ssw0rd!`.