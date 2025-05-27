# GET /summaries/{summaryId} (Summaries API)

Retrieves the details of a specific summary belonging to the authenticated user.

## Request

### Headers
- `Authorization: Bearer <token>` (Required) - JWT token from Supabase Auth

### URL Parameters
- `summaryId` (UUID, required) - The unique identifier of the summary to retrieve

## Response

### Success (200 OK)
```json
{
  "id": "c3d4e5f6-a7b8-9012-cdef-0123456789ab",
  "userId": "f0e9d8c7-b6a5-4321-fedc-ba9876543210",
  "content": "## Trends\nPositive sentiment detected...\n\n## Key Topics\nProject X, Meeting with Y...\n\n## People Mentioned\nAlice, Bob...",
  "generationDate": "2025-05-05T08:00:00Z",
  "periodDescription": "Last 7 days",
  "periodStart": "2025-04-28T00:00:00Z",
  "periodEnd": "2025-05-04T23:59:59Z",
  "isAutomatic": true
}
```

### Error Codes
- `401 Unauthorized` - User is not authenticated
- `403 Forbidden` - User does not own this summary
- `404 Not Found` - Summary with the given ID does not exist
- `500 Internal Server Error` - Server error during retrieval

## Notes
- The content field contains formatted markdown text with sections as specified in WF-010
- Period dates (start/end) are included only when applicable (e.g., "Last 7 days") and may be null for other period types (e.g., "Last 10 notes")
- The summary is only accessible to its owner (enforced by Supabase RLS)