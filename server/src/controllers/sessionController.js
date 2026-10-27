const db = require('../config/db');

/**
 * Saves a completed session in ONE atomic transaction with its individual step logs.
 */
async function saveSession(req, res) {
  let connection;
  try {
    const {
      user_id,
      scenario_id,
      score,
      total_duration_seconds,
      total_mistakes,
      status,
      step_logs
    } = req.body;

    if (!user_id || !scenario_id || score === undefined || total_duration_seconds === undefined) {
      return res.status(400).json({
        success: false,
        error: 'VALIDATION_ERROR',
        message: 'user_id, scenario_id, score, and total_duration_seconds are required.'
      });
    }

    const sessionStatus = status === 'FAILED' ? 'FAILED' : 'COMPLETED';

    connection = await db.getConnection();
    await connection.beginTransaction();

    // 1. Insert session record
    const [sessionResult] = await connection.execute(
      `INSERT INTO sessions (user_id, scenario_id, score, total_duration_seconds, total_mistakes, status)
       VALUES (?, ?, ?, ?, ?, ?)`,
      [
        user_id,
        scenario_id,
        score,
        total_duration_seconds,
        total_mistakes || 0,
        sessionStatus
      ]
    );

    const sessionId = sessionResult.insertId;

    // 2. Insert step logs in batch if present
    if (Array.isArray(step_logs) && step_logs.length > 0) {
      const stepInsertQuery = `
        INSERT INTO session_step_logs (session_id, step_number, step_name, duration_seconds, mistakes_count, status)
        VALUES (?, ?, ?, ?, ?, ?)
      `;

      for (const log of step_logs) {
        await connection.execute(stepInsertQuery, [
          sessionId,
          log.step_number || 1,
          log.step_name || 'Step',
          log.duration_seconds || 0,
          log.mistakes_count || 0,
          log.status === 'FAILED' ? 'FAILED' : 'COMPLETED'
        ]);
      }
    }

    await connection.commit();

    return res.status(201).json({
      success: true,
      message: 'Training session and step logs saved successfully.',
      session_id: sessionId
    });
  } catch (error) {
    if (connection) {
      await connection.rollback();
    }
    console.error('[sessionController.saveSession] Transaction rolled back. Error:', error);
    return res.status(500).json({
      success: false,
      error: 'TRANSACTION_FAILED',
      message: 'Failed to record session in database transaction.'
    });
  } finally {
    if (connection) {
      connection.release();
    }
  }
}

/**
 * Retrieves historical sessions for a given trainee.
 */
async function getUserHistory(req, res) {
  try {
    const userId = parseInt(req.params.userId, 10);
    if (isNaN(userId)) {
      return res.status(400).json({
        success: false,
        error: 'INVALID_ID',
        message: 'User ID must be an integer.'
      });
    }

    const [sessions] = await db.execute(
      `SELECT s.id, s.scenario_id, sc.title AS scenario_title, s.score,
              s.total_duration_seconds, s.total_mistakes, s.status, s.created_at
       FROM sessions s
       JOIN scenarios sc ON s.scenario_id = sc.id
       WHERE s.user_id = ?
       ORDER BY s.created_at DESC`,
      [userId]
    );

    return res.status(200).json({
      success: true,
      user_id: userId,
      count: sessions.length,
      data: sessions
    });
  } catch (error) {
    console.error('[sessionController.getUserHistory] Error:', error);
    return res.status(500).json({
      success: false,
      error: 'SERVER_ERROR',
      message: 'Failed to fetch user history.'
    });
  }
}

/**
 * Returns top performing trainees ordered by highest score and lowest duration.
 */
async function getLeaderboard(req, res) {
  try {
    const limit = Math.max(1, Math.min(100, parseInt(req.query.limit, 10) || 10));

    const [rows] = await db.query(
      `SELECT s.id, u.username, sc.title AS scenario_title, s.score,
              s.total_duration_seconds, s.total_mistakes, s.created_at
       FROM sessions s
       JOIN users u ON s.user_id = u.id
       JOIN scenarios sc ON s.scenario_id = sc.id
       WHERE s.status = 'COMPLETED'
       ORDER BY s.score DESC, s.total_duration_seconds ASC
       LIMIT ?`,
      [limit]
    );

    return res.status(200).json({
      success: true,
      count: rows.length,
      data: rows
    });
  } catch (error) {
    console.error('[sessionController.getLeaderboard] Error:', error);
    return res.status(500).json({
      success: false,
      error: 'SERVER_ERROR',
      message: 'Failed to fetch leaderboard.'
    });
  }
}

