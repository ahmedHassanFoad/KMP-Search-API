-- KMP Search Database Seed Script
-- This script seeds the database with comprehensive test data
-- 5 Departments and 25+ Documents with varied content for testing search functionality

USE KMPSearchDb;
GO

-- Clear existing data (optional - for clean slate)
DELETE FROM Documents;
DELETE FROM SearchQueries;
DELETE FROM Departments;
GO

-- Seed Departments
DECLARE @ITDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @HRDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @FinanceDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @OperationsDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @LegalDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Departments (Id, Name, Description, CreatedAt, UpdatedAt) VALUES
(@ITDeptId, 'Information Technology', 'IT infrastructure, development, and support', GETUTCDATE(), NULL),
(@HRDeptId, 'Human Resources', 'Employee relations, recruitment, and benefits', GETUTCDATE(), NULL),
(@FinanceDeptId, 'Finance', 'Financial planning, accounting, and budgeting', GETUTCDATE(), NULL),
(@OperationsDeptId, 'Operations', 'Daily operations and process management', GETUTCDATE(), NULL),
(@LegalDeptId, 'Legal', 'Legal compliance and contract management', GETUTCDATE(), NULL);

PRINT 'Departments seeded successfully';
GO

-- Seed Documents with varied content
DECLARE @ITDeptId UNIQUEIDENTIFIER = (SELECT Id FROM Departments WHERE Name = 'Information Technology');
DECLARE @HRDeptId UNIQUEIDENTIFIER = (SELECT Id FROM Departments WHERE Name = 'Human Resources');
DECLARE @FinanceDeptId UNIQUEIDENTIFIER = (SELECT Id FROM Departments WHERE Name = 'Finance');
DECLARE @OperationsDeptId UNIQUEIDENTIFIER = (SELECT Id FROM Departments WHERE Name = 'Operations');
DECLARE @LegalDeptId UNIQUEIDENTIFIER = (SELECT Id FROM Departments WHERE Name = 'Legal');
DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();

-- IT Department Documents
INSERT INTO Documents (Id, Title, Description, Category, Tags, FilePath, FileSize, MimeType, DepartmentId, CreatedBy, CreatedAt, IsDeleted) VALUES
(NEWID(), 'IT Security Policy 2024', 'Comprehensive information security policy covering data protection, access control, and incident response procedures for all company systems.', 'IT', 'security,policy,annual,IT,compliance', '/documents/it-security-policy-2024.pdf', 2048576, 'application/pdf', @ITDeptId, @AdminUserId, DATEADD(MONTH, -2, GETUTCDATE()), 0),
(NEWID(), 'Network Infrastructure Budget 2025', 'Budget planning document for network infrastructure upgrades including routers, switches, and security appliances.', 'IT', 'budget,planning,network,annual,2025', '/documents/network-budget-2025.pdf', 1536000, 'application/pdf', @ITDeptId, @AdminUserId, DATEADD(MONTH, -1, GETUTCDATE()), 0),
(NEWID(), 'Software Development Best Practices', 'Best practices guide for software development teams including coding standards, testing procedures, and deployment guidelines.', 'Engineering', 'development,policy,guidelines,engineering', '/documents/dev-best-practices.pdf', 3145728, 'application/pdf', @ITDeptId, @AdminUserId, DATEADD(MONTH, -6, GETUTCDATE()), 0),
(NEWID(), 'Cybersecurity Incident Report Q1 2024', 'Quarterly cybersecurity incident report detailing security events, responses, and remediation actions.', 'IT', 'security,quarterly,report,incident', '/documents/security-report-q1-2024.pdf', 1024000, 'application/pdf', @ITDeptId, @AdminUserId, DATEADD(MONTH, -9, GETUTCDATE()), 0),
(NEWID(), 'Cloud Migration Strategy', 'Strategic plan for migrating on-premise infrastructure to cloud platforms including timeline and risk assessment.', 'IT', 'cloud,planning,migration,strategy', '/documents/cloud-migration.pdf', 2560000, 'application/pdf', @ITDeptId, @AdminUserId, DATEADD(YEAR, -1, GETUTCDATE()), 0);

