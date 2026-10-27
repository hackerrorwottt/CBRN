const app = require('./app');
const db = require('./config/db');
const dotenv = require('dotenv');

dotenv.config();

const PORT = process.env.PORT || 3000;

const server = app.listen(PORT, '0.0.0.0', () => {
  console.log(`=======================================================`);
  console.log(` CBRN-Trainer API Server listening on port ${PORT}`);
  console.log(` Local:   http://localhost:${PORT}`);
  console.log(` Health:  http://localhost:${PORT}/health`);
  console.log(` Target:  Problem Statement 269 (NDRF VR Training)`);
  console.log(`=======================================================`);
});

// Graceful shutdown for clean MySQL connection pool release
function gracefulShutdown(signal) {
  console.log(`\n[${signal}] Shutting down CBRN-Trainer API Server gracefully...`);
  server.close(async () => {
    try {
      await db.end();
      console.log('MySQL connection pool closed cleanly.');
      process.exit(0);
    } catch (err) {
      console.error('Error closing MySQL pool:', err);
      process.exit(1);
    }
  });
}

process.on('SIGINT', () => gracefulShutdown('SIGINT'));
process.on('SIGTERM', () => gracefulShutdown('SIGTERM'));