/**
 * Adaptive Difficulty calculation:
 * Evaluates trainee's last 5 sessions:
 * - avg mistakes <= 2: harder (multiplier 1.3, faster hazard spread, fewer hints)
 * - avg mistakes 3-5: same (multiplier 1.0, standard hazard spread)
 * - avg mistakes > 5: easier (multiplier 0.7, slower hazard spread, proactive hints)
 */
async function getAdaptiveDifficulty(req, res) {
  try {
    const userId = parseInt(req.params.userId, 10);
    if (isNaN(userId)) {
      return res.status(400).json({
        success: false,
        error: 'INVALID_ID',
        message: 'User ID must be an integer.'
      });
    }

    const [recentSessions] = await db.execute(
      `SELECT total_mistakes FROM sessions
       WHERE user_id = ?
       ORDER BY created_at DESC
       LIMIT 5`,
      [userId]
    );

    if (recentSessions.length === 0) {
      return res.status(200).json({
        success: true,
        user_id: userId,
        adaptive_difficulty_multiplier: 1.0,
        average_mistakes_last_5: 0,
        recommendation: 'Baseline difficulty (no previous sessions recorded).'
      });
    }

    const totalMistakes = recentSessions.reduce((acc, s) => acc + s.total_mistakes, 0);
    const avgMistakes = totalMistakes / recentSessions.length;

    let multiplier = 1.0;
    let recommendation = 'Standard difficulty maintaining baseline hazard dynamics.';

    if (avgMistakes <= 2.0) {
      multiplier = 1.3;
      recommendation = 'Advanced responder: Increased hazard spread speed (+30%) and reduced hint prompts.';
    } else if (avgMistakes > 5.0) {
      multiplier = 0.7;
      recommendation = 'Remedial training: Decreased hazard spread speed (-30%) and enhanced virtual instructor cues.';
    }

    return res.status(200).json({
      success: true,
      user_id: userId,
      sessions_analyzed: recentSessions.length,
      average_mistakes_last_5: parseFloat(avgMistakes.toFixed(2)),
      adaptive_difficulty_multiplier: multiplier,
      recommendation
    });
  } catch (error) {
    console.error('[sessionController.getAdaptiveDifficulty] Error:', error);
    return res.status(500).json({
      success: false,
      error: 'SERVER_ERROR',
      message: 'Failed to compute adaptive difficulty.'
    });
  }
}

/**
 * Instructor Dashboard Endpoint:
 * Aggregates all trainee sessions, pass rates, average scores, and mistake trends.
 */
async function getInstructorTraineeSummary(req, res) {
  try {
    const [traineeSummary] = await db.execute(`
      SELECT u.id AS user_id, u.username, u.email,
             COUNT(s.id) AS total_sessions,
             COALESCE(AVG(s.score), 0) AS average_score,
             COALESCE(SUM(CASE WHEN s.score >= 70 THEN 1 ELSE 0 END), 0) AS passed_sessions,
             COALESCE(AVG(s.total_mistakes), 0) AS average_mistakes,
             MAX(s.created_at) AS last_active
      FROM users u
      LEFT JOIN sessions s ON u.id = s.user_id
      WHERE u.role = 'trainee'
      GROUP BY u.id, u.username, u.email
      ORDER BY last_active DESC
    `);

    const [aggregateStats] = await db.execute(`
      SELECT COUNT(id) AS total_runs,
             COALESCE(AVG(score), 0) AS overall_avg_score,
             COALESCE(AVG(total_duration_seconds), 0) AS overall_avg_duration,
             COALESCE(SUM(CASE WHEN score >= 70 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(id), 0), 0) AS overall_pass_rate
      FROM sessions
    `);

    return res.status(200).json({
      success: true,
      analytics: aggregateStats[0],
      trainees: traineeSummary
    });
  } catch (error) {
    console.error('[sessionController.getInstructorTraineeSummary] Error:', error);
    return res.status(500).json({
      success: false,
      error: 'SERVER_ERROR',
      message: 'Failed to retrieve instructor summary.'
    });
  }
}

module.exports = {
  saveSession,
  getUserHistory,
  getLeaderboard,
  getAdaptiveDifficulty,
  getInstructorTraineeSummary
};

