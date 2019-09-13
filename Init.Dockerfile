FROM alpine
RUN apk update && apk add postgresql-client bind-tools libc6-compat
COPY ["artifacts/init", "/root"]
COPY ["artifacts/database", "/root"]
COPY ["artifacts/init/pgo", "/usr/bin/pgo"]
COPY ["kubernetes/helm/postgresoperator/files/apiserver/pgouser", "/root/.pgouser"]
WORKDIR /root

RUN chmod +x /usr/bin/pgo
RUN chmod +x /root/init.sh

#Create cluster
ENV CO_APISERVER_URL=https://primeapps-pgo.primeapps.svc:8443
ENV PGO_CA_CERT=server.crt
ENV PGO_CLIENT_CERT=server.crt
ENV PGO_CLIENT_KEY=server.key
ENV PGHOST=primeapps-database.primeapps.svc
ENV PGPORT=5432
ENV PGUSER=postgres
ENV PGPASSWORD=pr!m€Appsi0d

ENTRYPOINT ["sh","/root/init.sh"]