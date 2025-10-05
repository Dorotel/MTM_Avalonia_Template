-- ============================================================================
-- MTM Avalonia Template - Database Schema Migration
-- Version: 001_initial_schema
-- Feature: 002-environment-and-configuration
-- Created: 2025-10-05
-- Database: MySQL 5.7 (MAMP)
-- ============================================================================

-- Set session variables for consistent migration
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================================
-- Table: Users
-- Description: User accounts (may already exist from Feature 001)
-- Note: Uses CREATE IF NOT EXISTS to avoid conflicts
-- ============================================================================

CREATE TABLE IF NOT EXISTS `Users` (
    `UserId` INT NOT NULL AUTO_INCREMENT,
    `Username` VARCHAR(100) NOT NULL,
    `DisplayName` VARCHAR(255) DEFAULT NULL,
    `IsActive` BOOLEAN NOT NULL DEFAULT TRUE,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastLoginAt` DATETIME DEFAULT NULL,
    PRIMARY KEY (`UserId`),
    UNIQUE KEY `UK_Users_Username` (`Username`),
    INDEX `IDX_Users_IsActive` (`IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci
COMMENT='User accounts for MTM Template Application';

-- ============================================================================
-- Table: UserPreferences
-- Description: Stores user-specific configuration preferences
-- Dependencies: Users table must exist
-- ============================================================================

CREATE TABLE IF NOT EXISTS `UserPreferences` (
    `PreferenceId` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `PreferenceKey` VARCHAR(255) NOT NULL,
    `PreferenceValue` TEXT DEFAULT NULL,
    `Category` VARCHAR(100) DEFAULT NULL COMMENT 'e.g., Display, Filters, Sort, UI',
    `LastUpdated` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`PreferenceId`),
    UNIQUE KEY `UK_UserPreferences` (`UserId`, `PreferenceKey`),
    INDEX `IDX_UserPreferences_UserId` (`UserId`),
    INDEX `IDX_UserPreferences_Category` (`Category`),
    CONSTRAINT `FK_UserPreferences_Users`
        FOREIGN KEY (`UserId`)
        REFERENCES `Users`(`UserId`)
        ON DELETE CASCADE
        ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci
COMMENT='User-specific configuration preferences with hierarchical keys';

-- ============================================================================
-- Table: FeatureFlags
-- Description: Feature flags synced from server (launcher integration)
-- Dependencies: None (standalone table)
-- ============================================================================

CREATE TABLE IF NOT EXISTS `FeatureFlags` (
    `FlagId` INT NOT NULL AUTO_INCREMENT,
    `FlagName` VARCHAR(255) NOT NULL,
    `IsEnabled` BOOLEAN NOT NULL DEFAULT FALSE,
    `Environment` VARCHAR(50) DEFAULT NULL COMMENT 'NULL = all environments; values: Development, Staging, Production',
    `RolloutPercentage` INT NOT NULL DEFAULT 0,
    `AppVersion` VARCHAR(50) DEFAULT NULL COMMENT 'Semantic versioning: 1.0.0',
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`FlagId`),
    UNIQUE KEY `UK_FeatureFlags_FlagName` (`FlagName`),
    INDEX `IDX_FeatureFlags_FlagName` (`FlagName`),
    INDEX `IDX_FeatureFlags_Environment` (`Environment`),
    INDEX `IDX_FeatureFlags_AppVersion` (`AppVersion`),
    CONSTRAINT `CHK_FeatureFlags_RolloutPercentage`
        CHECK (`RolloutPercentage` BETWEEN 0 AND 100)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci
COMMENT='Feature flags for phased rollouts and environment-specific features';

-- ============================================================================
-- Sample Data: Test Users
-- Description: Insert test users for development and testing
-- Note: Uses INSERT IGNORE to avoid duplicate key errors
-- ============================================================================

INSERT IGNORE INTO `Users` (`UserId`, `Username`, `DisplayName`, `IsActive`, `CreatedAt`) VALUES
    (42, 'testuser', 'Test User', TRUE, '2025-10-05 14:00:00'),
    (99, 'integrationtest', 'Integration Test User', TRUE, '2025-10-05 14:00:00');

-- ============================================================================
-- Sample Data: UserPreferences
-- Description: Sample preferences for test users
-- ============================================================================

INSERT IGNORE INTO `UserPreferences` (`UserId`, `PreferenceKey`, `PreferenceValue`, `Category`, `LastUpdated`) VALUES
    (42, 'Display.Theme', 'Dark', 'Display', '2025-10-05 14:30:00'),
    (42, 'Filter.DefaultLocation', 'WH-01', 'Filters', '2025-10-05 14:35:00'),
    (42, 'Sort.OrderBy', 'DateDescending', 'Sort', '2025-10-05 14:40:00'),
    (99, 'Display.Theme', 'Dark', 'Display', '2025-10-05 14:30:00'),
    (99, 'Filter.DefaultLocation', 'WH-01', 'Filters', '2025-10-05 14:35:00');

-- ============================================================================
-- Sample Data: FeatureFlags
-- Description: Sample feature flags for development
-- ============================================================================

INSERT IGNORE INTO `FeatureFlags` (`FlagName`, `IsEnabled`, `Environment`, `RolloutPercentage`, `AppVersion`, `UpdatedAt`) VALUES
    ('Visual.UseForItems', TRUE, 'Development', 100, '1.0.0', '2025-10-05 14:30:00'),
    ('OfflineModeAllowed', TRUE, NULL, 50, '1.0.0', '2025-10-05 14:30:00'),
    ('Debug.ShowSqlQueries', TRUE, 'Development', 100, '1.0.0', '2025-10-05 14:30:00'),
    ('Production.AdvancedFeature', FALSE, 'Production', 0, '1.0.0', '2025-10-05 14:30:00'),
    ('TestFeature.Rollout50', TRUE, NULL, 50, '1.0.0', '2025-10-05 14:30:00');

-- ============================================================================
-- Re-enable foreign key checks
-- ============================================================================

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- Verification Queries
-- Description: Run these queries to verify migration success
-- ============================================================================

-- Verify table structure
-- SHOW CREATE TABLE Users;
-- SHOW CREATE TABLE UserPreferences;
-- SHOW CREATE TABLE FeatureFlags;

-- Verify indexes
-- SHOW INDEX FROM UserPreferences;
-- SHOW INDEX FROM FeatureFlags;

-- Verify sample data
-- SELECT COUNT(*) AS UserCount FROM Users;
-- SELECT COUNT(*) AS PreferenceCount FROM UserPreferences;
-- SELECT COUNT(*) AS FlagCount FROM FeatureFlags;

-- ============================================================================
-- Migration Complete
-- ============================================================================
