Postman syntax for login details

curl --location 'http://localhost:86/WebApp/api/Login/UserLogin' \
--header 'Cookie: ARRAffinity=04f5de1902811e0ad35031b80aa8a599667305c63843621d6f4602ed3a2709a7; ARRAffinitySameSite=04f5de1902811e0ad35031b80aa8a599667305c63843621d6f4602ed3a2709a7; ARRAffinity=04f5de1902811e0ad35031b80aa8a599667305c63843621d6f4602ed3a2709a7; ARRAffinitySameSite=04f5de1902811e0ad35031b80aa8a599667305c63843621d6f4602ed3a2709a7' \
--header 'Content-Type: application/json' \
--data-raw '{
    "company_code": "boba_tea",
    "user_code": "superadmin",
    "user_password": "admin@123"
}'
