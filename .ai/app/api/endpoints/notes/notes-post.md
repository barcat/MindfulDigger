# POST /notes (Notes API)

Creates a new note for the authenticated user.

## Request

### Headers
- `Authorization: Bearer <token>` (Required) - JWT token from Supabase Auth
- `Content-Type: application/json`

### Body
```json
{
  "content": "This is the content of the note."
}
```

### Validation
- `content` is required and must not be empty
- `content` must not exceed 1000 characters
- User must not have reached the limit of 100 notes

## Response

### Success (201 Created)
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef0123456789",
  "userId": "f0e9d8c7-b6a5-4321-fedc-ba9876543210",
  "content": "This is the content of the note.",
  "creationDate": "2025-05-05T10:00:00Z",
  "contentSnippet": "This is the content of the note."
}
```

### Error Codes
- `400 Bad Request` - Validation failed (empty content, content > 1000 chars, user note limit reached)
- `401 Unauthorized` - User is not authenticated
- `403 Forbidden` - User is authenticated but not authorized
- `500 Internal Server Error` - Server error during creation

## Notes
- The `contentSnippet` field is provided for immediate UI updates without truncation logic on the client side
- The `creationDate` is set by the server in UTC
- The `userId` is extracted from the JWT token and validated against Supabase RLS policies