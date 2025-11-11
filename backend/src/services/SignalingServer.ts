/**
 * Signaling Server - WebSocket server for WebRTC signaling
 */

import { WebSocket, WebSocketServer } from 'ws';
import { IncomingMessage } from 'http';
import { RoomManager } from './RoomManager';
import { TurnService } from './TurnService';
import { verifyWSToken, extractTokenFromWS } from '../middleware/auth';
import { Logger } from '../utils/Logger';
import {
  MessageType,
  CreateRoomMessage,
  JoinRoomMessage,
  LeaveRoomMessage,
  SDPMessage,
  ICECandidateMessage,
  AnchorSyncMessage,
  ParticipantInfo,
  RoomStateMessage,
  ErrorMessage,
  BaseMessage
} from '../types';

export class SignalingServer {
  private wss: WebSocketServer;
  private roomManager: RoomManager;
  private logger = Logger.getInstance();
  private clientToUserId: Map<WebSocket, string> = new Map();
  private userIdToClient: Map<string, WebSocket> = new Map();

  constructor(wss: WebSocketServer) {
    this.wss = wss;
    this.roomManager = new RoomManager();
    this.setupWebSocketServer();
    this.startCleanupInterval();
  }

  /**
   * Setup WebSocket server with authentication and message handling
   */
  private setupWebSocketServer() {
    this.wss.on('connection', (ws: WebSocket, req: IncomingMessage) => {
      // Extract and verify JWT token
      const token = extractTokenFromWS(req.url || '');

      if (!token) {
        this.logger.warn('Connection rejected: No token provided');
        ws.close(1008, 'No token provided');
        return;
      }

      const user = verifyWSToken(token);

      if (!user) {
        this.logger.warn('Connection rejected: Invalid token');
        ws.close(1008, 'Invalid token');
        return;
      }

      const userId = user.userId;
      this.logger.info(`WebSocket connection established: ${userId}`);

      // Store client mappings
      this.clientToUserId.set(ws, userId);
      this.userIdToClient.set(userId, ws);

      // Send welcome message
      this.sendMessage(ws, {
        type: MessageType.ROOM_JOINED,
        timestamp: Date.now()
      });

      // Setup message handler
      ws.on('message', (data: Buffer) => {
        this.handleMessage(ws, userId, data);
      });

      // Setup close handler
      ws.on('close', () => {
        this.handleDisconnect(userId);
      });

      // Setup error handler
      ws.on('error', (error) => {
        this.logger.error(`WebSocket error for user ${userId}:`, error);
      });

      // Setup ping/pong for connection health
      ws.on('pong', () => {
        (ws as any).isAlive = true;
      });
    });

    // Heartbeat interval to detect dead connections
    const interval = setInterval(() => {
      this.wss.clients.forEach((ws: WebSocket) => {
        if ((ws as any).isAlive === false) {
          const userId = this.clientToUserId.get(ws);
          if (userId) {
            this.logger.warn(`Terminating dead connection: ${userId}`);
            this.handleDisconnect(userId);
          }
          return ws.terminate();
        }

        (ws as any).isAlive = false;
        ws.ping();
      });
    }, 30000); // 30 seconds

    this.wss.on('close', () => {
      clearInterval(interval);
    });
  }

  /**
   * Handle incoming WebSocket messages
   */
  private handleMessage(ws: WebSocket, userId: string, data: Buffer) {
    try {
      const message = JSON.parse(data.toString()) as BaseMessage;
      message.timestamp = Date.now();

      this.logger.debug(`Message from ${userId}: ${message.type}`);

      switch (message.type) {
        case MessageType.CREATE_ROOM:
          this.handleCreateRoom(ws, userId, message as CreateRoomMessage);
          break;

        case MessageType.JOIN_ROOM:
          this.handleJoinRoom(ws, userId, message as JoinRoomMessage);
          break;

        case MessageType.LEAVE_ROOM:
          this.handleLeaveRoom(userId, message as LeaveRoomMessage);
          break;

        case MessageType.SDP_OFFER:
        case MessageType.SDP_ANSWER:
          this.handleSDPMessage(userId, message as SDPMessage);
          break;

        case MessageType.ICE_CANDIDATE:
          this.handleICECandidate(userId, message as ICECandidateMessage);
          break;

        case MessageType.ANCHOR_SYNC:
          this.handleAnchorSync(userId, message as AnchorSyncMessage);
          break;

        case MessageType.PING:
          this.sendMessage(ws, { type: MessageType.PONG, timestamp: Date.now() });
          break;

        default:
          this.logger.warn(`Unknown message type: ${message.type}`);
      }
    } catch (error) {
      this.logger.error('Error handling message:', error);
      this.sendError(ws, 'INVALID_MESSAGE', 'Failed to process message');
    }
  }

