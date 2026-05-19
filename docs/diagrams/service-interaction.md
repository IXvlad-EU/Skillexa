```mermaid
flowchart LR

    Portal[Portal BFF]
    Core[Core API]
    Engine[Engine Worker]
    Broker[Message Broker]
    Search[TheirStack API]
    Storage[(Object Storage)]
    Db[(PostgreSQL)]
    Microsoft[Microsoft Entra ID]
    Google[Google OAuth]

    Portal --> Microsoft
    Portal --> Google
    Portal -->|Portal-signed JWT| Core
    Core --> Search
    Core --> Db
    Core --> Broker
    Broker --> Engine
    Engine --> Storage
    Engine --> Broker
    Broker --> Core
```
