curl --location --request PUT 'https://localhost:7023/api/v1/Event/update' \
--header 'accept: */*' \
--header 'X-API-Key: FUN-EVENTS-CLIENT-001' \
--header 'Content-Type: application/json' \
--data '{
  "code": "string",
  "name": "Primer Evento",
  "eventDate": "2026-08-17T17:08:17.290Z",
  "capacity": 10
}'