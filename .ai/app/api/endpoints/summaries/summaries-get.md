# GET /summaries (Summaries API)

Retrieves a paginated list of the authenticated user's summaries, sorted by generation date descending.

## Request

### Headers
- `Authorization: Bearer <token>` (Required) - JWT token from Supabase Auth

### Query Parameters
- `page` (integer, optional, default: 1) - Page number for pagination
- `pageSize` (integer, optional, default: 10) - Number of summaries per page

## Response

### Success (200 OK)
```json
{
  "summaries": [
    {
      "id": "c3d4e5f6-a7b8-9012-cdef-0123456789ab",
      "generationDate": "2025-05-05T08:00:00Z",
      "periodDescription": "Last 7 days",
      "isAutomatic": true
    },
    {
      "id": "d4e5f6a7-b8c9-0123-def0-123456789abc",
      "generationDate": "2025-04-28T08:00:00Z",
      "periodDescription": "Last 10 notes",
      "isAutomatic": false
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 2,
    "totalCount": 18
  }
}
```

### Error Codes
- `400 Bad Request` - Invalid query parameters
- `401 Unauthorized` - User is not authenticated
- `500 Internal Server Error` - Server error during retrieval

## Notes
- Summaries are sorted by generation date in descending order (newest first)
- The list view only includes metadata - full content is available via GET /summaries/{id}
- `isAutomatic` indicates whether the summary was generated automatically (weekly) or manually requested