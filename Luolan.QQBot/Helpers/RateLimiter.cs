using System.Collections.Concurrent;

namespace Luolan.QQBot.Helpers;

/// <summary>
/// 速率限制器 - 使用 Token Bucket 算法
/// </summary>
public class RateLimiter
{
    private readonly int _capacity;
    private readonly int _refillRate;
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
    
    public RateLimiter(int capacity = 60, int refillRate = 1)
    {
        _capacity = capacity;
        _refillRate = refillRate;
    }

    /// <summary>
    /// 尝试获取令牌 (非阻塞)
    /// </summary>
    public bool TryAcquire(string key = "default")
    {
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(_capacity, _refillRate));
        return bucket.TryConsume();
    }

    /// <summary>
    /// 等待直到可以获取令牌 (阻塞)
    /// </summary>
    public async Task AcquireAsync(string key = "default", CancellationToken cancellationToken = default)
    {
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(_capacity, _refillRate));
        
        while (!bucket.TryConsume())
        {
            await Task.Delay(100, cancellationToken);
        }
    }

    private class TokenBucket
    {
        private readonly int _capacity;
        private readonly int _refillRate;
        private readonly object _lock = new();
        private double _tokens;
        private DateTime _lastRefill;

        public TokenBucket(int capacity, int refillRate)
        {
            _capacity = capacity;
            _refillRate = refillRate;
            _tokens = capacity;
            _lastRefill = DateTime.UtcNow;
        }

        public bool TryConsume()
        {
            lock (_lock)
            {
                Refill();
                
                if (_tokens >= 1)
                {
                    _tokens -= 1;
                    return true;
                }
                
                return false;
            }
        }

        private void Refill()
        {
            var now = DateTime.UtcNow;
            var elapsed = (now - _lastRefill).TotalSeconds;
            
            if (elapsed > 0)
            {
                var tokensToAdd = elapsed * _refillRate;
                _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
                _lastRefill = now;
            }
        }
    }
}
