FROM alpine:3.6
RUN apk --no-cache add postgresql bind-tools libc6-compat
COPY ["artifacts/init", "/root"]
COPY ["artifacts/database", "/root"]
COPY ["artifacts/init/pgo", "/usr/bin/pgo"]
WORKDIR /root

RUN chmod +x /usr/bin/pgo
RUN chmod +x /root/init.sh

#Create cluster
ENV PGO_CA_CERT=server.crt
ENV PGO_CLIENT_CERT=server.crt
ENV PGO_CLIENT_KEY=server.key
ENV PGPORT=5432
ENV PGUSER=postgres

ENTRYPOINT ["sh","/root/init.sh"]