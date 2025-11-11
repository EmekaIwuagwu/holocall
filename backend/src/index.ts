/**
 * HoloCall Backend - Main entry point
 */

import express from 'express';
import { createServer } from 'http';
import { WebSocketServer } from 'ws';
import cors from 'cors';
import helmet from 'helmet';
import compression from 'compression';
import dotenv from 'dotenv';
import { SignalingServer } from './services/SignalingServer';
import { TurnService } from './services/TurnService';
import { authenticateToken, generateToken, generateRefreshToken, verifyRefreshToken } from './middleware/auth';
import { Logger } from './utils/Logger';

// Load environment variables
dotenv.config();

const app = express();
const logger = Logger.getInstance();
const PORT = parseInt(process.env.PORT || '8080', 10);
const HOST = process.env.HOST || '0.0.0.0';

// Middleware
app.use(helmet());
app.use(cors({
  origin: process.env.CORS_ORIGIN || '*',
  credentials: true
}));
app.use(compression());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Request logging
app.use((req, res, next) => {
  logger.info(`${req.method} ${req.path}`);
  next();
});

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({
    status: 'ok',
    timestamp: new Date().toISOString(),
    uptime: process.uptime()
  });
});

// Authentication endpoints
app.post('/api/auth/login', (req, res) => {
  const { email, displayName } = req.body;

  if (!email || !displayName) {
    return res.status(400).json({
      success: false,
      error: 'Email and displayName are required'
    });
  }

  // In production, validate credentials against database
  // For now, generate token for any valid request
  const userId = `user_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

  const token = generateToken({ userId, email, displayName });
  const refreshToken = generateRefreshToken({ userId, email, displayName });

  res.json({
    success: true,
    data: {
      token,
      refreshToken,
      user: {
        userId,
        email,
        displayName
      }
    }
  });
});

app.post('/api/auth/refresh', (req, res) => {
  const { refreshToken } = req.body;

  if (!refreshToken) {
    return res.status(400).json({
      success: false,
      error: 'Refresh token is required'
    });
  }

  const user = verifyRefreshToken(refreshToken);

  if (!user) {
    return res.status(403).json({
      success: false,
      error: 'Invalid refresh token'
    });
  }

  const newToken = generateToken({
    userId: user.userId,
    email: user.email,
    displayName: user.displayName
  });

  res.json({
    success: true,
    data: {
      token: newToken
    }
  });
});

// TURN credentials endpoint
app.get('/api/turn/credentials', authenticateToken, (req, res) => {
  const user = (req as any).user;
  const credentials = TurnService.generateCredentials(user.userId);

  res.json({
    success: true,
    data: credentials
  });
});

// ICE servers endpoint
app.get('/api/ice/servers', authenticateToken, (req, res) => {
  const user = (req as any).user;
  const iceServers = TurnService.getIceServers(user.userId);

  res.json({
    success: true,
    data: {
      iceServers
    }
  });
});

// Statistics endpoint (protected)
app.get('/api/stats', authenticateToken, (req, res) => {
  const stats = signalingServer?.getStats();

  res.json({
    success: true,
    data: stats
  });
});

// 404 handler
app.use((req, res) => {
  res.status(404).json({
    success: false,
    error: 'Endpoint not found'
  });
});

// Error handler
app.use((err: any, req: express.Request, res: express.Response, next: express.NextFunction) => {
  logger.error('Express error:', err);

  res.status(500).json({
    success: false,
    error: 'Internal server error'
  });
});

// Create HTTP server
const server = createServer(app);

// Create WebSocket server
const wss = new WebSocketServer({
  server,
  path: process.env.WS_PATH || '/ws'
});

// Create signaling server
let signalingServer: SignalingServer;

try {
  signalingServer = new SignalingServer(wss);
  logger.info('Signaling server initialized');
} catch (error) {
  logger.error('Failed to initialize signaling server:', error);
  process.exit(1);
}

// Start server
server.listen(PORT, HOST, () => {
  logger.info(`🚀 HoloCall Backend running on ${HOST}:${PORT}`);
  logger.info(`📡 WebSocket server listening on ws://${HOST}:${PORT}${process.env.WS_PATH || '/ws'}`);
  logger.info(`🌍 Environment: ${process.env.NODE_ENV || 'development'}`);
});

// Graceful shutdown
const shutdown = () => {
  logger.info('Shutting down gracefully...');

  server.close(() => {
    logger.info('HTTP server closed');
    process.exit(0);
  });

  // Force shutdown after 10 seconds
  setTimeout(() => {
    logger.error('Forced shutdown after timeout');
    process.exit(1);
  }, 10000);
};

process.on('SIGTERM', shutdown);
process.on('SIGINT', shutdown);

// Handle uncaught errors
process.on('uncaughtException', (error) => {
  logger.error('Uncaught exception:', error);
  process.exit(1);
});

process.on('unhandledRejection', (reason, promise) => {
  logger.error('Unhandled rejection at:', promise, 'reason:', reason);
  process.exit(1);
});
