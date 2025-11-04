# AIGS - Monitoring & Observability

## Overview

AIGS includes comprehensive monitoring and observability features using OpenTelemetry, Prometheus metrics, and structured logging with Serilog.

## Metrics

### Prometheus Metrics

The application exposes Prometheus metrics at `/metrics` endpoint:

#### HTTP Metrics
- `http_requests_total`: Total HTTP requests by method and endpoint
- `http_request_duration_seconds`: HTTP request duration histogram
- `http_requests_in_progress`: Current HTTP requests in progress

#### Custom Business Metrics
- `active_streams`: Current number of active chat streams
- `user_stream_count`: Number of active streams per user
- `model_concurrent_requests`: Concurrent requests per model
- `chat_messages_total`: Total chat messages processed
- `authentication_attempts_total`: Authentication attempts by result

#### System Metrics
- `process_cpu_seconds_total`: CPU usage
- `process_resident_memory_bytes`: Memory usage
- `dotnet_gc_collections_total`: Garbage collection metrics

### Grafana Dashboard

Create a Grafana dashboard with these panels:

```json
{
  "dashboard": {
    "title": "AIGS Dashboard",
    "panels": [
      {
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "{{method}} {{endpoint}}"
          }
        ]
      },
      {
        "title": "Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "95th percentile"
          }
        ]
      },
      {
        "title": "Active Streams",
        "type": "singlestat",
        "targets": [
          {
            "expr": "active_streams"
          }
        ]
      }
    ]
  }
}
```

## Logging

### Structured Logging

All logs are written in JSON format for easy parsing:

```json
{
  "Timestamp": "2024-01-01T12:00:00.000Z",
  "Level": "Information",
  "MessageTemplate": "User {Username} logged in successfully",
  "Properties": {
    "Username": "admin",
    "UserId": "123e4567-e89b-12d3-a456-426614174000",
    "SourceContext": "Gateway.Application.Services.AuthService"
  }
}
```

### Log Levels

- **Trace**: Detailed diagnostic information
- **Debug**: Diagnostic information for debugging
- **Information**: General information about application flow
- **Warning**: Warning messages for potential issues
- **Error**: Error messages for recoverable errors
- **Fatal**: Critical errors that may cause application failure

### Log Destinations

#### Development
- Console output with colored formatting
- File output to `./logs/gateway-{date}.log`

#### Production
- File output to `/opt/aigs/logs/gateway-{date}.log`
- Systemd journal integration
- Optional: Centralized logging (ELK stack, Fluentd, etc.)

### Log Rotation

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "./logs/gateway-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "fileSizeLimitBytes": 104857600
        }
      }
    ]
  }
}
```

## Tracing

### OpenTelemetry Traces

Distributed tracing is enabled for:

- HTTP requests
- Database operations
- External API calls (LM Studio)
- Custom business operations

### Trace Context

Traces include:
- Request ID
- User ID
- Conversation ID
- Model name
- Token usage

### Jaeger Integration

Configure Jaeger for trace visualization:

```yaml
# docker-compose.yml
version: '3.8'
services:
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
    environment:
      - COLLECTOR_OTLP_ENABLED=true
```

## Health Checks

### Liveness Probe

```http
GET /health/live
```

Returns `200 OK` if the service is running.

### Readiness Probe

```http
GET /health/ready
```

Returns detailed health status:

```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "Database",
      "status": "Healthy",
      "description": "Database connection is healthy"
    },
    {
      "name": "LM Studio",
      "status": "Healthy", 
      "description": "LM Studio connection is healthy"
    }
  ]
}
```

### Custom Health Checks

Add custom health checks:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<GatewayDbContext>()
    .AddCheck<LmStudioHealthCheck>("lm-studio")
    .AddCheck<ConcurrencyHealthCheck>("concurrency");
```

## Alerting

### Prometheus Alerting Rules

```yaml
groups:
- name: aigs
  rules:
  - alert: HighErrorRate
    expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.1
    for: 5m
    labels:
      severity: warning
    annotations:
      summary: "High error rate detected"
      
  - alert: HighResponseTime
    expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 5
    for: 5m
    labels:
      severity: warning
    annotations:
      summary: "High response time detected"
      
  - alert: ServiceDown
    expr: up{job="aigs"} == 0
    for: 1m
    labels:
      severity: critical
    annotations:
      summary: "AIGS service is down"
```

### Alertmanager Configuration

```yaml
route:
  group_by: ['alertname']
  group_wait: 10s
  group_interval: 10s
  repeat_interval: 1h
  receiver: 'web.hook'

receivers:
- name: 'web.hook'
  webhook_configs:
  - url: 'http://your-webhook-url'
```

## Performance Monitoring

### Key Performance Indicators (KPIs)

1. **Response Time**
   - Average response time < 2 seconds
   - 95th percentile < 5 seconds
   - 99th percentile < 10 seconds

2. **Throughput**
   - Requests per second
   - Concurrent users
   - Messages per minute

3. **Error Rate**
   - Error rate < 1%
   - 4xx errors < 5%
   - 5xx errors < 0.1%

4. **Resource Usage**
   - CPU usage < 80%
   - Memory usage < 80%
   - Disk I/O within limits

### Capacity Planning

Monitor these metrics for capacity planning:

- Peak concurrent users
- Peak requests per second
- Database connection pool usage
- Memory growth patterns
- Disk space usage

## Troubleshooting

### Common Issues

#### High Response Times
1. Check database performance
2. Monitor LM Studio response times
3. Check system resource usage
4. Review concurrent request limits

#### High Error Rates
1. Check application logs for errors
2. Monitor external service health
3. Review rate limiting configuration
4. Check authentication issues

#### Memory Issues
1. Monitor garbage collection metrics
2. Check for memory leaks
3. Review concurrent stream limits
4. Monitor database connection pools

### Debugging Commands

```bash
# Check service status
systemctl status aigs

# View recent logs
journalctl -u aigs -f

# Check metrics
curl http://localhost:5000/metrics

# Health check
curl http://localhost:5000/health/ready

# Check resource usage
htop
iostat -x 1
```

## Monitoring Stack

### Recommended Stack

1. **Metrics**: Prometheus + Grafana
2. **Logs**: ELK Stack (Elasticsearch, Logstash, Kibana)
3. **Traces**: Jaeger
4. **Alerting**: Alertmanager
5. **Uptime**: Uptime Kuma or Pingdom

### Docker Compose Example

```yaml
version: '3.8'
services:
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      
  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
      
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
```

## Best Practices

1. **Set Appropriate Retention**: Configure appropriate retention periods for metrics and logs
2. **Use Sampling**: Implement sampling for high-volume traces
3. **Monitor Costs**: Monitor costs of monitoring infrastructure
4. **Regular Reviews**: Regularly review and tune alerting thresholds
5. **Documentation**: Document monitoring procedures and runbooks
6. **Testing**: Regularly test alerting and incident response procedures
