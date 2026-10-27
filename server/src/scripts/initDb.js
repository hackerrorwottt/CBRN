const fs = require('fs');
const path = require('path');
const mysql = require('mysql2/promise');
const dotenv = require('dotenv');

dotenv.config({ path: path.join(__dirname, '../../.env') });

async function initDatabase() {
  console.log('=======================================================');
  console.log(' CBRN-Trainer MySQL Database Auto-Initializer');
  console.log(' Problem Statement 269: Educational Training Prototype');
  console.log('=======================================================');

  const config = {
    host: process.env.DB_HOST || '127.0.0.1',
    user: process.env.DB_USER || 'root',
    password: process.env.DB_PASSWORD || '',
    port: parseInt(process.env.DB_PORT || '3306', 10),
    multipleStatements: true
  };

  let connection;

  try {
    console.log(`Connecting to MySQL host: ${config.host}:${config.port} as user: ${config.user}...`);
    connection = await mysql.createConnection(config);

    // 1. Create database
    console.log('Step 1: Creating database cbrn_trainer if not exists...');
    await connection.query('CREATE DATABASE IF NOT EXISTS cbrn_trainer CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;');
    await connection.query('USE cbrn_trainer;');

    // 2. Read and execute schema.sql
    console.log('Step 2: Executing schema.sql (3NF tables: users, scenarios, steps, sessions, step_logs)...');
    const schemaSql = fs.readFileSync(path.join(__dirname, '../../schema.sql'), 'utf-8');
    await connection.query(schemaSql);

    // 3. Read and execute seed.sql
    console.log('Step 3: Seeding default accounts (trainee, instructor, admin) and 7 chemical scenario steps...');
    const seedSql = fs.readFileSync(path.join(__dirname, '../../seed.sql'), 'utf-8');
    await connection.query(seedSql);

    console.log('=======================================================');
    console.log(' SUCCESS: CBRN-Trainer database initialized and seeded!');
    console.log(' Database Name: cbrn_trainer');
    console.log(' Default Trainee:    trainee@cbrn.gov.in / password123');
    console.log(' Default Instructor: instructor@cbrn.gov.in / password123');
    console.log(' Default Admin:      admin@cbrn.gov.in / password123');
    console.log('=======================================================');
  } catch (error) {
    console.error('ERROR initializing database:', error.message);
    process.exit(1);
  } finally {
    if (connection) {
      await connection.end();
    }
  }
}

initDatabase();
