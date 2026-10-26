USE cbrn_trainer;

-- Default Users (Passwords hashed with bcrypt: 'password123')
-- Hash: $2b$10$Y10H6g6eCiqQx4x7Vq7ygeYw8V7G1k4qWz0kXy7w6E7d0E3kQ3e2q (analogue)
INSERT INTO users (username, email, password_hash, role) VALUES
('trainee_shlok', 'trainee@cbrn.gov.in', '$2b$10$tZ9v7o9U9eZ4i4k6m8o2uev9Z3q8X2a5c7e0m1k3q5w7e9r1t3y5u', 'trainee'),
('instructor_singh', 'instructor@cbrn.gov.in', '$2b$10$tZ9v7o9U9eZ4i4k6m8o2uev9Z3q8X2a5c7e0m1k3q5w7e9r1t3y5u', 'instructor'),
('admin_ndrf', 'admin@cbrn.gov.in', '$2b$10$tZ9v7o9U9eZ4i4k6m8o2uev9Z3q8X2a5c7e0m1k3q5w7e9r1t3y5u', 'admin')
ON DUPLICATE KEY UPDATE username=VALUES(username);

-- Scenarios (1 Chemical active, 3 procedural placeholders)
INSERT INTO scenarios (id, code, title, hazard_type, description, is_active) VALUES
(1, 'CHEM-IND-01', 'Industrial Toxic Vapor Leak Response', 'chemical', 'Educational simulation for response to an uncontained chemical vapor release at an industrial storage facility. Covers identification, Level A PPE donning, valve containment, and casualty extraction.', TRUE),
(2, 'BIO-ISOL-01', 'Biological Outbreak Quarantine & Isolation (Future Scope)', 'biological', 'Procedural protocol training: barrier establishment, quarantine perimeter, and contact tracing triage.', FALSE),
(3, 'RAD-PERIM-01', 'Radiological Lost Source Perimeter Survey (Future Scope)', 'radiological', 'Procedural protocol training: Inverse-square law distance maintenance, shielding deployment, and cordon setup.', FALSE),
(4, 'NUC-EVAC-01', 'Nuclear Facility Perimeter Evacuation (Future Scope)', 'nuclear', 'Procedural protocol training: Population shelter-in-place instructions, thyroid blocking distribution, and decontamination triage.', FALSE)
ON DUPLICATE KEY UPDATE title=VALUES(title);

-- 7 Scenario Steps for CHEM-IND-01
INSERT INTO scenario_steps (scenario_id, step_number, step_name, objective_text, target_time_seconds, max_score) VALUES
(1, 1, 'Briefing & Ingestion', 'Acknowledge incident report and inspect facility layout at the Cold Zone Briefing Board.', 60, 15),
(1, 2, 'Hazard Identification', 'Identify the chemical vapor threat via the UN Hazmat diamond placard on the chemical tank.', 60, 15),
(1, 3, 'PPE Selection', 'Select and equip the certified Level A fully encapsulating suit with SCBA at the staging rack.', 60, 15),
(1, 4, 'Safety Zone Assessment', 'Establish safe perimeter and confirm personal protection before crossing into the hazard area.', 60, 15),
(1, 5, 'Casualty Triage & Rescue', 'Locate the trapped technician in the Hot Zone and calm them to initiate evacuation.', 120, 20),
(1, 6, 'Evacuation to Triage', 'Escort the casualty safely out of the hot zone into the Decontamination Triage Point.', 90, 10),
(1, 7, 'Debrief & Score Evaluation', 'Review mistake logs, exposure telemetry, and final simulation performance report.', 30, 10)
ON DUPLICATE KEY UPDATE step_name=VALUES(step_name);