  /**
   * Handle room creation
   */
  private handleCreateRoom(ws: WebSocket, userId: string, message: CreateRoomMessage) {
    const roomId = this.roomManager.createRoom(
      userId,
      message.platform,
      message.maxParticipants
    );

    const participant: ParticipantInfo = {
      userId: userId,
      displayName: message.displayName,
      platform: message.platform,
      capabilities: message.capabilities,
      ws: ws,
      joinedAt: new Date(),
      isHost: true,
      isMuted: false,
      isCameraOff: false
    };

    this.roomManager.addParticipant(roomId, participant);

    // Get TURN credentials
    const turnCredentials = TurnService.generateCredentials(userId);

    // Send room created response
    this.sendMessage(ws, {
      type: MessageType.ROOM_CREATED,
      roomId: roomId,
      maxParticipants: message.maxParticipants,
      turnCredentials: turnCredentials,
      timestamp: Date.now()
    });

    this.logger.info(`Room created: ${roomId} by ${userId}`);
  }

  /**
   * Handle room joining
   */
  private handleJoinRoom(ws: WebSocket, userId: string, message: JoinRoomMessage) {
    const room = this.roomManager.getRoom(message.roomId);

    if (!room) {
      this.sendError(ws, 'ROOM_NOT_FOUND', 'Room not found');
      return;
    }

    if (room.participants.size >= room.maxParticipants) {
      this.sendError(ws, 'ROOM_FULL', 'Room is full');
      return;
    }

    const participant: ParticipantInfo = {
      userId: userId,
      displayName: message.displayName,
      platform: message.platform,
      capabilities: message.capabilities,
      ws: ws,
      joinedAt: new Date(),
      isHost: false,
      isMuted: false,
      isCameraOff: false
    };

    this.roomManager.addParticipant(message.roomId, participant);

    // Get TURN credentials
    const turnCredentials = TurnService.generateCredentials(userId);

    // Send room state to new participant
    const roomState: RoomStateMessage = {
      type: MessageType.ROOM_JOINED,
      roomId: message.roomId,
      participants: Array.from(room.participants.values())
        .filter(p => p.userId !== userId)
        .map(p => ({
          userId: p.userId,
          displayName: p.displayName,
          platform: p.platform,
          capabilities: p.capabilities,
          isHost: p.isHost,
          isMuted: p.isMuted,
          isCameraOff: p.isCameraOff
        })),
      turnCredentials: turnCredentials,
      timestamp: Date.now()
    };

    this.sendMessage(ws, roomState);

    // Notify existing participants about new participant
    this.broadcastToRoom(room.id, userId, {
      type: MessageType.PARTICIPANT_JOINED,
      participant: {
        userId: userId,
        displayName: message.displayName,
        platform: message.platform,
        capabilities: message.capabilities,
        isHost: false,
        isMuted: false,
        isCameraOff: false
      },
      timestamp: Date.now()
    });

    // Send communication mode to all participants
    this.negotiatePlatformCompatibility(room.id, userId);

    this.logger.info(`User ${userId} joined room ${message.roomId}`);
  }

  /**
   * Handle room leaving
   */
  private handleLeaveRoom(userId: string, message: LeaveRoomMessage) {
    const roomId = this.roomManager.removeParticipant(userId);

    if (roomId) {
      // Notify remaining participants
      this.broadcastToRoom(roomId, userId, {
        type: MessageType.PARTICIPANT_LEFT,
        userId: userId,
        timestamp: Date.now()
      });

      this.logger.info(`User ${userId} left room ${roomId}`);
    }
  }

  /**
   * Handle SDP offer/answer
   */
  private handleSDPMessage(fromUserId: string, message: SDPMessage) {
    const toClient = this.userIdToClient.get(message.toUserId);

    if (!toClient) {
      this.logger.warn(`Target user not found: ${message.toUserId}`);
      return;
    }

    // Forward SDP to target user
    this.sendMessage(toClient, {
      ...message,
      fromUserId: fromUserId
    });
  }

