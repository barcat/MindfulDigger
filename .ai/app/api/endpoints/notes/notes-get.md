# GET /notes (Notes API)

Retrieves a paginated list of the authenticated user's notes, sorted by creation date descending.

## Request

### Headers
- `Authorization: Bearer <token>` (Required) - JWT token from Supabase Auth

### Query Parameters
- `page` (integer, optional, default: 1) - Page number for pagination
- `pageSize` (integer, optional, default: 15) - Number of notes per page

## Response

### Success (200 OK)
```json
{
  "notes": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef0123456789",
      "creationDate": "2025-05-05T10:00:00Z",
      "contentSnippet": "This is the content of the note..."
    },
    {
      "id": "b2c3d4e5-f6a7-8901-bcde-f01234567890",
      "creationDate": "2025-05-04T15:30:00Z",
      "contentSnippet": "Another note with slightly longer content..."
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 15,
    "totalPages": 3,
    "totalCount": 42
  }
}
```

### Error Codes
- `400 Bad Request` - Invalid query parameters (e.g., non-integer page/pageSize)
- `401 Unauthorized` - User is not authenticated
- `500 Internal Server Error` - Server error during retrieval

## Notes
- Notes are sorted by creation date in descending order (newest first)
- Each note's content is truncated to a snippet of maximum 120 characters, respecting word boundaries
- The pagination object includes metadata needed for implementing pagination controls in the UI