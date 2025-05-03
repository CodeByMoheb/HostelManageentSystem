-- Add missing columns to Hostels table
ALTER TABLE `Hostels`
ADD COLUMN IF NOT EXISTS `Area` VARCHAR(50) NOT NULL AFTER `City`,
ADD COLUMN IF NOT EXISTS `AvailableRooms` INT NOT NULL DEFAULT 0 AFTER `TotalRooms`,
ADD COLUMN IF NOT EXISTS `SecurityDeposit` DECIMAL(65,30) NOT NULL DEFAULT 0 AFTER `AvailableRooms`,
ADD COLUMN IF NOT EXISTS `PricePerRoom` DECIMAL(65,30) NOT NULL DEFAULT 0 AFTER `SecurityDeposit`,
ADD COLUMN IF NOT EXISTS `Description` VARCHAR(1000) NOT NULL DEFAULT '' AFTER `PricePerRoom`,
ADD COLUMN IF NOT EXISTS `IsActive` BOOLEAN NOT NULL DEFAULT TRUE AFTER `Description`,
ADD COLUMN IF NOT EXISTS `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `IsActive`;

-- Update existing records to set default values
UPDATE `Hostels` SET
    `Area` = 'Unknown' WHERE `Area` IS NULL,
    `AvailableRooms` = `TotalRooms` WHERE `AvailableRooms` IS NULL,
    `SecurityDeposit` = 0 WHERE `SecurityDeposit` IS NULL,
    `PricePerRoom` = 0 WHERE `PricePerRoom` IS NULL,
    `Description` = '' WHERE `Description` IS NULL,
    `IsActive` = TRUE WHERE `IsActive` IS NULL,
    `CreatedAt` = CURRENT_TIMESTAMP WHERE `CreatedAt` IS NULL; 