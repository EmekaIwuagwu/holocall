/**
 * Logger utility using Winston
 */

import winston from 'winston';

export class Logger {
  private static instance: winston.Logger;

  private constructor() {}

  static getInstance(): winston.Logger {
    if (!Logger.instance) {
      Logger.instance = winston.createLogger({
        level: process.env.LOG_LEVEL || 'info',
        format: winston.format.combine(
          winston.format.timestamp({
            format: 'YYYY-MM-DD HH:mm:ss'
          }),
          winston.format.errors({ stack: true }),
          winston.format.splat(),
          winston.format.json()
        ),
        defaultMeta: { service: 'holocall-backend' },
        transports: [
          new winston.transports.File({
            filename: 'logs/error.log',
            level: 'error',
            maxsize: 5242880, // 5MB
            maxFiles: 5
          }),
          new winston.transports.File({
            filename: 'logs/combined.log',
            maxsize: 5242880,
            maxFiles: 5
          })
        ]
      });

      // If not production, also log to console
      if (process.env.NODE_ENV !== 'production') {
        Logger.instance.add(
          new winston.transports.Console({
            format: winston.format.combine(
              winston.format.colorize(),
              winston.format.simple()
            )
          })
        );
      }
    }

    return Logger.instance;
  }
}
