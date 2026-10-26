const db = require('../config/db');

async function getScenarios(req, res) {
  try {
    const [rows] = await db.execute(
      'SELECT id, code, title, hazard_type, description, is_active FROM scenarios ORDER BY id ASC'
    );

    return res.status(200).json({
      success: true,
      count: rows.length,
      data: rows
    });
  } catch (error) {
    console.error('[scenarioController.getScenarios] Error:', error);
    return res.status(500).json({
      success: false,
      error: 'SERVER_ERROR',
      message: 'Failed to retrieve training scenarios.'
    });
  }
}

async function getScenarioSteps(req, res) {
  try {
    const scenarioId = parseInt(req.params.id, 10);
    if (isNaN(scenarioId)) {
      return res.status(400).json({
        success: false,
        error: 'INVALID_ID',
        message: 'Scenario ID must be a valid integer.'
      });
    }

    const [steps] = await db.execute(
      'SELECT id, scenario_id, step_number, step_name, objective_text, target_time_seconds, max_score FROM scenario_steps WHERE scenario_id = ? ORDER BY step_number ASC',
      [scenarioId]
    );

    return res.status(200).json({
      success: true,
      scenario_id: scenarioId,
      count: steps.length,
      data: steps
    });
  } catch (error) {
    console.error('[scenarioController.getScenarioSteps] Error:', error);
    return res.status(500).json({
      success: false,
      error: 'SERVER_ERROR',
      message: 'Failed to retrieve scenario steps.'
    });
  }
}

module.exports = {
  getScenarios,
  getScenarioSteps
};
