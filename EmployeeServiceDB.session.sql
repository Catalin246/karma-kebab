-- Ensure the pgcrypto extension is available for UUID generation
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create the "Employees" table if it doesn't exist
CREATE TABLE IF NOT EXISTS "Employees" (
    "EmployeeId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DateOfBirth" DATE,
    "FirstName" VARCHAR(100),
    "LastName" VARCHAR(100),
    "Address" TEXT,
    "Payrate" NUMERIC,
    "Roles" INT[],
    "Email" VARCHAR(100),
    "Skills" TEXT[]
);

-- Alter the 'Skills' column to ensure it is of type 'text[]'
ALTER TABLE "Employees"
ALTER COLUMN "Skills" TYPE text[];

-- Insert initial data into the 'Employees' table
INSERT INTO "Employees" (
    "EmployeeId",
    "DateOfBirth",
    "FirstName",
    "LastName",
    "Address",
    "Payrate",
    "Roles",
    "Email",
    "Skills"
) VALUES (
    gen_random_uuid(), 
    '1997-11-28',
    'Jane',
    'Blair',
    '123 Not Main St, Springfield, USA',
    25.50,
    ARRAY[2,3],
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
    "Roles",
    "Email",
    "Skills"
) VALUES (
    gen_random_uuid(), 
    '1995-03-15',
    'Emily',
    'Clark',
    '456 Elm St, Metropolis, USA',
    25.50,
    ARRAY[2],
    'emily.clark@example.com',
    ARRAY['Cleaning', 'Waiter']
);

-- Add more entries if needed
-- Example:
-- INSERT INTO "Employees" (
--     "EmployeeId", "DateOfBirth", "FirstName", "LastName", "Address", 
--     "Payrate", "Role", "Email", "Skills"
-- ) VALUES (
--     gen_random_uuid(), '1992-07-01', 'Alice', 'Johnson', 
--     '789 Oak St, Gotham, USA', 30.00, 3, 'alice.johnson@example.com', ARRAY['Management']
-- );
