-- HoloCall Database Initialization

-- Create users table
CREATE TABLE IF NOT EXISTS users (
    id VARCHAR(255) PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    avatar_url VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

-- Create rooms table
CREATE TABLE IF NOT EXISTS rooms (
    id VARCHAR(255) PRIMARY KEY,
    host_id VARCHAR(255) REFERENCES users(id),
    name VARCHAR(255),
    max_participants INTEGER DEFAULT 10,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    closed_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

-- Create room_participants table
CREATE TABLE IF NOT EXISTS room_participants (
    room_id VARCHAR(255) REFERENCES rooms(id),
    user_id VARCHAR(255) REFERENCES users(id),
    joined_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    left_at TIMESTAMP,
    platform VARCHAR(50),
    is_host BOOLEAN DEFAULT FALSE,
    PRIMARY KEY (room_id, user_id, joined_at)
);

-- Create sessions table (for analytics)
CREATE TABLE IF NOT EXISTS sessions (
    id SERIAL PRIMARY KEY,
    room_id VARCHAR(255) REFERENCES rooms(id),
    user_id VARCHAR(255) REFERENCES users(id),
    platform VARCHAR(50),
    duration_seconds INTEGER,
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP,
    avg_latency_ms INTEGER,
    packet_loss_percent DECIMAL(5, 2),
    quality_score DECIMAL(3, 2)
);

-- Create recordings table (optional)
CREATE TABLE IF NOT EXISTS recordings (
    id SERIAL PRIMARY KEY,
    room_id VARCHAR(255) REFERENCES rooms(id),
    user_id VARCHAR(255) REFERENCES users(id),
    file_path VARCHAR(500),
    file_size_bytes BIGINT,
    duration_seconds INTEGER,
    format VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_rooms_host ON rooms(host_id);
CREATE INDEX idx_rooms_active ON rooms(is_active);
CREATE INDEX idx_sessions_room ON sessions(room_id);
CREATE INDEX idx_sessions_user ON sessions(user_id);
CREATE INDEX idx_sessions_started ON sessions(started_at);

-- Insert sample data for testing
INSERT INTO users (id, email, display_name) VALUES
    ('user_test_1', 'alice@holocall.com', 'Alice'),
    ('user_test_2', 'bob@holocall.com', 'Bob'),
    ('user_test_3', 'carol@holocall.com', 'Carol')
ON CONFLICT (id) DO NOTHING;
