/**
 * Type definitions for HoloCall backend
 */

import { WebSocket } from 'ws';

// Platform types
export type Platform = 'desktop' | 'android' | 'ios' | 'vr';
export type CommunicationMode = 'volumetric' | 'avatar' | 'hybrid' | 'avatar_vr';

// User and authentication
export interface User {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  createdAt: Date;
  lastSeen: Date;
}

export interface JWTPayload {
  userId: string;
  email: string;
  displayName: string;
  iat?: number;
  exp?: number;
}

// Platform capabilities
export interface PlatformCapabilities {
  volumetric: boolean;
  avatar: boolean;
  ar: boolean;
  vr: boolean;
  screenShare: boolean;
  faceTracking: boolean;
  handTracking: boolean;
  spatialAudio: boolean;
}

// Participant information
export interface ParticipantInfo {
  userId: string;
  displayName: string;
  platform: Platform;
  capabilities: PlatformCapabilities;
  ws: WebSocket;
  joinedAt: Date;
  isHost: boolean;
  isMuted: boolean;
  isCameraOff: boolean;
}

// Room information
export interface Room {
  id: string;
  hostId: string;
  participants: Map<string, ParticipantInfo>;
  createdAt: Date;
  maxParticipants: number;
  platform: Platform | 'mixed';
  isRecording: boolean;
  metadata?: Record<string, any>;
}

// WebSocket message types
export enum MessageType {
  // Room management
  CREATE_ROOM = 'create_room',
  JOIN_ROOM = 'join_room',
  LEAVE_ROOM = 'leave_room',
  ROOM_CREATED = 'room_created',
  ROOM_JOINED = 'joined_room',
  ROOM_LEFT = 'left_room',

  // Participant events
  PARTICIPANT_JOINED = 'participant_joined',
  PARTICIPANT_LEFT = 'participant_left',
  PARTICIPANT_UPDATED = 'participant_updated',

  // WebRTC signaling
  SDP_OFFER = 'sdp_offer',
  SDP_ANSWER = 'sdp_answer',
  ICE_CANDIDATE = 'ice_candidate',

  // Platform negotiation
  PLATFORM_CAPABILITIES = 'platform_capabilities',
  COMMUNICATION_MODE = 'communication_mode',

  // AR features
  ANCHOR_SYNC = 'anchor_sync',
  ANCHOR_UPDATE = 'anchor_update',

  // Utility
  PING = 'ping',
  PONG = 'pong',
  ERROR = 'error',
  HEARTBEAT = 'heartbeat'
}

// Base message interface
export interface BaseMessage {
  type: MessageType;
  timestamp?: number;
}

// Room messages
export interface CreateRoomMessage extends BaseMessage {
  type: MessageType.CREATE_ROOM;
  platform: Platform;
  maxParticipants?: number;
  displayName: string;
  capabilities: PlatformCapabilities;
}

export interface JoinRoomMessage extends BaseMessage {
  type: MessageType.JOIN_ROOM;
  roomId: string;
  displayName: string;
  platform: Platform;
  capabilities: PlatformCapabilities;
}

export interface LeaveRoomMessage extends BaseMessage {
  type: MessageType.LEAVE_ROOM;
  roomId: string;
}

// Signaling messages
export interface SDPMessage extends BaseMessage {
  type: MessageType.SDP_OFFER | MessageType.SDP_ANSWER;
  toUserId: string;
  fromUserId?: string;
  sdp: string;
}

export interface ICECandidateMessage extends BaseMessage {
  type: MessageType.ICE_CANDIDATE;
  toUserId: string;
  fromUserId?: string;
  candidate: any;
}

// AR messages
export interface AnchorSyncMessage extends BaseMessage {
  type: MessageType.ANCHOR_SYNC;
  participantId: string;
  anchorData: {
    position: [number, number, number];
    rotation: [number, number, number, number];
    cloudAnchorId?: string;
  };
}

// Platform negotiation
export interface CommunicationModeMessage extends BaseMessage {
  type: MessageType.COMMUNICATION_MODE;
  withParticipant: string;
  mode: CommunicationMode;
}

// Error message
export interface ErrorMessage extends BaseMessage {
  type: MessageType.ERROR;
  code: string;
  message: string;
}

// TURN credentials
export interface TURNCredentials {
  username: string;
  password: string;
  ttl: number;
  uris: string[];
}

// Room state for client
export interface RoomStateMessage extends BaseMessage {
  type: MessageType.ROOM_JOINED;
  roomId: string;
  participants: Array<{
    userId: string;
    displayName: string;
    platform: Platform;
    capabilities: PlatformCapabilities;
    isHost: boolean;
    isMuted: boolean;
    isCameraOff: boolean;
  }>;
  turnCredentials: TURNCredentials;
}

// Mediasoup types
export interface MediasoupTransportOptions {
  id: string;
  iceParameters: any;
  iceCandidates: any[];
  dtlsParameters: any;
}

// Analytics event
export interface AnalyticsEvent {
  eventType: string;
  userId?: string;
  roomId?: string;
  platform?: Platform;
  metadata?: Record<string, any>;
  timestamp: Date;
}

// Service responses
export interface ServiceResponse<T = any> {
  success: boolean;
  data?: T;
  error?: string;
  code?: string;
}
