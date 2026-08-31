curl --location --request PUT 'https://localhost:7023/api/v1/Reservation/update' \
--header 'accept: */*' \
--header 'Content-Type: application/json' \
--data '{
  "code": "RV1",
  "quantity": 4,
  "status": {
    "code": 1,
    "name": "string"
  }
}'