```mermaid
sequenceDiagram

    actor User
    participant Portal as Skillexa Portal
    participant Core as Skillexa Core
    participant Broker as Message Broker
    participant Engine as Skillexa Engine
    participant Storage as Object Storage

    User->>Portal: Generate CV from listing
    Portal->>Core: POST /documents
    Core->>Broker: Publish GeneratePdf
    Broker-->>Engine: Deliver GeneratePdf
    Engine->>Storage: Upload pdf/{documentId}.pdf
    Engine->>Storage: Upload snapshots/{documentId}.json
    Engine->>Broker: Publish PdfStatusChanged Succeeded
    Broker-->>Core: Deliver PdfStatusChanged
    Core-->>Portal: Document status updated
```
