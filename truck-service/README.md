
`python3 -m venv .venv`

Desktop: `.venv\Scripts\activate` Linux: `source .venv/bin/activate`

`pip install -r requirements.txt`

run: python run.py

now with docker:
docker-compose up --build -d



for postman the link is: (get/post)
http://localhost:3006/trucks

example json to post:

{
  "plate_number": "abc-123",
  "name": "kebab",
  "description": "can carry 120kg kebab",
  "note": "perfect"
}



rest: (delete, get, put);
example:
http://localhost:3006/trucks/abc-123