-- HR Department Documents
INSERT INTO Documents (Id, Title, Description, Category, Tags, FilePath, FileSize, MimeType, DepartmentId, CreatedBy, CreatedAt, IsDeleted) VALUES
(NEWID(), 'Employee Handbook 2024', 'Comprehensive employee handbook covering company policies, benefits, code of conduct, and workplace guidelines.', 'HR', 'policy,handbook,annual,2024,employee', '/documents/employee-handbook-2024.pdf', 4194304, 'application/pdf', @HRDeptId, @AdminUserId, DATEADD(MONTH, -3, GETUTCDATE()), 0),
(NEWID(), 'Recruitment Policy and Procedures', 'Standard operating procedures for recruitment activities including job posting, interviewing, and onboarding processes.', 'HR', 'policy,recruitment,hiring,procedures', '/documents/recruitment-policy.pdf', 1843200, 'application/pdf', @HRDeptId, @AdminUserId, DATEADD(MONTH, -8, GETUTCDATE()), 0),
(NEWID(), 'Annual Performance Review Guidelines', 'Guidelines for conducting annual performance reviews including evaluation criteria and review schedules.', 'HR', 'annual,performance,review,guidelines', '/documents/performance-review.pdf', 921600, 'application/pdf', @HRDeptId, @AdminUserId, DATEADD(MONTH, -4, GETUTCDATE()), 0),
(NEWID(), 'Workplace Diversity and Inclusion Report 2024', 'Annual report on diversity and inclusion initiatives, metrics, and future goals for creating an inclusive workplace.', 'HR', 'diversity,inclusion,annual,report,2024', '/documents/diversity-report-2024.pdf', 1536000, 'application/pdf', @HRDeptId, @AdminUserId, DATEADD(MONTH, -2, GETUTCDATE()), 0),
(NEWID(), 'Benefits Enrollment Guide 2025', 'Comprehensive guide for employee benefits enrollment including health insurance, retirement plans, and other benefits.', 'HR', 'benefits,enrollment,guide,annual,2025', '/documents/benefits-guide-2025.pdf', 2048000, 'application/pdf', @HRDeptId, @AdminUserId, DATEADD(MONTH, -1, GETUTCDATE()), 0),
(NEWID(), 'Remote Work Policy', 'Policy document outlining remote work eligibility, expectations, equipment, and communication guidelines.', 'HR', 'policy,remote,work,flexibility', '/documents/remote-work-policy.pdf', 1228800, 'application/pdf', @HRDeptId, @AdminUserId, DATEADD(YEAR, -2, GETUTCDATE()), 0);

