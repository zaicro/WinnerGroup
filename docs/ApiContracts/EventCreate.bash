curl --location 'https://localhost:7023/api/v1/Event/create' \
--header 'accept: */*' \
--header 'X-API-Key: FUN-EVENTS-CLIENT-001' \
--header 'Content-Type: application/json' \
--data '{
  "code": "string",
  "name": "string",
  "eventDate": "2026-08-17T17:07:13.361Z",
  "capacity": 10
}'