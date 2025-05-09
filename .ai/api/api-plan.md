# REST API Plan

This document outlines the REST API design for the MindfulDigger application, based on the provided database schema, product requirements document (PRD), and technical stack.

## 1. Resources

*   **Notes:** Represents user-created text notes.
    *   Database Table: `public.notes`
*   **Summaries:** Represents LLM-generated summaries of user notes.
    *   Database Table: `public.summaries`
*   **Feedback:** Represents user feedback on generated summaries.
    *   Database Table: `public.feedback`

*(Note: User authentication and management are handled by Supabase Auth and are not detailed as separate resources in this custom API plan).*

## 2. Endpoints

### Notes Resource (`/notes`)

*   **`POST /notes`**
    *   **Description:** Creates a new note for the authenticated user.
    *   **Request Body:**
        ```json
        {
          "content": "This is the content of the note."
        }
        ```
    *   **Response Body (Success - 201 Created):**
        ```json
        {
          "id": "a1b2c3d4-e5f6-7890-abcd-ef0123456789",
          "userId": "f0e9d8c7-b6a5-4321-fedc-ba9876543210",
          "content": "This is the content of the note.",
          "creationDate": "2025-05-05T10:00:00Z",
          "contentSnippet": "This is the content of the note." // Snippet for immediate UI update if needed
        }
        ```
    *   **Success Codes:** `201 Created`
    *   **Error Codes:**
        *   `400 Bad Request`: Validation failed (e.g., empty content, content > 1000 chars, user note limit reached).
        *   `401 Unauthorized`: User is not authenticated.
        *   `403 Forbidden`: User is authenticated but not authorized (should not happen with Supabase RLS if `user_id` is derived from token).
        *   `500 Internal Server Error`: Server error during creation.

*   **`GET /notes`**
    *   **Description:** Retrieves a paginated list of the authenticated user's notes, sorted by creation date descending.
    *   **Query Parameters:**
        *   `page` (integer, optional, default: 1): Page number for pagination.
        *   `pageSize` (integer, optional, default: 15): Number of notes per page.
    *   **Response Body (Success - 200 OK):**
        ```json
        {
          "notes": [
            {
              "id": "a1b2c3d4-e5f6-7890-abcd-ef0123456789",
              "creationDate": "2025-05-05T10:00:00Z",
              "contentSnippet": "This is the content of the note..." // Max 120 chars, word-aware truncation
            },
            {
              "id": "b2c3d4e5-f6a7-8901-bcde-f01234567890",
              "creationDate": "2025-05-04T15:30:00Z",
              "contentSnippet": "Another note with slightly longer content..."
            }
            // ... more notes
          ],
          "pagination": {
            "currentPage": 1,
            "pageSize": 15,
            "totalPages": 3,
            "totalCount": 42
          }
        }
        ```
    *   **Success Codes:** `200 OK`
    *   **Error Codes:**
        *   `400 Bad Request`: Invalid query parameters (e.g., non-integer page/pageSize).
        *   `401 Unauthorized`: User is not authenticated.
        *   `500 Internal Server Error`: Server error during retrieval.

### Summaries Resource (`/summaries`)

