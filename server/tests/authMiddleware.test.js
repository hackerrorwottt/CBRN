const jwt = require('jsonwebtoken');
const { verifyToken, requireRole, JWT_SECRET } = require('../src/middleware/auth');

function createMockRes() {
  const res = {};
  res.status = jest.fn().mockReturnValue(res);
  res.json = jest.fn().mockReturnValue(res);
  return res;
}

describe('Auth Middleware Unit Tests', () => {
  describe('verifyToken', () => {
    it('rejects requests without an Authorization header (401)', () => {
      const req = { headers: {} };
      const res = createMockRes();
      const next = jest.fn();

      verifyToken(req, res, next);

      expect(next).not.toHaveBeenCalled();
      expect(res.status).toHaveBeenCalledWith(401);
      expect(res.json).toHaveBeenCalledWith(expect.objectContaining({
        success: false,
        error: 'UNAUTHORIZED'
      }));
    });

    it('rejects malformed Authorization headers not using Bearer scheme (401)', () => {
      const req = { headers: { authorization: 'Basic abc123' } };
      const res = createMockRes();
      const next = jest.fn();

      verifyToken(req, res, next);

      expect(next).not.toHaveBeenCalled();
      expect(res.status).toHaveBeenCalledWith(401);
      expect(res.json).toHaveBeenCalledWith(expect.objectContaining({
        success: false,
        error: 'UNAUTHORIZED'
      }));
    });

    it('rejects forged or expired JWT tokens (401 TOKEN_INVALID)', () => {
      const req = { headers: { authorization: 'Bearer invalid.jwt.token' } };
      const res = createMockRes();
      const next = jest.fn();

      verifyToken(req, res, next);

      expect(next).not.toHaveBeenCalled();
      expect(res.status).toHaveBeenCalledWith(401);
      expect(res.json).toHaveBeenCalledWith(expect.objectContaining({
        success: false,
        error: 'TOKEN_INVALID'
      }));
    });

    it('populates req.user and calls next() for a valid JWT token', () => {
      const token = jwt.sign(
        { id: 2, username: 'instructor1', email: 'instructor1@ndrf.gov.in', role: 'instructor' },
        JWT_SECRET,
        { expiresIn: '1h' }
      );
      const req = { headers: { authorization: `Bearer ${token}` } };
      const res = createMockRes();
      const next = jest.fn();

      verifyToken(req, res, next);

      expect(next).toHaveBeenCalledTimes(1);
      expect(req.user).toEqual(expect.objectContaining({
        id: 2,
        username: 'instructor1',
        role: 'instructor'
      }));
    });
  });

  describe('requireRole', () => {
    it('returns 401 when req.user is absent', () => {
      const middleware = requireRole('instructor', 'admin');
      const req = {};
      const res = createMockRes();
      const next = jest.fn();

      middleware(req, res, next);

      expect(next).not.toHaveBeenCalled();
      expect(res.status).toHaveBeenCalledWith(401);
      expect(res.json).toHaveBeenCalledWith(expect.objectContaining({
        success: false,
        error: 'UNAUTHORIZED'
      }));
    });

    it('returns 403 FORBIDDEN when user role is insufficient', () => {
      const middleware = requireRole('instructor', 'admin');
      const req = { user: { id: 1, role: 'trainee' } };
      const res = createMockRes();
      const next = jest.fn();

      middleware(req, res, next);

      expect(next).not.toHaveBeenCalled();
      expect(res.status).toHaveBeenCalledWith(403);
      expect(res.json).toHaveBeenCalledWith(expect.objectContaining({
        success: false,
        error: 'FORBIDDEN'
      }));
    });

    it('calls next() when user has an authorized role', () => {
      const middleware = requireRole('instructor', 'admin');
      const req = { user: { id: 2, role: 'instructor' } };
      const res = createMockRes();
      const next = jest.fn();

      middleware(req, res, next);

      expect(next).toHaveBeenCalledTimes(1);
    });
  });
});
