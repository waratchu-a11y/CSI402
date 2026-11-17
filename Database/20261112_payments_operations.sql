USE csi402db;

CREATE TABLE IF NOT EXISTS roles (
    RoleId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT IGNORE INTO roles (RoleName, Description) VALUES
    ('Customer', 'ลูกค้า'),
    ('Employee', 'พนักงาน'),
    ('Manager', 'ผู้จัดการ'),
    ('Finance', 'ฝ่ายการเงิน'),
    ('Service', 'ทีมเซอร์วิส'),
    ('Admin', 'ผู้ดูแลระบบ');

CREATE TABLE IF NOT EXISTS add_on_services (
    ServiceId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ServiceCode VARCHAR(50) NOT NULL UNIQUE,
    ServiceName VARCHAR(150) NOT NULL,
    PricePerDay DECIMAL(10,2) NOT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT IGNORE INTO add_on_services (ServiceCode, ServiceName, PricePerDay, IsActive) VALUES
    ('INSURANCE', 'ประกันภัยรถยนต์ชั้น 1', 200, 1),
    ('BABY_SEAT', 'คาร์ซีทสำหรับเด็ก', 100, 1);

CREATE TABLE IF NOT EXISTS system_settings (
    SettingKey VARCHAR(100) NOT NULL PRIMARY KEY,
    SettingValue VARCHAR(500) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT IGNORE INTO system_settings (SettingKey, SettingValue) VALUES
    ('BankName', ''),
    ('BankAccount', ''),
    ('BankAccountName', '');

CREATE TABLE IF NOT EXISTS promotions (
    PromotionId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    PromotionName VARCHAR(150) NOT NULL,
    PromotionCode VARCHAR(50) NULL UNIQUE,
    ConditionType VARCHAR(30) NOT NULL,
    DiscountPercent DECIMAL(5,2) NOT NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO promotions (PromotionName, ConditionType, DiscountPercent, IsActive)
SELECT 'ส่วนลดลูกค้าใหม่', 'new_user', 10, 1
WHERE NOT EXISTS (SELECT 1 FROM promotions WHERE ConditionType = 'new_user');
INSERT INTO promotions (PromotionName, ConditionType, DiscountPercent, IsActive)
SELECT 'เช่าครบ 3 วัน', '3_days', 15, 1
WHERE NOT EXISTS (SELECT 1 FROM promotions WHERE ConditionType = '3_days');
INSERT INTO promotions (PromotionName, ConditionType, DiscountPercent, IsActive)
SELECT 'เช่าระยะยาวครบ 7 วัน', '7_days', 20, 1
WHERE NOT EXISTS (SELECT 1 FROM promotions WHERE ConditionType = '7_days');
INSERT INTO promotions (PromotionName, ConditionType, DiscountPercent, IsActive)
SELECT 'โปรโมชั่นวันจันทร์ถึงพฤหัสบดี', 'weekday', 10, 1
WHERE NOT EXISTS (SELECT 1 FROM promotions WHERE ConditionType = 'weekday');

CREATE TABLE IF NOT EXISTS payments (
    PaymentId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    BookingId INT NOT NULL,
    PaymentType VARCHAR(30) NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentStatus VARCHAR(30) NOT NULL DEFAULT 'pending',
    ProofFileName VARCHAR(255) NULL,
    ReferenceNumber VARCHAR(100) NULL,
    Note VARCHAR(500) NULL,
    ReviewedBy VARCHAR(100) NULL,
    ReviewedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_payments_BookingId (BookingId),
    CONSTRAINT FK_payments_bookings_BookingId
        FOREIGN KEY (BookingId) REFERENCES bookings (BookingId)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS car_inspections (
    InspectionId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    BookingId INT NOT NULL,
    CarId INT NOT NULL,
    InspectionStage VARCHAR(30) NOT NULL,
    Mileage INT NOT NULL,
    FuelLevel INT NOT NULL,
    ConditionNotes VARCHAR(2000) NULL,
    DamageAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    InspectionStatus VARCHAR(30) NOT NULL DEFAULT 'complete',
    InspectedBy VARCHAR(100) NOT NULL,
    InspectedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_inspections_BookingId (BookingId),
    CONSTRAINT FK_inspections_bookings_BookingId
        FOREIGN KEY (BookingId) REFERENCES bookings (BookingId) ON DELETE RESTRICT,
    CONSTRAINT FK_inspections_cars_CarId
        FOREIGN KEY (CarId) REFERENCES cars (CarId) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS maintenance_records (
    MaintenanceId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    CarId INT NOT NULL,
    BookingId INT NULL,
    IssueDescription VARCHAR(2000) NOT NULL,
    RepairCost DECIMAL(10,2) NOT NULL DEFAULT 0,
    MaintenanceStatus VARCHAR(30) NOT NULL DEFAULT 'reported',
    ReportedBy VARCHAR(100) NOT NULL,
    ResolvedBy VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ResolvedAt DATETIME NULL,
    INDEX IX_maintenance_CarId (CarId),
    CONSTRAINT FK_maintenance_cars_CarId
        FOREIGN KEY (CarId) REFERENCES cars (CarId) ON DELETE RESTRICT,
    CONSTRAINT FK_maintenance_bookings_BookingId
        FOREIGN KEY (BookingId) REFERENCES bookings (BookingId) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'usermodels' AND COLUMN_NAME = 'IsActive') = 0,
    'ALTER TABLE usermodels ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'bookings' AND COLUMN_NAME = 'DiscountAmount') = 0,
    'ALTER TABLE bookings ADD COLUMN DiscountAmount DECIMAL(10,2) NOT NULL DEFAULT 0.00',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'bookings' AND COLUMN_NAME = 'PromotionCode') = 0,
    'ALTER TABLE bookings ADD COLUMN PromotionCode VARCHAR(50) NULL',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'bookings' AND COLUMN_NAME = 'LicenseImagePath') = 0,
    'ALTER TABLE bookings ADD COLUMN LicenseImagePath VARCHAR(255) NULL',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'bookings' AND COLUMN_NAME = 'LicenseStatus') = 0,
    'ALTER TABLE bookings ADD COLUMN LicenseStatus VARCHAR(30) NOT NULL DEFAULT ''pending''',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'bookings' AND COLUMN_NAME = 'LicenseReviewNote') = 0,
    'ALTER TABLE bookings ADD COLUMN LicenseReviewNote VARCHAR(500) NULL',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;
