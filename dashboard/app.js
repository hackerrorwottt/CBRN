// Instructor Dashboard Frontend Logic
const API_BASE = 'http://localhost:3000/api';

let scoreChartInstance = null;
let mistakeChartInstance = null;

async function loadDashboardData() {
  try {
    const response = await fetch(`${API_BASE}/instructor/trainee-summary`);
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
    const data = await response.json();
    renderDashboard(data);
  } catch (err) {
    console.warn('[Dashboard] Live API server unreachable at localhost:3000. Loading baseline demonstration data.', err);
    renderDemoData();
  }
}

function renderDashboard(data) {
  const analytics = data.analytics || {};
  const trainees = data.trainees || [];

  // Update KPIs
  document.getElementById('kpiTotalRuns').textContent = analytics.total_runs || 0;
  document.getElementById('kpiAvgScore').textContent = `${Math.round(analytics.overall_avg_score || 0)} / 100`;
  document.getElementById('kpiPassRate').textContent = `${Math.round(analytics.overall_pass_rate || 0)}%`;
  document.getElementById('kpiAvgDuration').textContent = `${Math.round(analytics.overall_avg_duration || 0)}s`;

  // Render Table
  renderTraineeTable(trainees);

  // Render Charts
  const traineeNames = trainees.map(t => t.username);
  const traineeScores = trainees.map(t => Math.round(t.average_score));
  renderScoreChart(traineeNames, traineeScores);
  renderMistakeChart([40, 30, 20, 10]); // Proportional distribution
}

function renderDemoData() {
  document.getElementById('kpiTotalRuns').textContent = '14';
  document.getElementById('kpiAvgScore').textContent = '82 / 100';
  document.getElementById('kpiPassRate').textContent = '78%';
  document.getElementById('kpiAvgDuration').textContent = '184s';

  const demoTrainees = [
    { username: 'trainee_shlok', email: 'trainee@cbrn.gov.in', total_sessions: 6, average_score: 92, average_mistakes: 1.2 },
    { username: 'trainee_rahul', email: 'rahul@ndrf.gov.in', total_sessions: 4, average_score: 84, average_mistakes: 2.8 },
    { username: 'trainee_priya', email: 'priya@ndrf.gov.in', total_sessions: 3, average_score: 68, average_mistakes: 5.4 },
    { username: 'trainee_amit', email: 'amit@ndrf.gov.in', total_sessions: 1, average_score: 55, average_mistakes: 7.0 }
  ];

  renderTraineeTable(demoTrainees);
  renderScoreChart(demoTrainees.map(t => t.username), demoTrainees.map(t => t.average_score));
  renderMistakeChart([42, 28, 18, 12]);
}

function renderTraineeTable(trainees) {
  const tbody = document.getElementById('traineeTableBody');
  tbody.innerHTML = '';

  if (trainees.length === 0) {
    tbody.innerHTML = '<tr><td colspan="6" style="text-align: center;">No trainee records found.</td></tr>';
    return;
  }

  trainees.forEach(t => {
    const avgMistakes = parseFloat(t.average_mistakes || 0);
    let adaptiveLabel = 'Standard (1.0x)';
    let adaptiveClass = 'status-pass';

    if (avgMistakes <= 2.0 && t.total_sessions >= 2) {
      adaptiveLabel = 'Advanced (1.3x Harder)';
    } else if (avgMistakes > 5.0) {
      adaptiveLabel = 'Remedial (0.7x Easier + Hints)';
      adaptiveClass = 'status-retry';
    }

    const row = document.createElement('tr');
    row.innerHTML = `
      <td><strong>${t.username}</strong></td>
      <td style="color: var(--text-muted);">${t.email}</td>
      <td>${t.total_sessions}</td>
      <td><strong>${Math.round(t.average_score)}</strong></td>
      <td>${avgMistakes.toFixed(1)}</td>
      <td><span class="status-pill ${adaptiveClass}">${adaptiveLabel}</span></td>
    `;
    tbody.appendChild(row);
  });
}

function renderScoreChart(labels, scores) {
  const ctx = document.getElementById('scoreChart').getContext('2d');
  if (scoreChartInstance) scoreChartInstance.destroy();

  scoreChartInstance = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: labels.length ? labels : ['Trainee A', 'Trainee B', 'Trainee C'],
      datasets: [{
        label: 'Average Score',
        data: scores.length ? scores : [85, 72, 90],
        backgroundColor: '#3b82f6',
        borderRadius: 4
      }]
    },
    options: {
      responsive: true,
      scales: {
        y: { beginAtZero: true, max: 100, grid: { color: '#334155' } },
        x: { grid: { display: false } }
      },
      plugins: { legend: { display: false } }
    }
  });
}

function renderMistakeChart(distribution) {
  const ctx = document.getElementById('mistakeChart').getContext('2d');
  if (mistakeChartInstance) mistakeChartInstance.destroy();

  mistakeChartInstance = new Chart(ctx, {
    type: 'doughnut',
    data: {
      labels: [
        'Wrong PPE Level (Level C/D instead of A)',
        'Premature Rescue without Leak Containment',
        'Hot Zone Boundary Breach without Hazard ID',
        'Time Limit Overrun'
      ],
      datasets: [{
        data: distribution,
        backgroundColor: ['#ef4444', '#f59e0b', '#3b82f6', '#8b5cf6']
      }]
    },
    options: {
      responsive: true,
      plugins: {
        legend: { position: 'bottom', labels: { color: '#f8fafc', font: { size: 11 } } }
      }
    }
  });
}

// Initial load on page ready
window.addEventListener('DOMContentLoaded', loadDashboardData);

