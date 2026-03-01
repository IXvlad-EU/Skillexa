FROM rabbitmq:4.1-management-alpine

# Copy custom configuration
COPY rabbitmq.conf /etc/rabbitmq/rabbitmq.conf
COPY definitions.json /etc/rabbitmq/definitions.json

# Management UI on 15672, AMQP on 5672
EXPOSE 5672 15672

# Health check using rabbitmq diagnostics
HEALTHCHECK --interval=15s --timeout=5s --retries=5 --start-period=30s \
  CMD rabbitmq-diagnostics -q ping
