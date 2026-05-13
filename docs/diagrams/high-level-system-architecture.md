```mermaid
flowchart LR

    User((User))
    Portal[Skillexa Portal]
    Core[Skillexa Core API]
    Engine[Skillexa Engine Worker]
    Broker[Message Broker]
    Db[(PostgreSQL)]
    Blob[(Object Storage)]
    Search[TheirStack API]
    Entra[Microsoft Entra ID]

    User --> Portal
    Portal --> Entra
    Portal --> Core
    Core --> Search
    Core --> Db
    Core --> Broker
    Broker --> Engine
    Engine --> Blob
    Engine --> Broker
    Broker --> Core
```
