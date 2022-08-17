from Hawkbit.hawkbit_client import DefaultClientBuilder, EventHandler

if __name__ == '__main__':
    gateway_token = "GatewayToken 1ef6137927dba99f934d554cc671c4b0"
    hawkbit_endpoint = "http://cls-lxc-12:8080"
    controller_id = "MA_TEST"

    builder = DefaultClientBuilder()
    builder.set_hawkbit_endpoint(hawkbit_endpoint, controller_id) \
        .set_gateway_token(gateway_token) \
        .set_event_handler(EventHandler()) \
        .build() \
        .run()
