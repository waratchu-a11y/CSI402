USE csi402db;

ALTER TABLE cars
    ADD COLUMN TransmissionType VARCHAR(20) NULL,
    ADD COLUMN Seats INT NULL;

UPDATE cars
SET Seats = CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(Details, ' ที่นั่ง', 1), ' ', -1) AS UNSIGNED),
    TransmissionType = CASE
        WHEN Details LIKE '%อัตโนมัติ%' THEN 'automatic'
        WHEN Details LIKE '%ธรรมดา%' THEN 'manual'
        ELSE NULL
    END
WHERE Details REGEXP '[0-9]+ ที่นั่ง';

ALTER TABLE bookings
    ADD COLUMN CarId INT NULL,
    ADD COLUMN ReturnLocation VARCHAR(150) NULL,
    ADD COLUMN PickupTime TIME NULL,
    ADD COLUMN ReturnDate DATE NULL,
    ADD COLUMN ReturnTime TIME NULL,
    ADD COLUMN DepositAmount DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    ADD COLUMN BookingStatus VARCHAR(30) NOT NULL DEFAULT 'pending';

UPDATE bookings b
JOIN cars c ON c.CarName = b.CarModel
SET b.CarId = c.CarId,
    b.ReturnDate = DATE_ADD(STR_TO_DATE(b.PickupDate, '%Y-%m-%d'), INTERVAL b.RentalDays DAY),
    b.DepositAmount = ROUND(b.TotalPrice * 0.10, 2);

ALTER TABLE bookings
    ADD INDEX IX_bookings_CarId (CarId),
    ADD CONSTRAINT FK_bookings_cars_CarId
        FOREIGN KEY (CarId) REFERENCES cars (CarId)
        ON UPDATE CASCADE
        ON DELETE RESTRICT;
