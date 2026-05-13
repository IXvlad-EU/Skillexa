```mermaid
flowchart LR

    Core[Core API]

    Queue[Generate Queue]

    Engine[Engine Worker]

    ResultQueue[Result Queue]

    Db[(PostgreSQL)]

    Blob[(Object Storage)]

    Core --> Queue

    Queue --> Engine

    Engine --> Blob

    Engine --> ResultQueue

    ResultQueue --> Core

    Core --> Db
```
