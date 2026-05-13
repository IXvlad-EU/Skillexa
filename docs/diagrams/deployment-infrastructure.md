```mermaid
flowchart LR

    Browser[Web Browser]
    Portal[Portal Container]
    Core[Core API Container]
    Engine[Engine Worker Container]
    Broker[RabbitMQ or Azure Service Bus]
    Db[(PostgreSQL)]
    Blob[(Azurite or Azure Blob)]
    Entra[Microsoft Entra ID]
    Search[TheirStack API]

    Browser --> Portal
    Portal --> Entra
    Portal --> Core
    Core --> Search
    Core --> Db
    Core --> Broker
    Broker --> Engine
    Engine --> Blob
    Broker --> Core
```
