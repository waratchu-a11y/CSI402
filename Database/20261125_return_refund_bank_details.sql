USE csi402db;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'bookings' AND COLUMN_NAME = 'RefundBankName') = 0,
    'ALTER TABLE bookings ADD COLUMN RefundBankName VARCHAR(100) NULL',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'bookings' AND COLUMN_NAME = 'RefundAccountName') = 0,
    'ALTER TABLE bookings ADD COLUMN RefundAccountName VARCHAR(150) NULL',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'bookings' AND COLUMN_NAME = 'RefundAccountNumber') = 0,
    'ALTER TABLE bookings ADD COLUMN RefundAccountNumber VARCHAR(30) NULL',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'bookings' AND COLUMN_NAME = 'RefundPhone') = 0,
    'ALTER TABLE bookings ADD COLUMN RefundPhone VARCHAR(20) NULL',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;
