```mermaid
sequenceDiagram

    actor User
    participant Portal as Skillexa Portal
    participant Core as Skillexa Core
    participant Search as TheirStack API
    participant Db as PostgreSQL
    participant Broker as Message Broker

    User->>Portal: Search jobs
    Portal->>Core: POST /job-listings/search
    Core->>Search: Proxy search request
    Search-->>Core: Job listings
    Core-->>Portal: Search response
    User->>Portal: Generate CV
    Portal->>Core: POST /documents
    Core->>Db: Insert document row
    Core->>Broker: Publish GeneratePdf
    Core-->>Portal: 202 Accepted with document id
```
