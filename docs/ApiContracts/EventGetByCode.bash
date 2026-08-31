curl --location --request GET 'https://localhost:7023/api/v1/Event/getByCode' \
--header 'accept: */*' \
--header 'X-API-Key: FUN-EVENTS-CLIENT-001' \
--header 'Content-Type: application/json' \
--data '{
  "code": "string"
}'