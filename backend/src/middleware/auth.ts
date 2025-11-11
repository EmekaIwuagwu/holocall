/**
 * Authentication middleware
 */

import jwt from 'jsonwebtoken';
import { Request, Response, NextFunction } from 'express';
import { JWTPayload } from '../types';

const JWT_SECRET = process.env.JWT_SECRET || 'your-secret-key-change-in-production';

export interface AuthRequest extends Request {
  user?: JWTPayload;
}

/**
 * Verify JWT token from Authorization header
 */
export const authenticateToken = (
  req: AuthRequest,
  res: Response,
  next: NextFunction
) => {
  const authHeader = req.headers['authorization'];
  const token = authHeader && authHeader.split(' ')[1]; // Bearer TOKEN

  if (!token) {
    return res.status(401).json({
      success: false,
      error: 'No token provided'
    });
  }

  try {
    const user = jwt.verify(token, JWT_SECRET) as JWTPayload;
    req.user = user;
    next();
  } catch (error) {
    return res.status(403).json({
      success: false,
      error: 'Invalid or expired token'
    });
  }
};

/**
 * Generate JWT token
 */
export const generateToken = (payload: Omit<JWTPayload, 'iat' | 'exp'>): string => {
  return jwt.sign(payload, JWT_SECRET, {
    expiresIn: process.env.JWT_EXPIRES_IN || '15m'
  });
};

/**
 * Generate refresh token
 */
export const generateRefreshToken = (payload: Omit<JWTPayload, 'iat' | 'exp'>): string => {
  const refreshSecret = process.env.JWT_REFRESH_SECRET || JWT_SECRET;
  return jwt.sign(payload, refreshSecret, {
    expiresIn: process.env.JWT_REFRESH_EXPIRES_IN || '7d'
  });
};

/**
 * Verify refresh token
 */
export const verifyRefreshToken = (token: string): JWTPayload | null => {
  try {
    const refreshSecret = process.env.JWT_REFRESH_SECRET || JWT_SECRET;
    return jwt.verify(token, refreshSecret) as JWTPayload;
  } catch (error) {
    return null;
  }
};

/**
 * Extract token from WebSocket upgrade request
 */
export const extractTokenFromWS = (url: string): string | null => {
  const urlParams = new URLSearchParams(url.split('?')[1]);
  return urlParams.get('token');
};

/**
 * Verify WebSocket token
 */
export const verifyWSToken = (token: string): JWTPayload | null => {
  try {
    return jwt.verify(token, JWT_SECRET) as JWTPayload;
  } catch (error) {
    return null;
  }
};