*   **`GET /summaries`**
    *   **Description:** Retrieves a paginated list of the authenticated user's summaries, sorted by generation date descending.
    *   **Query Parameters:**
        *   `page` (integer, optional, default: 1): Page number for pagination.
        *   `pageSize` (integer, optional, default: 10): Number of summaries per page.
    *   **Response Body (Success - 200 OK):**
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
            // ... more summaries
          ],
          "pagination": {
            "currentPage": 1,
            "pageSize": 10,
            "totalPages": 2,
            "totalCount": 18
          }
        }
        ```
    *   **Success Codes:** `200 OK`
    *   **Error Codes:**
        *   `400 Bad Request`: Invalid query parameters.
        *   `401 Unauthorized`: User is not authenticated.
        *   `500 Internal Server Error`: Server error during retrieval.

*   **`GET /summaries/{summaryId}`**
    *   **Description:** Retrieves the details of a specific summary belonging to the authenticated user.
    *   **Response Body (Success - 200 OK):**
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
    *   **Success Codes:** `200 OK`
    *   **Error Codes:**
        *   `401 Unauthorized`: User is not authenticated.
        *   `403 Forbidden`: User does not own this summary.
        *   `404 Not Found`: Summary with the given ID does not exist.
        *   `500 Internal Server Error`: Server error during retrieval.

*   **`POST /summaries/generate`**
    *   **Description:** Triggers the generation of a new summary for the authenticated user based on the specified period. This is likely an asynchronous operation.
    *   **Request Body:**
        ```json
        {
          "period": "last_7_days" // Options: "last_7_days", "last_14_days", "last_30_days", "last_10_notes"
        }
        ```
    *   **Response Body (Success - 202 Accepted):**
        ```json
        {
          "message": "Summary generation request accepted. It will be processed in the background.",
          "statusCheckUrl": "/summaries/generation/status/{jobId}" // Optional: URL to check status
        }
        ```
        *(Alternative Success - 201 Created if generation is synchronous and fast)*
        ```json
        // Response body similar to GET /summaries/{summaryId}
        ```
    *   **Success Codes:** `202 Accepted` (preferred for potentially long operations), `201 Created` (if synchronous)
    *   **Error Codes:**
        *   `400 Bad Request`: Invalid `period` value, or insufficient notes for the selected period.
        *   `401 Unauthorized`: User is not authenticated.
        *   `429 Too Many Requests`: Rate limit exceeded for summary generation.
        *   `500 Internal Server Error`: Server error during request processing or LLM interaction.
        *   `503 Service Unavailable`: LLM service (Openrouter.ai) is unavailable.

### Feedback Resource (`/feedback`)

*   **`POST /feedback`**
    *   **Description:** Creates or updates feedback (rating) for a specific summary by the authenticated user. Uses UPSERT logic or checks existence first due to the unique constraint (`summary_id`, `user_id`).
    *   **Request Body:**
        ```json
        {
          "summaryId": "c3d4e5f6-a7b8-9012-cdef-0123456789ab",
          "rating": "positive" // Options: "positive", "negative"
        }
        ```
    *   **Response Body (Success - 200 OK or 201 Created):**
        ```json
        {
          "summaryId": "c3d4e5f6-a7b8-9012-cdef-0123456789ab",
          "userId": "f0e9d8c7-b6a5-4321-fedc-ba9876543210",
          "rating": "positive",
          "creationDate": "2025-05-05T11:00:00Z" // Or update date if modified
        }
        ```
    *   **Success Codes:** `201 Created` (if new feedback), `200 OK` (if feedback updated)
    *   **Error Codes:**
        *   `400 Bad Request`: Invalid `rating` value, missing `summaryId`.
        *   `401 Unauthorized`: User is not authenticated.
        *   `403 Forbidden`: User does not own the specified summary.
        *   `404 Not Found`: Summary with the given `summaryId` does not exist.
        *   `500 Internal Server Error`: Server error during creation/update.

## 3. Authentication and Authorization

*   **Authentication:** Handled via Supabase Authentication. The API expects a valid JWT (issued by Supabase Auth) in the `Authorization: Bearer <token>` header for all protected endpoints. The backend (.NET 8 API) will validate this token.
*   **Authorization:** Primarily enforced by PostgreSQL Row Level Security (RLS) policies configured in Supabase, based on `auth.uid() = user_id`. The backend API will also implicitly enforce this by extracting the `user_id` from the validated JWT and using it in database queries. Endpoints like `GET /summaries/{summaryId}` will include checks to ensure the requested resource belongs to the authenticated user before returning data.

## 4. Validation and Business Logic

*   **Notes (`POST /notes`):**
    *   `content` cannot be null or empty.
    *   `content` length must be <= 1000 characters (WF-005).
    *   The authenticated user must have fewer than 100 existing notes before creation (WF-015). This check occurs in the API logic before inserting into the database.
*   **Notes (`GET /notes`):**
    *   `contentSnippet` is generated by the backend, truncating `content` to a maximum of 120 characters, respecting word boundaries (WF-007).
    *   Sorting is fixed to `creation_date` descending (WF-006).
    *   Pagination parameters (`page`, `pageSize`) are validated (must be positive integers).
*   **Summaries (`POST /summaries/generate`):**
    *   `period` must be one of the allowed enum values.
    *   The API checks if there are sufficient notes within the specified period/count to generate a meaningful summary before calling the LLM.
*   **Feedback (`POST /feedback`):**
    *   `summaryId` must correspond to an existing summary owned by the user.
    *   `rating` must be either `positive` or `negative`.
    *   The combination of `summaryId` and the authenticated user's `userId` must be unique (handled by database constraint and API logic - UPSERT or check-then-insert/update).
*   **General:**
    *   All endpoints interacting with user-specific data (`notes`, `summaries`, `feedback`) implicitly use the authenticated user's ID (from JWT) for filtering and authorization checks, aligning with RLS policies.
    *   Rate limiting should be applied (implementation detail, e.g., ASP.NET Core middleware) to prevent abuse, especially for resource-intensive endpoints like `POST /summaries/generate`.