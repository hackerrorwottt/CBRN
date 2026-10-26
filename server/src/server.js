const app = require('./app');
const dotenv = require('dotenv');

dotenv.config();

const PORT = process.env.PORT || 3000;

app.listen(PORT, '0.0.0.0', () => {
  console.log(`=======================================================`);
  console.log(` CBRN-Trainer API Server listening on port ${PORT}`);
  console.log(` Local:   http://localhost:${PORT}`);
  console.log(` Health:  http://localhost:${PORT}/health`);
  console.log(` Target:  Problem Statement 269 (NDRF VR Training)`);
  console.log(`=======================================================`);
});

