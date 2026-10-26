const express = require('express');
const router = express.Router();

const authController = require('../controllers/authController');
const scenarioController = require('../controllers/scenarioController');
const sessionController = require('../controllers/sessionController');
const { verifyToken, requireRole } = require('../middleware/auth');

// --- Auth Routes ---
router.post('/auth/register', authController.register);
router.post('/auth/login', authController.login);

// --- Scenario Routes ---
router.get('/scenarios', scenarioController.getScenarios);
router.get('/scenarios/:id/steps', scenarioController.getScenarioSteps);

// --- Session & Telemetry Routes ---
router.post('/sessions', sessionController.saveSession);
router.get('/sessions/user/:userId', sessionController.getUserHistory);
router.get('/sessions/leaderboard', sessionController.getLeaderboard);
router.get('/sessions/adaptive-difficulty/:userId', sessionController.getAdaptiveDifficulty);

// --- Instructor Dashboard Route ---
router.get(
  '/instructor/trainee-summary',
  // Can be optionally guarded with verifyToken, requireRole('instructor', 'admin')
  sessionController.getInstructorTraineeSummary
);

module.exports = router;

