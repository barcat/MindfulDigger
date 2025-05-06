# POST /feedback (Feedback API)

Creates or updates feedback (rating) for a specific summary by the authenticated user.

## Request

### Headers
- `Authorization: Bearer <token>` (Required) - JWT token from Supabase Auth
- `Content-Type: application/json`

### Body
```json
{
  "summaryId": "c3d4e5f6-a7b8-9012-cdef-0123456789ab",
  "rating": "positive"
}
```

### Validation
- `summaryId` must be a valid UUID of an existing summary owned by the user
- `rating` must be one of: "positive", "negative"

## Response

### Success (201 Created or 200 OK)
```json
{
  "summaryId": "c3d4e5f6-a7b8-9012-cdef-0123456789ab",
  "userId": "f0e9d8c7-b6a5-4321-fedc-ba9876543210",
  "rating": "positive",
  "creationDate": "2025-05-05T11:00:00Z"
}
```

### Error Codes
- `400 Bad Request` - Invalid rating value or missing summaryId
- `401 Unauthorized` - User is not authenticated
- `403 Forbidden` - User does not own the specified summary
- `404 Not Found` - Summary with the given summaryId does not exist
- `500 Internal Server Error` - Server error during creation/update

## Notes
- The endpoint uses UPSERT logic due to the unique constraint on (summary_id, user_id)
- If feedback already exists for the given summary and user, it will be updated
- The creationDate field reflects either the original creation time or the update time
- RLS policies ensure users can only create/update feedback for summaries they own