curl --location 'https://localhost:7023/api/v1/User/create' \
--header 'accept: */*' \
--header 'Content-Type: application/json' \
--data-raw '{
  "username": "anmartinez",
  "name": "andres martinez",
  "email": "andres14_12@hotmail.com",
  "phone": "3134768427",
  "password": "string",
  "confirmedPassword": "string"
}'