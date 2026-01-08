CREATE TABLE IF NOT EXISTS trading_events (
    event_id VARCHAR(36) PRIMARY KEY,
    timestamp TIMESTAMP NOT NULL,
    symbol VARCHAR(20) NOT NULL,
    order_type VARCHAR(20) NOT NULL,
    side VARCHAR(20) NOT NULL,
    quantity DECIMAL(18,8) NOT NULL,
    price DECIMAL(18,8) NOT NULL,
    user_id VARCHAR(50) NOT NULL,
    order_id VARCHAR(36) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_trading_events_symbol ON trading_events(symbol);
CREATE INDEX idx_trading_events_timestamp ON trading_events(timestamp);
CREATE INDEX idx_trading_events_user_id ON trading_events(user_id);
CREATE INDEX idx_trading_events_order_id ON trading_events(order_id);

-- Order Executions Table
CREATE TABLE IF NOT EXISTS order_executions (
    event_id VARCHAR(36) PRIMARY KEY,
    timestamp TIMESTAMP NOT NULL,
    order_id VARCHAR(36) NOT NULL,
    symbol VARCHAR(20) NOT NULL,
    executed_quantity DECIMAL(18,8) NOT NULL,
    executed_price DECIMAL(18,8) NOT NULL,
    remaining_quantity DECIMAL(18,8) NOT NULL,
    status VARCHAR(20) NOT NULL,
    execution_id VARCHAR(36) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_order_executions_order_id ON order_executions(order_id);
CREATE INDEX idx_order_executions_symbol ON order_executions(symbol);
CREATE INDEX idx_order_executions_timestamp ON order_executions(timestamp);

-- Market Data Table
CREATE TABLE IF NOT EXISTS market_data (
    event_id VARCHAR(36) PRIMARY KEY,
    timestamp TIMESTAMP NOT NULL,
    symbol VARCHAR(20) NOT NULL,
    bid_price DECIMAL(18,8) NOT NULL,
    ask_price DECIMAL(18,8) NOT NULL,
    last_price DECIMAL(18,8) NOT NULL,
    volume DECIMAL(18,8) NOT NULL,
    bid_size DECIMAL(18,8) NOT NULL,
    ask_size DECIMAL(18,8) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_market_data_symbol ON market_data(symbol);
CREATE INDEX idx_market_data_timestamp ON market_data(timestamp);

-- Trading Analytics Table
CREATE TABLE IF NOT EXISTS trading_analytics (
    id SERIAL PRIMARY KEY,
    symbol VARCHAR(20) NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    total_volume DECIMAL(20,2) NOT NULL,
    trade_count INT NOT NULL,
    avg_trade_size DECIMAL(20,2) NOT NULL,
    buy_volume DECIMAL(20,2) NOT NULL,
    sell_volume DECIMAL(20,2) NOT NULL,
    vwap DECIMAL(18,8) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_trading_analytics_symbol ON trading_analytics(symbol);
CREATE INDEX idx_trading_analytics_timestamp ON trading_analytics(timestamp);

-- Dashboard View
CREATE OR REPLACE VIEW trading_dashboard AS
SELECT 
    te.symbol,
    COUNT(DISTINCT te.event_id) as total_trades,
    SUM(te.quantity * te.price) as total_volume,
    AVG(te.price) as avg_price,
    MAX(te.timestamp) as last_trade_time,
    SUM(CASE WHEN te.side = 'Buy' THEN te.quantity * te.price ELSE 0 END) as buy_volume,
    SUM(CASE WHEN te.side = 'Sell' THEN te.quantity * te.price ELSE 0 END) as sell_volume
FROM trading_events te
WHERE te.timestamp >= NOW() - INTERVAL '1 hour'
GROUP BY te.symbol
ORDER BY total_volume DESC;

-- Grant permissions
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO trading_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO trading_user;