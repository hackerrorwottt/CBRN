-- =====================================================================
-- CBRN-Trainer Database Schema (MySQL 8, 3NF Normalized)
-- Problem Statement 269: Educational Training Prototype
-- =====================================================================

CREATE DATABASE IF NOT EXISTS cbrn_trainer CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE cbrn_trainer;

-- 1. Users Table
CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role ENUM('trainee', 'instructor', 'admin') NOT NULL DEFAULT 'trainee',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_users_email (email),
    INDEX idx_users_role (role)
) ENGINE=InnoDB;

-- 2. Scenarios Table (Chemical active, Bio/Rad/Nuc as procedural placeholders)
CREATE TABLE IF NOT EXISTS scenarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    title VARCHAR(150) NOT NULL,
    hazard_type ENUM('chemical', 'biological', 'radiological', 'nuclear') NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_scenarios_hazard_type (hazard_type)
) ENGINE=InnoDB;

-- 3. Scenario Steps Table
CREATE TABLE IF NOT EXISTS scenario_steps (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scenario_id INT NOT NULL,
    step_number INT NOT NULL,
    step_name VARCHAR(100) NOT NULL,
    objective_text TEXT NOT NULL,
    target_time_seconds INT DEFAULT 60,
    max_score INT DEFAULT 15,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (scenario_id) REFERENCES scenarios(id) ON DELETE CASCADE,
    UNIQUE KEY uq_scenario_step (scenario_id, step_number),
    INDEX idx_steps_scenario (scenario_id)
) ENGINE=InnoDB;

-- 4. Training Sessions Table
CREATE TABLE IF NOT EXISTS sessions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    scenario_id INT NOT NULL,
    score INT NOT NULL,
    total_duration_seconds DECIMAL(7, 2) NOT NULL,
    total_mistakes INT NOT NULL DEFAULT 0,
    status ENUM('COMPLETED', 'FAILED') NOT NULL DEFAULT 'COMPLETED',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (scenario_id) REFERENCES scenarios(id) ON DELETE RESTRICT,
    INDEX idx_sessions_user_id (user_id),
    INDEX idx_sessions_score (score),
    INDEX idx_sessions_created_at (created_at)
) ENGINE=InnoDB;

-- 5. Session Step Execution Logs Table (3NF Breakdown)
CREATE TABLE IF NOT EXISTS session_step_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    session_id INT NOT NULL,
    step_number INT NOT NULL,
    step_name VARCHAR(100) NOT NULL,
    duration_seconds DECIMAL(6, 2) NOT NULL,
    mistakes_count INT NOT NULL DEFAULT 0,
    status ENUM('COMPLETED', 'FAILED') NOT NULL DEFAULT 'COMPLETED',
    FOREIGN KEY (session_id) REFERENCES sessions(id) ON DELETE CASCADE,
    INDEX idx_step_logs_session (session_id)
) ENGINE=InnoDB;
