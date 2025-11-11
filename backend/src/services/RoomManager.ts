/**
 * Room Manager - Handles room creation, joining, and participant management
 */

import { v4 as uuidv4 } from 'uuid';
import { Room, ParticipantInfo, Platform, CommunicationMode } from '../types';
import { Logger } from '../utils/Logger';

export class RoomManager {
  private rooms: Map<string, Room> = new Map();
  private userToRoom: Map<string, string> = new Map();
  private logger = Logger.getInstance();

  /**
   * Create a new room
   * @param hostId Host user ID
   * @param platform Primary platform type
   * @param maxParticipants Maximum number of participants
   * @returns Room ID
   */
  createRoom(hostId: string, platform: Platform, maxParticipants?: number): string {
    const roomId = this.generateRoomId();

    const room: Room = {
      id: roomId,
      hostId: hostId,
      participants: new Map(),
      createdAt: new Date(),
      maxParticipants: maxParticipants || this.getDefaultMaxParticipants(platform),
      platform: platform,
      isRecording: false
    };

    this.rooms.set(roomId, room);
    this.logger.info(`Room created: ${roomId} by host: ${hostId}`);

    return roomId;
  }

  /**
   * Add participant to room
   * @param roomId Room ID
   * @param participant Participant information
   * @returns Success status
   */
  addParticipant(roomId: string, participant: ParticipantInfo): boolean {
    const room = this.rooms.get(roomId);

    if (!room) {
      this.logger.warn(`Room not found: ${roomId}`);
      return false;
    }

    if (room.participants.size >= room.maxParticipants) {
      this.logger.warn(`Room full: ${roomId}`);
      return false;
    }

    // Set host flag for first participant
    if (room.participants.size === 0) {
      participant.isHost = true;
      room.hostId = participant.userId;
    }

    room.participants.set(participant.userId, participant);
    this.userToRoom.set(participant.userId, roomId);

    this.logger.info(`Participant ${participant.userId} joined room ${roomId}`);
    return true;
  }

  /**
   * Remove participant from room
   * @param userId User ID
   * @returns Room ID if user was in a room, null otherwise
   */
  removeParticipant(userId: string): string | null {
    const roomId = this.userToRoom.get(userId);

    if (!roomId) {
      return null;
    }

    const room = this.rooms.get(roomId);
    if (!room) {
      return null;
    }

    const participant = room.participants.get(userId);
    room.participants.delete(userId);
    this.userToRoom.delete(userId);

    this.logger.info(`Participant ${userId} left room ${roomId}`);

    // If room is empty, delete it
    if (room.participants.size === 0) {
      this.rooms.delete(roomId);
      this.logger.info(`Room ${roomId} deleted (empty)`);
    }
    // If host left, assign new host
    else if (participant?.isHost) {
      const newHost = Array.from(room.participants.values())[0];
      newHost.isHost = true;
      room.hostId = newHost.userId;
      this.logger.info(`New host assigned in room ${roomId}: ${newHost.userId}`);
    }

    return roomId;
  }

  /**
   * Get room by ID
   * @param roomId Room ID
   * @returns Room or undefined
   */
  getRoom(roomId: string): Room | undefined {
    return this.rooms.get(roomId);
  }

  /**
   * Get room for user
   * @param userId User ID
   * @returns Room or undefined
   */
  getRoomForUser(userId: string): Room | undefined {
    const roomId = this.userToRoom.get(userId);
    return roomId ? this.rooms.get(roomId) : undefined;
  }

  /**
   * Get all participants in a room
   * @param roomId Room ID
   * @returns Array of participants
   */
  getParticipants(roomId: string): ParticipantInfo[] {
    const room = this.rooms.get(roomId);
    return room ? Array.from(room.participants.values()) : [];
  }

  /**
   * Get participant by user ID
   * @param userId User ID
   * @returns Participant info or undefined
   */
  getParticipant(userId: string): ParticipantInfo | undefined {
    const room = this.getRoomForUser(userId);
    return room?.participants.get(userId);
  }

  /**
   * Update participant status
   * @param userId User ID
   * @param updates Partial participant updates
   * @returns Success status
   */
  updateParticipant(userId: string, updates: Partial<ParticipantInfo>): boolean {
    const room = this.getRoomForUser(userId);
    const participant = room?.participants.get(userId);

    if (!participant) {
      return false;
    }

    Object.assign(participant, updates);
    return true;
  }

  /**
   * Determine optimal communication mode between two participants
   * @param platform1 First participant platform
   * @param platform2 Second participant platform
   * @returns Communication mode
   */
  determineCommunicationMode(platform1: Platform, platform2: Platform): CommunicationMode {
    // Desktop ↔ Desktop: Volumetric point cloud
    if (platform1 === 'desktop' && platform2 === 'desktop') {
      return 'volumetric';
    }

    // Mobile ↔ Mobile or Mobile ↔ VR: Avatar
    if ((platform1 === 'android' || platform1 === 'ios') &&
        (platform2 === 'android' || platform2 === 'ios' || platform2 === 'vr')) {
      return 'avatar';
    }

    // Desktop ↔ Mobile: Desktop sends volumetric, Mobile sends avatar
    if ((platform1 === 'desktop' && platform2 !== 'desktop') ||
        (platform2 === 'desktop' && platform1 !== 'desktop')) {
      return 'hybrid';
    }

    // VR ↔ VR: Avatar with hand tracking
    if (platform1 === 'vr' && platform2 === 'vr') {
      return 'avatar_vr';
    }

    return 'avatar'; // Safe default
  }

  /**
   * Get default max participants based on platform
   * @param platform Platform type
   * @returns Max participants
   */
  private getDefaultMaxParticipants(platform: Platform): number {
    const limits: Record<Platform, number> = {
      desktop: 10,
      vr: 8,
      android: 4,
      ios: 6
    };

    return limits[platform] || 6;
  }

  /**
   * Generate unique room ID
   * @returns Room ID (8 character alphanumeric)
   */
  private generateRoomId(): string {
    // Generate short, user-friendly room codes
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let roomId = '';

    do {
      roomId = '';
      for (let i = 0; i < 8; i++) {
        roomId += chars.charAt(Math.floor(Math.random() * chars.length));
      }
    } while (this.rooms.has(roomId));

    return roomId;
  }

  /**
   * Get statistics
   * @returns Room statistics
   */
  getStats() {
    return {
      totalRooms: this.rooms.size,
      totalParticipants: this.userToRoom.size,
      rooms: Array.from(this.rooms.values()).map(room => ({
        id: room.id,
        participantCount: room.participants.size,
        maxParticipants: room.maxParticipants,
        platform: room.platform,
        createdAt: room.createdAt,
        isRecording: room.isRecording
      }))
    };
  }

  /**
   * Clean up expired rooms
   * @param maxAge Maximum room age in milliseconds
   */
  cleanupExpiredRooms(maxAge: number = 24 * 60 * 60 * 1000) {
    const now = Date.now();
    let cleaned = 0;

    for (const [roomId, room] of this.rooms.entries()) {
      const age = now - room.createdAt.getTime();

      if (age > maxAge && room.participants.size === 0) {
        this.rooms.delete(roomId);
        cleaned++;
      }
    }

    if (cleaned > 0) {
      this.logger.info(`Cleaned up ${cleaned} expired rooms`);
    }

    return cleaned;
  }
}