-- Finance Department Documents
INSERT INTO Documents (Id, Title, Description, Category, Tags, FilePath, FileSize, MimeType, DepartmentId, CreatedBy, CreatedAt, IsDeleted) VALUES
(NEWID(), 'Annual Financial Report 2024', 'Comprehensive annual financial report including balance sheets, income statements, cash flow analysis, and auditor notes.', 'Finance', 'annual,report,financial,2024,accounting', '/documents/annual-report-2024.pdf', 5242880, 'application/pdf', @FinanceDeptId, @AdminUserId, DATEADD(MONTH, -2, GETUTCDATE()), 0),
(NEWID(), 'Q4 Budget Report 2024', 'Quarterly budget report for Q4 2024 showing budget vs actual spending across all departments.', 'Finance', 'quarterly,budget,report,Q4,2024', '/documents/q4-budget-2024.pdf', 2048000, 'application/pdf', @FinanceDeptId, @AdminUserId, DATEADD(MONTH, -1, GETUTCDATE()), 0),
(NEWID(), 'Corporate Budget Planning 2025', 'Corporate budget planning document for fiscal year 2025 with departmental allocations and strategic initiatives funding.', 'Finance', 'budget,planning,annual,2025,corporate', '/documents/budget-planning-2025.pdf', 3670016, 'application/pdf', @FinanceDeptId, @AdminUserId, DATEADD(MONTH, -4, GETUTCDATE()), 0),
(NEWID(), 'Financial Forecasting Model Q1-Q4 2024', 'Financial forecasting model with quarterly projections for revenue, expenses, and profitability.', 'Finance', 'forecasting,planning,quarterly,2024', '/documents/financial-forecast-2024.xlsx', 614400, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', @FinanceDeptId, @AdminUserId, DATEADD(MONTH, -10, GETUTCDATE()), 0),
(NEWID(), 'Tax Compliance Report 2023', 'Annual tax compliance report detailing tax obligations, filings, and compliance status.', 'Finance', 'tax,compliance,annual,report,2023', '/documents/tax-compliance-2023.pdf', 1843200, 'application/pdf', @FinanceDeptId, @AdminUserId, DATEADD(YEAR, -1, GETUTCDATE()), 0),
(NEWID(), 'Expense Reimbursement Policy', 'Policy document for employee expense reimbursement including eligible expenses, limits, and submission procedures.', 'Finance', 'policy,expense,reimbursement,guidelines', '/documents/expense-policy.pdf', 1024000, 'application/pdf', @FinanceDeptId, @AdminUserId, DATEADD(YEAR, -2, GETUTCDATE()), 0);

-- Operations Department Documents
INSERT INTO Documents (Id, Title, Description, Category, Tags, FilePath, FileSize, MimeType, DepartmentId, CreatedBy, CreatedAt, IsDeleted) VALUES
(NEWID(), 'Operations Manual 2024', 'Comprehensive operations manual covering standard operating procedures for all operational processes.', 'Operations', 'manual,operations,procedures,2024', '/documents/operations-manual-2024.pdf', 6291456, 'application/pdf', @OperationsDeptId, @AdminUserId, DATEADD(MONTH, -5, GETUTCDATE()), 0),
(NEWID(), 'Supply Chain Management Report Q3 2024', 'Quarterly supply chain management report analyzing vendor performance, inventory levels, and logistics efficiency.', 'Operations', 'supply,chain,quarterly,report,Q3,2024', '/documents/supply-chain-q3-2024.pdf', 2560000, 'application/pdf', @OperationsDeptId, @AdminUserId, DATEADD(MONTH, -3, GETUTCDATE()), 0),
(NEWID(), 'Quality Assurance Standards', 'Quality assurance standards and testing procedures for product quality control and improvement.', 'Operations', 'quality,assurance,standards,procedures', '/documents/qa-standards.pdf', 1638400, 'application/pdf', @OperationsDeptId, @AdminUserId, DATEADD(MONTH, -7, GETUTCDATE()), 0),
(NEWID(), 'Facility Management Annual Report 2024', 'Annual report on facility management including maintenance activities, costs, and improvement projects.', 'Operations', 'annual,report,facility,management,2024', '/documents/facility-report-2024.pdf', 2867200, 'application/pdf', @OperationsDeptId, @AdminUserId, DATEADD(MONTH, -2, GETUTCDATE()), 0),
(NEWID(), 'Business Continuity Plan', 'Business continuity and disaster recovery plan outlining procedures for maintaining operations during disruptions.', 'Operations', 'continuity,disaster,recovery,planning', '/documents/business-continuity.pdf', 3145728, 'application/pdf', @OperationsDeptId, @AdminUserId, DATEADD(YEAR, -1, GETUTCDATE()), 0);

-- Legal Department Documents
INSERT INTO Documents (Id, Title, Description, Category, Tags, FilePath, FileSize, MimeType, DepartmentId, CreatedBy, CreatedAt, IsDeleted) VALUES
(NEWID(), 'Legal Compliance Handbook 2024', 'Comprehensive legal compliance handbook covering regulatory requirements, corporate governance, and legal obligations.', 'Legal', 'compliance,handbook,annual,legal,2024', '/documents/legal-compliance-2024.pdf', 4718592, 'application/pdf', @LegalDeptId, @AdminUserId, DATEADD(MONTH, -3, GETUTCDATE()), 0),
(NEWID(), 'Contract Management Guidelines', 'Guidelines for contract lifecycle management including drafting, review, approval, and monitoring procedures.', 'Legal', 'contract,guidelines,policy,management', '/documents/contract-guidelines.pdf', 1433600, 'application/pdf', @LegalDeptId, @AdminUserId, DATEADD(MONTH, -6, GETUTCDATE()), 0),
(NEWID(), 'Intellectual Property Policy', 'Policy document covering intellectual property rights, patents, trademarks, and trade secrets protection.', 'Legal', 'policy,intellectual,property,IP,patents', '/documents/ip-policy.pdf', 1843200, 'application/pdf', @LegalDeptId, @AdminUserId, DATEADD(YEAR, -1, GETUTCDATE()), 0),
(NEWID(), 'Data Privacy Compliance Report 2024', 'Annual data privacy compliance report covering GDPR, CCPA, and other data protection regulations.', 'Legal', 'privacy,compliance,annual,report,GDPR,2024', '/documents/privacy-compliance-2024.pdf', 2252800, 'application/pdf', @LegalDeptId, @AdminUserId, DATEADD(MONTH, -4, GETUTCDATE()), 0),
(NEWID(), 'Corporate Governance Framework', 'Corporate governance framework document outlining board responsibilities, ethics policies, and compliance structures.', 'Legal', 'governance,compliance,policy,corporate', '/documents/governance-framework.pdf', 2867200, 'application/pdf', @LegalDeptId, @AdminUserId, DATEADD(YEAR, -2, GETUTCDATE()), 0);

PRINT 'Documents seeded successfully - Total: 26 documents';
GO

-- Seed some sample search queries for autocomplete
INSERT INTO SearchQueries (Id, QueryText, SearchCount, LastSearchedAt, CreatedAt) VALUES
(NEWID(), 'annual report', 45, GETUTCDATE(), GETUTCDATE()),
(NEWID(), 'budget', 32, GETUTCDATE(), GETUTCDATE()),
(NEWID(), 'policy', 28, GETUTCDATE(), GETUTCDATE()),
(NEWID(), 'security', 25, GETUTCDATE(), GETUTCDATE()),
(NEWID(), 'compliance', 22, GETUTCDATE(), GETUTCDATE()),
(NEWID(), 'quarterly report', 18, GETUTCDATE(), GETUTCDATE()),
(NEWID(), 'planning', 15, GETUTCDATE(), GETUTCDATE()),
(NEWID(), 'guidelines', 12, GETUTCDATE(), GETUTCDATE()),
(NEWID(), 'handbook', 10, GETUTCDATE(), GETUTCDATE()),
(NEWID(), 'financial', 8, GETUTCDATE(), GETUTCDATE());

PRINT 'Search queries seeded successfully';
GO

PRINT 'Database seeding completed successfully!';
PRINT '--------------------------------';
PRINT 'Summary:';
PRINT '- Departments: 5';
PRINT '- Documents: 26';
PRINT '- Search Queries: 10';
PRINT '--------------------------------';
SELECT 'Documents by Category' AS Summary, Category, COUNT(*) AS Count 
FROM Documents 
GROUP BY Category 
ORDER BY Count DESC;

SELECT 'Documents by Department' AS Summary, d.Name AS Department, COUNT(doc.Id) AS Count
FROM Departments d
LEFT JOIN Documents doc ON d.Id = doc.DepartmentId
GROUP BY d.Name
ORDER BY Count DESC;
GO
