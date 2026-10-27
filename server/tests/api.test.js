const request = require('supertest');
const app = require('../src/app');

describe('CBRN-Trainer REST API Smoke & Validation Tests', () => {
  describe('GET /health', () => {
    it('should return 200 OK with status HEALTHY', async () => {
      const res = await request(app).get('/health');
      expect(res.statusCode).toBe(200);
      expect(res.body.status).toBe('HEALTHY');
      expect(res.body.service).toContain('CBRN-Trainer');
    });
  });

  describe('POST /api/auth/register validation', () => {
    it('should reject registration requests with missing fields (400)', async () => {
      const res = await request(app)
        .post('/api/auth/register')
        .send({ username: 'trainee_test' }); // Missing email and password

      expect(res.statusCode).toBe(400);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('VALIDATION_ERROR');
    });

    it('should reject registration with malformed email address (400)', async () => {
      const res = await request(app)
        .post('/api/auth/register')
        .send({ username: 'trainee_test', email: 'not-an-email', password: 'password123' });

      expect(res.statusCode).toBe(400);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('VALIDATION_ERROR');
    });

    it('should reject registration with password shorter than 6 characters (400)', async () => {
      const res = await request(app)
        .post('/api/auth/register')
        .send({ username: 'trainee_test', email: 'trainee@ndrf.gov.in', password: '123' });

      expect(res.statusCode).toBe(400);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('VALIDATION_ERROR');
    });
  });

  describe('POST /api/auth/login validation', () => {
    it('should reject login requests with missing credentials (400)', async () => {
      const res = await request(app)
        .post('/api/auth/login')
        .send({});

      expect(res.statusCode).toBe(400);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('VALIDATION_ERROR');
    });
  });

  describe('POST /api/sessions validation', () => {
    it('should reject session uploads without required fields (400)', async () => {
      const res = await request(app)
        .post('/api/sessions')
        .send({ score: 95 }); // Missing user_id, scenario_id, total_duration_seconds

      expect(res.statusCode).toBe(400);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('VALIDATION_ERROR');
    });

    it('should reject session uploads when score exceeds 0-100 rubric range (400)', async () => {
      const res = await request(app)
        .post('/api/sessions')
        .send({ user_id: 1, scenario_id: 1, score: 150, total_duration_seconds: 120 });

      expect(res.statusCode).toBe(400);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('VALIDATION_ERROR');
    });

    it('should reject session uploads with negative duration (400)', async () => {
      const res = await request(app)
        .post('/api/sessions')
        .send({ user_id: 1, scenario_id: 1, score: 85, total_duration_seconds: -5 });

      expect(res.statusCode).toBe(400);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('VALIDATION_ERROR');
    });
  });

  describe('GET /api/scenarios/:id/steps validation', () => {
    it('should return 400 Bad Request when scenario ID is not an integer', async () => {
      const res = await request(app).get('/api/scenarios/invalid-id/steps');
      expect(res.statusCode).toBe(400);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('INVALID_ID');
    });
  });

  describe('GET /api/sessions/user/:userId validation', () => {
    it('should return 400 Bad Request when user ID is not an integer', async () => {
      const res = await request(app).get('/api/sessions/user/abc');
      expect(res.statusCode).toBe(400);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('INVALID_ID');
    });
  });

  describe('GET /api/sessions/adaptive-difficulty/:userId validation', () => {
    it('should return 400 Bad Request when adaptive difficulty user ID is invalid', async () => {
      const res = await request(app).get('/api/sessions/adaptive-difficulty/notanumber');
      expect(res.statusCode).toBe(400);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('INVALID_ID');
    });
  });

  describe('404 Fallback Handler', () => {
    it('should return standard JSON error format for non-existent routes', async () => {
      const res = await request(app).get('/api/non-existent-endpoint');
      expect(res.statusCode).toBe(404);
      expect(res.body.success).toBe(false);
      expect(res.body.error).toBe('NOT_FOUND');
    });
  });
});

