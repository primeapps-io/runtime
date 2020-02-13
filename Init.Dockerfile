FROM alpine:3.6
RUN apk --no-cache add postgresql bind-tools libc6-compat
COPY ["artifacts/init", "/root"]
COPY ["artifacts/init/pgo", "/usr/bin/pgo"]
COPY ["database", "/root"]
RUN rm database/studio.bak
RUN rm database/templet0.bak 
WORKDIR /root

RUN chmod +x /usr/bin/pgo
RUN chmod +x /root/init.sh

ENV PGO_CA_CERT=server.crt
ENV PGO_CLIENT_CERT=server.crt
ENV PGO_CLIENT_KEY=server.key
ENV PGPORT=5432
ENV PGUSER=postgres

ENTRYPOINT ["sh","/root/init.sh"]