curl --location 'https://localhost:7023/api/v1/Reservation/create' \
--header 'accept: */*' \
--header 'Content-Type: application/json' \
--data '{
  "code": "RV1",
  "eventCode": "string",
  "userName": "anmartinez",
  "quantity": 2,
  "channel": 1
}'