Select * FROM "Employees"


CREATE EXTENSION IF NOT EXISTS "pgcrypto";

ALTER TABLE "Employees"
ALTER COLUMN "Skills" TYPE text[];


INSERT INTO "Employees" (
    "EmployeeId",
    "DateOfBirth",
    "FirstName",
    "LastName",
    "Address",
    "Payrate",
    "Role",
    "Email",
    "Skills"
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

INSERT INTO "Employees" (
    "EmployeeId",
    "DateOfBirth",
    "FirstName",
    "LastName",
    "Address",
    "Payrate",
    "Role",
    "Email",
    "Skills"
) VALUES (
    gen_random_uuid(), -- Automatically generates a unique UUID
    '1995-03-15',
    'Emily',
    'Clark',
    '456 Elm St, Metropolis, USA',
    25.50,
    2,
    'emily.clark@example.com',
    ARRAY['Cleaning', 'Waiter']
);


UPDATE "Employees"
SET 
    "DateOfBirth" = '1990-05-20',
    "FirstName" = 'John',
    "LastName" = 'Doe',
    "Address" = '123 Maple St, Gotham, USA',
    "Payrate" = 30.00,
    "Role" = 3,
    "Email" = 'john.doe@example.com',
    "Skills" = ARRAY['Cleaning', 'Cooking']
WHERE 
    "EmployeeId" = '27044b39-df22-4d32-8f0e-d96cc3963750';


DROP TABLE employees;

INSERT INTO "Employees" ("EmployeeId", "DateOfBirth", "FirstName", "LastName", "Address", "Payrate", "Role", "Email", "Skills")
SELECT "employee_id", "date_of_birth", "first_name", "last_name", "address", "payrate", "role", "email", "skills"::integer[]
FROM employees;

SELECT "Skills" FROM "Employees";

ALTER TABLE "Employees"
ALTER COLUMN "Skills" TYPE text[];

ALTER TABLE "employeesbefore" RENAME TO "employees";

ALTER TABLE "Employees" ALTER COLUMN "Skills" TYPE integer[];


DELETE FROM "Employees"
WHERE "EmployeeId" IN (
    -- Provide the exact UUIDs here
    '2240799a-b126-43d1-b00e-d51e181f053d', 
    'db5f466a-a152-46e6-93b6-0a7d0b5ad918'
);
