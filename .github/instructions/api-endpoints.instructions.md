---
applyTo: "skillexa-core/**"
---

# API Endpoints — Instructions

## Base URL

- Local: `http://localhost:8080`
- Container-to-container: `http://skillexa-core:8080`

## Authentication Endpoints

### `POST /auth/login`

- **Auth**: Anonymous
- **Body**: `{ "email": "string", "password": "string" }`
- **200**: `{ "accessToken": "string", "expiresIn": 3600, "refreshToken": "string" }`
- **401**: Invalid credentials

### `POST /auth/refresh`

- **Auth**: Anonymous
- **Body**: `{ "refreshToken": "string" }`
- **200**: `{ "accessToken": "string", "expiresIn": 3600, "refreshToken": "string" }`
- **401**: Invalid or expired refresh token

## Document / Job Endpoints

### `POST /documents`

- **Auth**: Bearer JWT
- **Body**:
  ```jsonc
  {
    "templateKey": "string",
    "templateVersion": 1,        // optional — defaults to active version
    "payload": {
      "jobTitle": "string",
      "keywords": ["string"],
      "salaryMin": 0,
      "salaryMax": 0
      // ...other domain fields
    }
  }
  ```
- **202 Accepted**: `{ "jobId": 123, "status": "Queued" }`
- Creates a `Job` in DB (status=`Queued`) and enqueues `GeneratePdf`.

### `GET /jobs`

- **Auth**: Bearer JWT
- **200**: Array of jobs for the current user, newest first.
  ```jsonc
  [
    {
      "id": 123,
      "status": "Queued | Processing | Succeeded | Failed",
      "templateKey": "string",
      "errorCode": "string | null",
      "createdAt": "ISO-8601",
      "updatedAt": "ISO-8601"
    }
  ]
  ```
- Supports pagination query params (`?page=1&pageSize=20`).

### `GET /jobs/{jobId}`

- **Auth**: Bearer JWT (owner only)
- **200**: Full job detail including `errorMessage`, storage keys (internal), timestamps.
- **403**: Job belongs to another user.
- **404**: Job not found.

### `POST /jobs/{jobId}/download-url`

- **Auth**: Bearer JWT (owner only)
- **Pre-conditions**: `job.status == Succeeded`, `job.userId == currentUser`.
- **200**: `{ "url": "https://...signed-url...", "expiresIn": 600 }`
- **403**: Not the owner.
- **404**: Job not found.
- **409**: Job not in `Succeeded` status.

## Usage Endpoints

### `GET /app/usage`

- **Auth**: Bearer JWT
- **200**: Current provider usage / quota for the authenticated user.
  ```jsonc
  {
    "provider": "theirstack",
    "periodKey": "2026-02",
    "used": 12,
    "remaining": 88
  }
  ```

## Admin Endpoints (optional)

### `POST /admin/templates`

- **Auth**: Bearer JWT + Admin role
- **Body**: `{ "templateKey": "string", "content": "..." }`
- **201**: Created template with auto-incremented version.

### `PUT /admin/templates/{key}/versions/{version}`

- **Auth**: Bearer JWT + Admin role
- Updates template content for a specific version.

### `POST /admin/templates/{key}/activate/{version}`

- **Auth**: Bearer JWT + Admin role
- Sets the given version as the active version for the template key.

## General Conventions

- Return **problem details** (`application/problem+json`) for all error responses (RFC 9457).
- Use `.WithOpenApi()`, `[EndpointSummary]`, and `[EndpointDescription]` on every endpoint — this feeds the OpenAPI spec consumed by Kiota.
- All `2xx` responses include a JSON body (no bare `204` without reason).
- Timestamps are always **ISO-8601 UTC**.
- IDs (`jobId`, `id`) are `long` (BIGINT) values matching database primary keys.
