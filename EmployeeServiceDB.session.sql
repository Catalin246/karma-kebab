Select * FROM employees


CREATE EXTENSION IF NOT EXISTS "pgcrypto";


INSERT INTO employees (
    employee_id,
    date_of_birth,
    first_name,
    last_name,
    address,
    payrate,
    role,
    email,
    skills
) VALUES (
    gen_random_uuid(), -- Automatically generates a unique UUID
    '1997-11-28',
    'Jane',
    'Blair',
    '123 Not Main St, Springfield, USA',
    25.50,
    2,
    'john.doe@example.com',
    ARRAY['Cooking', 'Driving']
);


DROP TABLE employees;
