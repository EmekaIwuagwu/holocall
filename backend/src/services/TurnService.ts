/**
 * TURN Service - Generates dynamic TURN/STUN credentials
 */

import crypto from 'crypto';
import { TURNCredentials } from '../types';

export class TurnService {
  private static secret: string = process.env.TURN_SECRET || 'default-secret-change-me';
  private static turnUrl: string = process.env.TURN_SERVER_URL || 'turn:turn.holocall.com:3478';
  private static turnsUrl: string = process.env.TURNS_SERVER_URL || 'turns:turn.holocall.com:5349';
  private static stunUrl: string = process.env.STUN_SERVER_URL || 'stun:stun.l.google.com:19302';

  /**
   * Generate time-limited TURN credentials using the REST API authentication mechanism
   * @param username User identifier
   * @param ttl Time to live in seconds (default: 24 hours)
   * @returns TURN credentials object
   */
  static generateCredentials(username: string, ttl: number = 86400): TURNCredentials {
    const timestamp = Math.floor(Date.now() / 1000) + ttl;
    const turnUsername = `${timestamp}:${username}`;

    // Generate HMAC-SHA1 password
    const hmac = crypto.createHmac('sha1', this.secret);
    hmac.update(turnUsername);
    const turnPassword = hmac.digest('base64');

    return {
      username: turnUsername,
      password: turnPassword,
      ttl: ttl,
      uris: [
        this.stunUrl,
        `${this.turnUrl}?transport=udp`,
        `${this.turnUrl}?transport=tcp`,
        `${this.turnsUrl}?transport=tcp`
      ]
    };
  }

  /**
   * Validate TURN credentials
   * @param username TURN username with timestamp
   * @param password TURN password
   * @returns true if valid, false otherwise
   */
  static validateCredentials(username: string, password: string): boolean {
    try {
      const [timestampStr] = username.split(':');
      const timestamp = parseInt(timestampStr, 10);
      const now = Math.floor(Date.now() / 1000);

      // Check if credentials are expired
      if (timestamp < now) {
        return false;
      }

      // Regenerate password and compare
      const hmac = crypto.createHmac('sha1', this.secret);
      hmac.update(username);
      const expectedPassword = hmac.digest('base64');

      return password === expectedPassword;
    } catch (error) {
      return false;
    }
  }

  /**
   * Get ICE server configuration for WebRTC
   * @param username User identifier
   * @returns RTCIceServer configuration array
   */
  static getIceServers(username: string) {
    const creds = this.generateCredentials(username);

    return [
      {
        urls: this.stunUrl
      },
      {
        urls: [
          `${this.turnUrl}?transport=udp`,
          `${this.turnUrl}?transport=tcp`,
          `${this.turnsUrl}?transport=tcp`
        ],
        username: creds.username,
        credential: creds.password
      }
    ];
  }
}
