# POST /summaries/generate (Summaries API)

Triggers the generation of a new summary for the authenticated user based on the specified period.

## Request

### Headers
- `Authorization: Bearer <token>` (Required) - JWT token from Supabase Auth
- `Content-Type: application/json`

### Body
```json
{
  "period": "last_7_days"
}
```

### Validation
- `period` must be one of: "last_7_days", "last_14_days", "last_30_days", "last_10_notes"
- User must have at least one note in the specified period

## Response

### Success (202 Accepted)
```json
{
  "message": "Summary generation request accepted. It will be processed in the background.",
  "statusCheckUrl": "/summaries/generation/status/{jobId}"
}
```

### Alternative Success (201 Created)
If the generation is fast enough to be synchronous, the response will be the same as GET /summaries/{id}
```json
{
  "id": "c3d4e5f6-a7b8-9012-cdef-0123456789ab",
  "userId": "f0e9d8c7-b6a5-4321-fedc-ba9876543210",
  "content": "## Trends\nPositive sentiment detected...",
  "generationDate": "2025-05-05T08:00:00Z",
  "periodDescription": "Last 7 days",
  "periodStart": "2025-04-28T00:00:00Z",
  "periodEnd": "2025-05-04T23:59:59Z",
  "isAutomatic": false
}
```

### Error Codes
- `400 Bad Request` - Invalid period value or insufficient notes for the selected period
- `401 Unauthorized` - User is not authenticated
- `429 Too Many Requests` - Rate limit exceeded for summary generation
- `500 Internal Server Error` - Server error during request processing or LLM interaction
- `503 Service Unavailable` - LLM service is unavailable

## Notes
- Generation is typically asynchronous due to LLM processing time
- The status check URL is provided for polling the generation status
- Manual summaries are marked with `isAutomatic: false`
- Period dates are calculated server-side based on the selected period type