  /**
   * Handle ICE candidate
   */
  private handleICECandidate(fromUserId: string, message: ICECandidateMessage) {
    const toClient = this.userIdToClient.get(message.toUserId);

    if (!toClient) {
      this.logger.warn(`Target user not found: ${message.toUserId}`);
      return;
    }

    // Forward ICE candidate to target user
    this.sendMessage(toClient, {
      ...message,
      fromUserId: fromUserId
    });
  }

  /**
   * Handle AR anchor synchronization
   */
  private handleAnchorSync(userId: string, message: AnchorSyncMessage) {
    const room = this.roomManager.getRoomForUser(userId);

    if (!room) {
      return;
    }

    // Broadcast anchor update to all participants
    this.broadcastToRoom(room.id, userId, {
      type: MessageType.ANCHOR_UPDATE,
      fromUserId: userId,
      participantId: message.participantId,
      anchorData: message.anchorData,
      timestamp: Date.now()
    });
  }

  /**
   * Negotiate platform compatibility and communication modes
   */
  private negotiatePlatformCompatibility(roomId: string, newUserId: string) {
    const room = this.roomManager.getRoom(roomId);
    if (!room) return;

    const newParticipant = room.participants.get(newUserId);
    if (!newParticipant) return;

    // Determine communication mode with each existing participant
    room.participants.forEach((participant, participantId) => {
      if (participantId === newUserId) return;

      const mode = this.roomManager.determineCommunicationMode(
        newParticipant.platform,
        participant.platform
      );

      // Send mode to both participants
      this.sendMessage(newParticipant.ws, {
        type: MessageType.COMMUNICATION_MODE,
        withParticipant: participantId,
        mode: mode,
        timestamp: Date.now()
      });

      this.sendMessage(participant.ws, {
        type: MessageType.COMMUNICATION_MODE,
        withParticipant: newUserId,
        mode: mode,
        timestamp: Date.now()
      });
    });
  }

  /**
   * Broadcast message to all participants in a room except sender
   */
  private broadcastToRoom(roomId: string, excludeUserId: string, message: any) {
    const room = this.roomManager.getRoom(roomId);
    if (!room) return;

    room.participants.forEach((participant, participantId) => {
      if (participantId !== excludeUserId) {
        this.sendMessage(participant.ws, message);
      }
    });
  }

  /**
   * Handle client disconnect
   */
  private handleDisconnect(userId: string) {
    this.logger.info(`User disconnected: ${userId}`);

    const ws = this.userIdToClient.get(userId);
    if (ws) {
      this.clientToUserId.delete(ws);
      this.userIdToClient.delete(userId);
    }

    const roomId = this.roomManager.removeParticipant(userId);
    if (roomId) {
      this.broadcastToRoom(roomId, userId, {
        type: MessageType.PARTICIPANT_LEFT,
        userId: userId,
        timestamp: Date.now()
      });
    }
  }

  /**
   * Send message to client
   */
  private sendMessage(ws: WebSocket, message: any) {
    if (ws.readyState === WebSocket.OPEN) {
      ws.send(JSON.stringify(message));
    }
  }

  /**
   * Send error message
   */
  private sendError(ws: WebSocket, code: string, message: string) {
    const errorMessage: ErrorMessage = {
      type: MessageType.ERROR,
      code: code,
      message: message,
      timestamp: Date.now()
    };

    this.sendMessage(ws, errorMessage);
  }

  /**
   * Start cleanup interval for expired rooms
   */
  private startCleanupInterval() {
    const interval = parseInt(process.env.ROOM_CLEANUP_INTERVAL || '300000', 10);

    setInterval(() => {
      this.roomManager.cleanupExpiredRooms();
    }, interval);

    this.logger.info(`Room cleanup interval started: ${interval}ms`);
  }

  /**
   * Get room manager instance
   */
  getRoomManager(): RoomManager {
    return this.roomManager;
  }

  /**
   * Get statistics
   */
  getStats() {
    return {
      ...this.roomManager.getStats(),
      connectedClients: this.userIdToClient.size
    };
  }
}
