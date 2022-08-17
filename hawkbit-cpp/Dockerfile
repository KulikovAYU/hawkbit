ARG VARIANT="bullseye"
FROM debian:${VARIANT}

RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install libssl-dev ca-certificates jq \
    && apt-get autoremove -y && apt-get clean -y && rm -rf /var/lib/apt/lists/*

COPY build/example/hawkbit_example_cli /opt/up2date
COPY up2date_wrapper /opt/up2date_wrapper

VOLUME ["/opt/conf"]

CMD ["/opt/up2date_wrapper"]
