# Usage Guide

## Typical command

```bash
ProObjLogLite --level INFO --source api --message "Request started"
```

## Common patterns

JSON logs:

```bash
ProObjLogLite --json --source auth --message "User signed in"
```

Observability metadata:

```bash
ProObjLogLite --level ERROR --event-id AUTH-7 --cid req-19 --context userId=42 --message "Token expired"
```

File rotation:

```bash
ProObjLogLite --message "job complete" --max-size 10MB --max-files 5
```

No file write (stdout only):

```bash
ProObjLogLite --stdout-only --json --message "heartbeat"
```

No write side effects (dry run):

```bash
ProObjLogLite --dry-run --message "preview only"
```

Tamper-evident chained logs:

```bash
ProObjLogLite --chain-hash --source billing --message "Invoice generated"
```

Timer workflow:

```bash
ProObjLogLite --timer deploy --timer-start
# do work
ProObjLogLite --timer deploy --timer-stop --level INFO
```

Production preset + redaction:

```bash
ProObjLogLite --profile prod --redact-keys token,password --context token=abc123 --message "token=abc123"
```

Sampling noisy logs:

```bash
ProObjLogLite --sample-rate 0.1 --level DEBUG --message "poll tick"
```

Smart/logic logging:

```bash
ProObjLogLite --smart --message "payment failed due to timeout"
ProObjLogLite --when "env == prod" --context env=prod --message "prod-only event"
ProObjLogLite --assert "status == ok" --context status=fail --message "health gate"
```

Decision + metric logging:

```bash
ProObjLogLite --decision cache_strategy --outcome redis --context region=us-east --message "strategy selected"
ProObjLogLite --metric latency_ms=143 --metric cpu_pct=78 --message "runtime sample"
```
